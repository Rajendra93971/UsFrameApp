using CommunityToolkit.Maui.Views;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Xml.Linq;
using UsFrameApp.Constants;
using UsFrameApp.Popups;
using UsFrameApp.Services;
using UsFrameApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace UsFrameApp.View
{
	[QueryProperty(nameof(RoomKey), "roomKey")]
	public partial class RoomPage : ContentPage
	{
		public event EventHandler? EmojiTapped;
		private readonly RoomViewModel _viewModel;
		private readonly WebRtcBridgeService _webRtcBridge;
		private bool _noInternetPopupVisible;
		private bool _htmlLoaded = false;
		private bool _initialized = false;
		private bool _webViewConfigured = false;
		private bool _isNavigating;
		private bool _speakerOpen;
		private bool _isCopying;
		private bool _handlingInternetLoss;

		public string? RoomKey { get; set; }
		private const string RoomLinkEndpoint = "room";
		private Label InfoLabelControl => this.FindByName<Label>("InfoLabel");

		public RoomPage(RoomViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			BindingContext = _viewModel;
			_webRtcBridge = new WebRtcBridgeService();
			InfoLabelControl.Text = "Base:  (unknown)  Key: (none)";

            try
            {
                var footer = this.FindByName<UsFrameApp.Controls.FooterPanel>("FooterPanel");
                if (footer != null)
                {
                    footer.LocalMicTapped += (s, e) => OnMicTapped(s, e);
                    footer.LocalVideoTapped += (s, e) => OnVideoTapped(s, e);
                    footer.RemoteMicTapped += (s, e) => OnMicTapped(s, e);
                    footer.RemoteVideoTapped += (s, e) => OnVideoTapped(s, e);
                    footer.SpeakerTapped += (s, e) => OnSpeakerTapped(s, new TappedEventArgs(s));
                    footer.LeaveClicked += async (s, e) => OnLeaveRoomClicked(s, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPage] Footer wiring error: {ex}");
            }
        }

		private void ForceNavigateToHome()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				try
				{
					Connectivity.ConnectivityChanged -= OnConnectivityChanged;

					var homePage = App.Services.GetRequiredService<HomePage>();
					Application.Current.MainPage = new NavigationPage(homePage);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[ForceNavigateToHome] {ex}");
				}
			});
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			Connectivity.ConnectivityChanged -= OnConnectivityChanged;
			Connectivity.ConnectivityChanged += OnConnectivityChanged;

			if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
			{
				await Application.Current.MainPage.ShowPopupAsync(
					new AppAlertPopup(
						"Not Connected",
						"Internet connection required"
					)
				);

				ForceNavigateToHome();
				return;
			}

			if (_initialized)
				return;

			bool success = false;

			bool permissionGranted = await RequestCameraMicPermissionAsync();
			if (!permissionGranted)
				return;

			try
			{
				if (_viewModel?.Room == null)
				{
					if (string.IsNullOrWhiteSpace(RoomKey))
					{
						await DisplayAlertAsync("Room Not Ready", "Room information is not available. Please create or open a room first.", "OK");
						return;
					}
					await _viewModel.InitializeAsync(RoomKey!);
				}

				UpdateInfoHeader();

				MainThread.BeginInvokeOnMainThread(() =>
				{
					try { ConfigureWebView(); }
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"[RoomPage] ConfigureWebView error: {ex}");
					}
				});

				success = true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[RoomPage] InitializeAsync error: {ex}");
				try { await DisplayAlertAsync("Error", "Failed to join room: " + ex.Message, "OK"); } catch { }
			}
			finally
			{
				_initialized = success;
			}
		}
		protected override void OnDisappearing()

		{
			base.OnDisappearing();
			Connectivity.ConnectivityChanged -= OnConnectivityChanged;

			try
			{
				if (_webViewConfigured && WebRtcWebView != null)
				{
					WebRtcWebView.Navigated -= OnWebViewNavigated;
					WebRtcWebView.Navigating -= OnWebViewNavigating;
					_webViewConfigured = false;
					_htmlLoaded = false;
				}
			}
			catch { }

			_initialized = false;
		}


		private async Task<bool> RequestCameraMicPermissionAsync()
		{
			var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
			var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

			if (cameraStatus != PermissionStatus.Granted || micStatus != PermissionStatus.Granted)
			{
				await DisplayAlertAsync("Permission Required", "Camera and microphone access is required to join the video call.", "OK");
				return false;
			}
			return true;
		}

		private void ConfigureWebView()
		{
			if (_webViewConfigured) return;
			if (WebRtcWebView == null)
				throw new InvalidOperationException("WebRtcWebView is not available");

			WebRtcWebView.Source = new UrlWebViewSource { Url = "webrtc.html" };
			WebRtcWebView.Navigated += OnWebViewNavigated;
			WebRtcWebView.Navigating += OnWebViewNavigating;
			_webViewConfigured = true;
		}


		private async void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
		{
			if (_htmlLoaded)
				return;

			_htmlLoaded = true;

			await Task.Delay(500);

			if (WebRtcWebView == null || _viewModel?.Room == null)
				return;

			var roomKey = _viewModel.Room.Key?.Replace("'", "\\'") ?? string.Empty;

			bool isSelfView = Preferences.Get("SelfView", false);

			MainThread.BeginInvokeOnMainThread(() =>
			{
				try
				{
					WebRtcWebView.Eval($"startCall('{roomKey}', {isSelfView.ToString().ToLower()})");
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[WebRTC] startCall error: {ex}");
				}
			});

			await Task.Delay(800);
			ApplySelfViewFromSettings();
		}

		private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
		{
			if (e == null || string.IsNullOrWhiteSpace(e.Url)) return;
			if (!e.Url.StartsWith("app://", StringComparison.OrdinalIgnoreCase)) return;

			e.Cancel = true;
			try
			{
				string encoded = e.Url.Substring("app://".Length);
				string json = Uri.UnescapeDataString(encoded);
				if (!json.TrimStart().StartsWith("{")) return;
				_webRtcBridge.HandleJsMessage(json);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[WebRTC][JS ERROR] {ex}");
			}
		}

		private async void OnQrTapped(object sender, EventArgs e)
		{
			if (_viewModel?.Room == null) return;

			var baseUrl = ApiEndpoints.BaseUrl?.TrimEnd('/') ?? string.Empty;
			var roomKey = _viewModel.Room.Key ?? string.Empty;
			var link = $"{baseUrl}/{RoomLinkEndpoint}/{Uri.EscapeDataString(roomKey)}";

			await Application.Current.MainPage
				.ShowPopupAsync(new QrPopup(link));
		}

		private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
		{
			if (e.NetworkAccess == NetworkAccess.Internet)
				return;

			if (_handlingInternetLoss)
				return;

			_handlingInternetLoss = true;

			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				await Application.Current.MainPage.ShowPopupAsync(
					new AppAlertPopup(
						"Not Connected",
						"Internet connection lost"
					)
				);

				ForceNavigateToHome();
			});
		}
		// Copy icon
		private string GetRoomShareUrl()
		{
			if (_viewModel?.Room == null)
				return string.Empty;

			var baseUrl = ApiEndpoints.BaseUrl?.TrimEnd('/') ?? string.Empty;
			var roomKey = _viewModel.Room.Key ?? string.Empty;

			return $"{baseUrl}/{RoomLinkEndpoint}/{Uri.EscapeDataString(roomKey)}";
		}

		private async void OnCopyClicked(object? sender, EventArgs e)
		{
			if (_isCopying) return;
			_isCopying = true;

			try
			{
				var shareUrl = GetRoomShareUrl();
				if (string.IsNullOrEmpty(shareUrl))
					return;

				await Clipboard.Default.SetTextAsync(shareUrl);

				try
				{
					await Application.Current.MainPage.ShowPopupAsync(new CopiedPopup());
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[RoomPage] ShowPopupAsync error: {ex}");
				}

				await Task.Delay(300);

				await Share.RequestAsync(new ShareTextRequest
				{
					Text = shareUrl,
					Title = "Share"
				});
			}
			finally
			{
				await Task.Delay(300);
				_isCopying = false;
			}
		}
		public void ToggleCamera(bool isOff)
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				try
				{
					WebRtcWebView?.Eval($"toggleCamera({isOff.ToString().ToLower()})");
				}
				catch (Exception ex)
				{
				 System.Diagnostics.Debug.WriteLine($"[WebRTC] ToggleCamera Eval error: {ex}");
				}
			});
		}

		public void ToggleMic(bool isMuted)
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				try
				{
					WebRtcWebView?.Eval($"toggleMic({isMuted.ToString().ToLower()})");
				}
				catch (Exception ex)
				{
				 System.Diagnostics.Debug.WriteLine($"[WebRTC] ToggleMic Eval error: {ex}");
				}
			});
		}

		public void EndCall()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				try { WebRtcWebView?.Eval("endCall()"); }
				catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WebRTC] EndCall Eval error: {ex}"); }
			});
		}
		private void UpdateInfoHeader()
		{
			try
			{
				InfoLabel.Text = GetRoomShareUrl();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[RoomPage] UpdateInfoHeader error: {ex}");
			}
		}

		private Task DisplayAlertAsync(string title, string message, string cancel)
			=> base.DisplayAlert(title, message, cancel);

        //Setting 
        private async void OnSettingsClicked(object sender, TappedEventArgs e)
        {
            if (_isNavigating)
                return;

            _isNavigating = true;

            try
            {
                if (sender is VisualElement view)
                {
                    view.InputTransparent = true;
                    await view.ScaleTo(0.9, 60);
                    await view.ScaleTo(1, 60);
                }

                var panel = this.FindByName<UsFrameApp.Popups.SettingsPanel>("SettingsPanel");
                if (panel != null)
                {
                    try
                    {
                        if (panel.IsVisible)
                            await panel.HideAsync();
                        else
                            await panel.ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[RoomPage] SettingsPanel show/hide error: {ex}");
                    }
                }
                else
                {

                    if (!Navigation.NavigationStack.Any(p => p is SettingsPage))
                        await Navigation.PushAsync(new SettingsPage(GetRoomShareUrl()));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPage] Settings navigation error: {ex}");
            }
            finally
            {
                if (sender is VisualElement view)
                    view.InputTransparent = false;

                _isNavigating = false;
            }
        }

        //  EVENTS 
        private void OnEmojiIconTapped(object sender, EventArgs e)
		{
			EmojiOverlay.TogglePicker();
		}
		private async void OnSpeakerTapped(object sender, TappedEventArgs e)
		{
			if (_speakerOpen) return;
			_speakerOpen = true;

			try
			{
				if (sender is VisualElement v)
				{
					await v.ScaleTo(0.9, 60);
					await v.ScaleTo(1, 60);
				}

				SpeakerOverlay.Show();
			}
			finally
			{
				await Task.Delay(200);
				_speakerOpen = false;
			}
		}

		//Toggle local video
		public void ApplySelfViewFromSettings()
		{
			bool isOn = Preferences.Get("SelfView", false);

			MainThread.BeginInvokeOnMainThread(() =>
			{
				try
				{
					WebRtcWebView?.Eval($"setSelfViewEnabled({isOn.ToString().ToLower()})");
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[WebRTC] SelfView apply error: {ex}");
				}
			});
		}

		//Mic and Video icon 
		bool _micMuted;

		private void OnMicTapped(object sender, EventArgs e)
		{
			_micMuted = !_micMuted;

			MicSlash.IsVisible = _micMuted;
			MicBorder.BackgroundColor = _micMuted
				? Colors.Red
				: Color.FromArgb("#146CFF");

			ToggleMic(_micMuted);
		}

		//video icon code
		bool _videoOff;

		private void OnVideoTapped(object sender, EventArgs e)
		{
			_videoOff = !_videoOff;

			VideoSlash.IsVisible = _videoOff;
			VideoBorder.BackgroundColor = _videoOff
				? Colors.Red
				: Color.FromArgb("#146CFF");

			ToggleCamera(_videoOff);
			ApplySelfViewFromSettings();
		}

		//Switch camera code
		public void SwitchCamera()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				try
				{
					WebRtcWebView?.Eval("switchCamera()");
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[WebRTC] SwitchCamera Eval error: {ex}");
				}
			});
		}

		private void OnCameraSwitchTapped(object sender, EventArgs e)
		{
			// call JS switch camera
			SwitchCamera();
		}

		// Leave flow
		private LeaveRoomOverlay? _activeLeaveOverlay;

        private void HideLeaveOverlay()
        {
            try
            {
                // hide freeze layer and host (only header+content rows)
                var freeze = this.FindByName<Microsoft.Maui.Controls.BoxView>("FreezeLayer");
                if (freeze != null)
                    freeze.IsVisible = false;

                var host = this.FindByName<Microsoft.Maui.Controls.Grid>("LeaveRoomHost");
                if (host != null)
                    host.IsVisible = false;

                if (_activeLeaveOverlay != null)
                {
                    _activeLeaveOverlay.Cancelled -= OnLeaveCancelled;
                    _activeLeaveOverlay.Confirmed -= OnLeaveConfirmed;

                    // remove overlay from host if present
                    if (host != null)
                    {
                        try { host.Children.Clear(); } catch { }
                    }

                    _activeLeaveOverlay = null;
                }
            }
            catch { }
        }

        private void OnLeaveRoomClicked(object sender, EventArgs e)
        {
            try
            {
                // show freeze layer (over header+content only)
                var freeze = this.FindByName<Microsoft.Maui.Controls.BoxView>("FreezeLayer");
                if (freeze != null) freeze.IsVisible = true;

                // create a fresh overlay instance and wire events
                var overlay = new LeaveRoomOverlay();

                // defensive unsubscribe then subscribe
                overlay.Cancelled -= OnLeaveCancelled;
                overlay.Confirmed -= OnLeaveConfirmed;

                overlay.Cancelled += OnLeaveCancelled;
                overlay.Confirmed += OnLeaveConfirmed;

                var host = this.FindByName<Microsoft.Maui.Controls.Grid>("LeaveRoomHost");
                if (host != null)
                {
                    // center overlay using correct LayoutOptions type
                    overlay.HorizontalOptions = LayoutOptions.Center;
                    overlay.VerticalOptions = LayoutOptions.Center;

                    // reasonable max width for phones/tablets
                    try
                    {
                        var pageWidth = (double)(Application.Current?.MainPage?.Width ?? 420.0);
                        overlay.WidthRequest = Math.Min(520, pageWidth * 0.9);
                    }
                    catch { }

                    host.Children.Clear();
                    host.Children.Add(overlay);
                    host.IsVisible = true;
                }

                _activeLeaveOverlay = overlay;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RoomPage] OnLeaveRoomClicked error: {ex}");
            }
        }

        private void OnLeaveConfirmed()
        {
            // Close overlay immediately
            HideLeaveOverlay();

            // Navigate to home on UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var homePage = App.Services.GetRequiredService<HomePage>();
                    Application.Current.MainPage = new NavigationPage(homePage);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RoomPage] OnLeaveConfirmed navigation error: {ex}");
                }
            });

            // Then show rejoin popup
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var result = await Application.Current.MainPage.ShowPopupAsync(new Popups.RejoinPopup());
                    if (result is string s && s == "rejoin")
                    {
                        try
                        {
                            var rp = App.Services.GetRequiredService<RoomPage>();
                            rp.RoomKey = RoomKey;
                            Application.Current.MainPage = new NavigationPage(rp);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[RoomPage] Rejoin navigation error: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RoomPage] RejoinPopup error: {ex}");
                }
            });
        }

        private void OnLeaveCancelled()
        {
            HideLeaveOverlay();
        }
	}
}
