

using CommunityToolkit.Maui.Views;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
//using Microsoft.UI.Xaml;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Xml.Linq;
using UsFrameApp.Constants;
using UsFrameApp.Popups;
using UsFrameApp.Services;
using UsFrameApp.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UsFrameApp.View
{
    [QueryProperty(nameof(RoomKey), "roomKey")]
    public partial class RoomPage : ContentPage
    {
        public event EventHandler? EmojiTapped;
        private readonly RoomViewModel _viewModel;
        private readonly WebRtcBridgeService _webRtcBridge;
        private bool _htmlLoaded = false;
        private bool _initialized = false;
        private bool _webViewConfigured = false;
        private bool _isNavigating;
        private bool _speakerOpen;
        private bool _isCopying;
        bool _lowConnectionShown;
        bool _handlingInternetLoss;
        bool _noInternetPopupVisible;


        public string? RoomKey { get; set; }
        private const string RoomLinkEndpoint = "room";
        private Label InfoLabelControl => this.FindByName<Label>("InfoLabel");




        public RoomPage(RoomViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            BindingContext = _viewModel;
            _webRtcBridge = new WebRtcBridgeService();
            //InfoLabelControl.Text = "Base:  (unknown)  Key: (none)";
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
                        //await DisplayAlertAsync("Room Not Ready", "Room information is not available. Please create or open a room first.", "OK");
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

            _lowConnectionShown = false;
            _handlingInternetLoss = false;
            _noInternetPopupVisible = false;

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


        //Web navigation completed


        private async void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
        {
            if (_htmlLoaded)
                return;

            _htmlLoaded = true;

            await Task.Delay(500);

            if (WebRtcWebView == null || _viewModel?.Room == null)
                return;

            var roomKey = _viewModel.Room.Key?.Replace("'", "\\'") ?? string.Empty;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    WebRtcWebView.Eval($"startCall('{roomKey}')");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebRTC] startCall error: {ex}");
                }
            });

            //  Apply self view ONLY after call starts
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


        //Internet connection

        private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (!IsVisible)
                return;

            // ❌ NO INTERNET → popup + home
            if (e.NetworkAccess != NetworkAccess.Internet)
            {
                if (_handlingInternetLoss)
                    return;

                _handlingInternetLoss = true;
                _noInternetPopupVisible = true;

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

                return;
            }

            // ⚠️ LOW CONNECTION → overlay only
            var profiles = Connectivity.Current.ConnectionProfiles;

            bool isLowConnection =
                profiles.Contains(ConnectionProfile.Cellular) ||
                profiles.Contains(ConnectionProfile.Bluetooth) ||
                profiles.Contains(ConnectionProfile.Unknown);

            if (isLowConnection)
            {
                if (_lowConnectionShown)
                    return;

                _lowConnectionShown = true;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LowConnectionOverlayControl?.Show();


                });
            }
            else
            {
                // ✅ internet stable again
                _lowConnectionShown = false;
                _handlingInternetLoss = false;
                _noInternetPopupVisible = false;
            }
        }


        //private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        //{
        //    if (e.NetworkAccess == NetworkAccess.Internet)
        //        return;

        //    if (_handlingInternetLoss)
        //        return;

        //    _handlingInternetLoss = true;

        //await MainThread.InvokeOnMainThreadAsync(async () =>
        //    {
        //    await Application.Current.MainPage.ShowPopupAsync(
        //        new AppAlertPopup(
        //            "Not Connected",
        //            "Internet connection lost"
        //        )
        //    );

        //    ForceNavigateToHome();
        //});
        //}


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

                CopiedPopup?.Show();

                // Brief delay so the user can read the confirmation
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


        //private async void OnCopyClicked(object? sender, EventArgs e)
        //{
        //    if (_isCopying) return;
        //    _isCopying = true;

        //    try
        //    {
        //        if (_viewModel?.Room == null) return;

        //        var baseUrl = ApiEndpoints.BaseUrl?.TrimEnd('/') ?? string.Empty;
        //        var roomKey = _viewModel.Room.Key ?? string.Empty;
        //        var shareUrl = $"{baseUrl}/{RoomLinkEndpoint}/{Uri.EscapeDataString(roomKey)}";

        //        await Clipboard.Default.SetTextAsync(shareUrl);

        //        await Application.Current.MainPage
        //            .ShowPopupAsync(new CopiedPopup());

        //        await Share.RequestAsync(new ShareTextRequest
        //        {
        //            Text = shareUrl,
        //            Title = "Share"
        //        });
        //    }
        //    finally
        //    {
        //        await Task.Delay(300);
        //        _isCopying = false;
        //    }
        //}

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


        //public void ToggleCamera(bool isOff)
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        try { WebRtcWebView?.Eval($"toggleCamera({isOff.ToString().ToLower()})"); }
        //        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WebRTC] ToggleCamera Eval error: {ex}"); }
        //    });
        //}




        //public void ToggleMic(bool isMuted)
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        try { WebRtcWebView?.Eval($"toggleMic({isMuted.ToString().ToLower()})"); }
        //        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WebRTC] ToggleMic Eval error: {ex}"); }
        //    });
        //}

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


        //QR CODE  
        //private async void OnQrTapped(object sender, TappedEventArgs e)
        //{
        //    var link = LinkEntry.Text?.Trim();

        //    if (string.IsNullOrEmpty(link))
        //        return;

        //    await Application.Current.MainPage
        //        .ShowPopupAsync(new QrPopup(link));
        //}

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

                if (Navigation.NavigationStack.Any(p => p is SettingsPage))
                    return;

                await Navigation.PushAsync(
    new SettingsPage(GetRoomShareUrl())
);

                //await Navigation.PushAsync(new SettingsPage());
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

        // ================= EVENTS =================
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

        //Mic and Video icon code
        bool _micMuted;

        private void OnMicTapped(object sender, EventArgs e)
        {
            _micMuted = !_micMuted;

            MicSlash.IsVisible = _micMuted;
            MicBorder.BackgroundColor = _micMuted
                ? Colors.Red
                : Color.FromArgb("#146CFF");

            // 🔑 TALK TO WEBRTC
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

            // CAMERA  ON/OFF
            ToggleCamera(_videoOff);

            // SELF VIEW STATE
            ApplySelfViewFromSettings();
        }


        //Switch camera code

        private void OnCameraSwitchTapped(object sender, EventArgs e)
        {
            //SwitchCamera();
        }

        //public void SwitchCamera()
        //{
        //    LocalCameraView?.SwitchCamera();
        //}

        private void HideLeaveOverlay()
        {
            FreezeLayer.IsVisible = false;
            LeaveRoomHost.IsVisible = false;

            LeaveRoomView.Cancelled -= OnLeaveCancelled;
            LeaveRoomView.Confirmed -= OnLeaveConfirmed;
        }


        private async void OnLeaveConfirmed()
        {
            HideLeaveOverlay();

            try
            {
                // Stop WebRTC cleanly
                EndCall();

                // Get current room key
                var roomKey = _viewModel?.Room?.Key;

                if (string.IsNullOrWhiteSpace(roomKey))
                {
                    await Shell.Current.GoToAsync("//Home");
                    return;
                }

                await Shell.Current.GoToAsync(
                    $"{nameof(JoinAgainPage)}?roomKey={Uri.EscapeDataString(roomKey)}"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LeaveRoom] {ex}");
                await Shell.Current.GoToAsync("//Home");
            }
        }

        private LowConnectionPopup? LowConnectionOverlayControl
    => this.FindByName<LowConnectionPopup>("LowConnectionOverlay");

        private void OnLeaveCancelled()
        {
            HideLeaveOverlay();
        }


        private void OnLeaveRoomClicked(object sender, EventArgs e)
        {
            FreezeLayer.IsVisible = true;
            LeaveRoomHost.IsVisible = true;

            // prevent duplicate subscriptions
            LeaveRoomView.Cancelled -= OnLeaveCancelled;
            LeaveRoomView.Confirmed -= OnLeaveConfirmed;

            LeaveRoomView.Cancelled += OnLeaveCancelled;
            LeaveRoomView.Confirmed += OnLeaveConfirmed;
        }
    }

}
