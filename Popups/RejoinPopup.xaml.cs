using CommunityToolkit.Maui.Views;

namespace UsFrameApp.Popups;

public partial class RejoinPopup : Popup
{
    public RejoinPopup()
    {
        InitializeComponent();
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close("cancel");
    }

    private void OnRejoinClicked(object sender, EventArgs e)
    {
        Close("rejoin");
    }
}