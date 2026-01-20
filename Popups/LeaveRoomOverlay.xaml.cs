namespace UsFrameApp.Popups;

public partial class LeaveRoomOverlay : ContentView
{
    public event Action? Cancelled;
    public event Action? Confirmed;

    public LeaveRoomOverlay()
    {
        InitializeComponent();
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        // Notify subscribers that the leave action was cancelled
        Cancelled?.Invoke();
    }

    private void OnLeaveClicked(object sender, EventArgs e)
    {
        // Notify subscribers that the user confirmed leaving the room
        Confirmed?.Invoke();
    }
}
