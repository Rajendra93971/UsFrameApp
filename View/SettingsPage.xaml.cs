using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using UsFrameApp.Popups;
using Microsoft.Maui.Storage;
using UsFrameApp.View;
using Microsoft.Extensions.DependencyInjection;

namespace UsFrameApp.View;

public partial class SettingsPage : ContentPage
{
    private readonly string _roomShareUrl;
    private bool _noInternetPopupVisible;

    public SettingsPage(string roomShareUrl)
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        _roomShareUrl = roomShareUrl ?? string.Empty;
        InfoLabel.Text = _roomShareUrl;

        // initialize toggle states from Preferences
        SelfViewSwitch.IsToggled = Preferences.Get("SelfView", false);
        DebugSwitch.IsToggled = Preferences.Get("DebugMode", false);
        ExpandSwitch.IsToggled = Preferences.Get("ExpandVideo", false);
        NightSwitch.IsToggled = Preferences.Get("NightMode", false);

        // initial visuals
        UpdateControlButtonVisuals();

        try
        {
            var footer = this.FindByName<UsFrameApp.Controls.FooterPanel>("FooterPanel");
            if (footer != null)
            {
                footer.LocalMicTapped += (s, e) => OnMicTapped(s, e);
                footer.LocalVideoTapped += (s, e) => OnVideoTapped(s, e);
                footer.RemoteMicTapped += (s, e) => OnMicTapped(s, e);
                footer.RemoteVideoTapped += (s, e) => OnVideoTapped(s, e);
                footer.SpeakerTapped += (s, e) => OnSpeakerTapped(s, e);
                footer.LeaveClicked += async (s, e) => OnLeaveRoomClicked(s, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsPage] Footer wiring error: {ex}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_roomShareUrl))
            return;

        await Clipboard.Default.SetTextAsync(_roomShareUrl);
        try { await Application.Current.MainPage.ShowPopupAsync(new CopiedPopup()); } catch { }
    }

    private async void OnQrTapped(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_roomShareUrl)) return;
        await Application.Current.MainPage.ShowPopupAsync(new QrPopup(_roomShareUrl));
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        // close settings: pop page
        try
        {
            if (Navigation.NavigationStack.Count > 1)
                Navigation.PopAsync();
        }
        catch { }
    }

    private void OnSelfViewSwitchToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("SelfView", e.Value);
        var roomPage = Navigation.NavigationStack.OfType<RoomPage>().LastOrDefault();
        roomPage?.ApplySelfViewFromSettings();
    }

    private void OnDebugSwitchToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("DebugMode", e.Value);
    }

    private void OnExpandSwitchToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("ExpandVideo", e.Value);
        var roomPage = Navigation.NavigationStack.OfType<RoomPage>().LastOrDefault();
        roomPage?.ApplySelfViewFromSettings();
    }

    private void OnNightSwitchToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("NightMode", e.Value);
        // Night mode behavior handled elsewhere
    }

    // Mic & video tap - toggle local button visuals and talk to RoomPage
    private bool _micMuted;
    private bool _videoOff;

    private void OnMicTapped(object sender, EventArgs e)
    {
        _micMuted = !_micMuted;
        UpdateControlButtonVisuals();
        var roomPage = Navigation.NavigationStack.OfType<RoomPage>().LastOrDefault();
        roomPage?.ToggleMic(_micMuted);
    }

    private void OnVideoTapped(object sender, EventArgs e)
    {
        _videoOff = !_videoOff;
        UpdateControlButtonVisuals();
        var roomPage = Navigation.NavigationStack.OfType<RoomPage>().LastOrDefault();
        roomPage?.ToggleCamera(_videoOff);
        roomPage?.ApplySelfViewFromSettings();
    }

    private void UpdateControlButtonVisuals()
    {
        // Mic
        if (_micMuted)
        {
            MicBorder.BackgroundColor = Colors.Red;
        }
        else
        {
            MicBorder.BackgroundColor = Color.FromArgb("#146CFF");
        }

        // Video
        if (_videoOff)
        {
            VideoBorder.BackgroundColor = Colors.Red;
        }
        else
        {
            VideoBorder.BackgroundColor = Color.FromArgb("#146CFF");
        }
    }

    private void OnCameraSwitchTapped(object sender, EventArgs e)
    {
        var roomPage = Navigation.NavigationStack.OfType<RoomPage>().LastOrDefault();
        roomPage?.SwitchCamera();
    }

    private void OnSpeakerTapped(object sender, EventArgs e)
    {
        var speaker = this.FindByName<SpeakerVolumeOverlay>("SpeakerOverlay");
        speaker?.Show();
    }

    private void OnLeaveRoomClicked(object sender, EventArgs e)
    {
        // show leave overlay dynamically if present
        var host = this.FindByName<Grid>("LeaveRoomHost");
        if (host != null)
        {
            var overlay = new LeaveRoomOverlay();
            overlay.Cancelled -= OnLeaveCancelled;
            overlay.Confirmed -= OnLeaveConfirmed;
            overlay.Cancelled += OnLeaveCancelled;
            overlay.Confirmed += OnLeaveConfirmed;

            host.Children.Clear();
            host.Children.Add(overlay);
            host.IsVisible = true;

            var freeze = this.FindByName<BoxView>("FreezeLayer");
            if (freeze != null) freeze.IsVisible = true;
        }
        else
        {
            var home = App.Services.GetRequiredService<HomePage>();
            Application.Current.MainPage = new NavigationPage(home);
        }
    }

    private void OnLeaveConfirmed()
    {
        var home = App.Services.GetRequiredService<HomePage>();
        Application.Current.MainPage = new NavigationPage(home);
    }

    private void OnLeaveCancelled()
    {
        var host = this.FindByName<Grid>("LeaveRoomHost");
        if (host != null)
        {
            host.Children.Clear();
            host.IsVisible = false;
        }
        var freeze = this.FindByName<BoxView>("FreezeLayer");
        if (freeze != null) freeze.IsVisible = false;
    }

    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            if (_noInternetPopupVisible) return;
            _noInternetPopupVisible = true;
            await Application.Current.MainPage.ShowPopupAsync(new AppAlertPopup("Not Connected", "Internet connection required"));
        }
        else
        {
            _noInternetPopupVisible = false;
        }
    }
}
