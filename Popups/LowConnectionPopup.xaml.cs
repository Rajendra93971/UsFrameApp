namespace UsFrameApp.Popups;

public partial class LowConnectionPopup : ContentView
{
    CancellationTokenSource? _cts;

    public LowConnectionPopup()
    {
        InitializeComponent();
    }

    public void Show()
    {
        IsVisible = true;
        RestartTimer();
    }

    void RestartTimer()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(1500, token);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsVisible = false;
                });
            }
            catch { }
        });
    }
}
