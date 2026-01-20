using UsFrameApp.Services;

namespace UsFrameApp.Popups;

public partial class SpeakerVolumeOverlay : ContentView
{
    CancellationTokenSource? _cts;

    public SpeakerVolumeOverlay()
    {
        InitializeComponent();

        // set initial volume from app state
        VolumeSlider.Value = AppVolumeService.Volume * 100;
        VolumeText.Text = ((int)VolumeSlider.Value).ToString();
    }

    // show volume overlay
    public void Show()
    {
        IsVisible = true;
    }

    // hide overlay and cancel auto close
    public void Hide()
    {
        IsVisible = false;
        _cts?.Cancel();
    }

    // close when tapped outside
    private void OnOutsideTapped(object sender, EventArgs e)
    {
        Hide();
    }

    // update volume while sliding
    private void OnVolumeChanged(object sender, ValueChangedEventArgs e)
    {
        int value = (int)e.NewValue;

        VolumeText.Text = value.ToString();
        AppVolumeService.Volume = value / 100.0;

        RestartAutoClose();
    }

    // restart auto close after drag ends
    private void OnDragCompleted(object sender, EventArgs e)
    {
        RestartAutoClose();
    }

    // auto hide after delay
    private void RestartAutoClose()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2500, token);
                MainThread.BeginInvokeOnMainThread(Hide);
            }
            catch
            {
                // ignore cancel
            }
        });
    }
}
