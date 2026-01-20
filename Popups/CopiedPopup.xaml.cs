using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core;

namespace UsFrameApp.Popups;

public partial class CopiedPopup : Popup
{
    // used to cancel auto close when popup closes early
    private CancellationTokenSource _cts = new();

    public CopiedPopup()
    {
        InitializeComponent();

        // handle popup close event
        Closed += OnPopupClosed;

        // start auto close timer
        StartAutoClose();
    }

    // automatically close popup after delay
    private async void StartAutoClose()
    {
        try
        {
            await Task.Delay(1200, _cts.Token);
            Close();
        }
        catch (TaskCanceledException)
        {
            // ignore cancellation
        }
    }

    // cleanup when popup is closed
    private void OnPopupClosed(object? sender, PopupClosedEventArgs e)
    {
        _cts.Cancel();
        Closed -= OnPopupClosed;
    }
}
