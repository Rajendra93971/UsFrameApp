using CommunityToolkit.Maui.Views;

namespace UsFrameApp.Popups;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
public partial class CopiedPopup : ContentView
{
    private CancellationTokenSource? _cts;

    public CopiedPopup()
    {
        InitializeComponent();
    }

    // Show the transient copied confirmation and restart the auto-close timer
    public void Show()
    {
        IsVisible = true;
        RestartAutoClose();
    }

    // Restart a background timer to automatically hide the popup
    private async void RestartAutoClose()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(1200, token);
            IsVisible = false;
        }
        catch (TaskCanceledException) { }
    }

}
