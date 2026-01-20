using CommunityToolkit.Maui.Views;

namespace UsFrameApp.Popups;

public partial class QrPopup : Popup
{
    public QrPopup(string meetingLink)
    {
        InitializeComponent();

        // ensure no overlay dim
        this.Color = Microsoft.Maui.Graphics.Colors.Transparent;

        if (string.IsNullOrWhiteSpace(meetingLink))
            return;
#if ANDROID
        this.HandlerChanged += (s, e) =>
        {
            try
            {
                if (this.Handler?.PlatformView is Android.Views.View pv)
                {
                    var activity = pv.Context as Android.App.Activity;
                    activity?.Window?.ClearFlags(Android.Views.WindowManagerFlags.DimBehind);
                }
            }
            catch { }
        };
#endif

        var cleanLink = meetingLink.Trim();

        QrImage.Source = ImageSource.FromUri(
            new Uri(
                $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(cleanLink)}"
            )
        );

    }
    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}

