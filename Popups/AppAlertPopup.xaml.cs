using CommunityToolkit.Maui.Views;

namespace UsFrameApp.Popups;

public partial class AppAlertPopup : Popup
{
    // popup with title and ok button
    public AppAlertPopup(string title, string message)
    {
        InitializeComponent();

        AlertBorder.IsVisible = true;
        TitleLabel.IsVisible = true;
        OkButton.IsVisible = true;

        TitleLabel.Text = title;
        MessageLabel.Text = message;
    }

    // popup without title, auto close option
    public AppAlertPopup(string message, bool autoClose)
    {
        InitializeComponent();

        AlertBorder.IsVisible = true;
        TitleLabel.IsVisible = false;
        OkButton.IsVisible = false;

        MessageLabel.Text = message;

        if (autoClose)
            AutoClose();
    }

    // hidden popup used for volume overlay
    public static AppAlertPopup CreateVolumePopup()
    {
        var popup = new AppAlertPopup(string.Empty, string.Empty);
        popup.AlertBorder.IsVisible = false;
        return popup;
    }

    // auto close popup after delay
    private async void AutoClose()
    {
        await Task.Delay(1500);
        Close();
    }

    // ok button click handler
    private void OnOkClicked(object sender, EventArgs e)
    {
        Close();
    }

    // system volume change (android only)
    private void OnVolumeChanged(object sender, ValueChangedEventArgs e)
    {
#if ANDROID
        var audioManager = Android.App.Application.Context
            .GetSystemService(Android.Content.Context.AudioService)
            as Android.Media.AudioManager;

        if (audioManager == null)
            return;

        int max = audioManager.GetStreamMaxVolume(Android.Media.Stream.Music);
        int volume = (int)(e.NewValue * max);

        audioManager.SetStreamVolume(
            Android.Media.Stream.Music,
            volume,
            Android.Media.VolumeNotificationFlags.ShowUi);
#endif
    }
}
