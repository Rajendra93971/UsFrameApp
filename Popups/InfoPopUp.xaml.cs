using CommunityToolkit.Maui.Views;

namespace UsFrameApp.Popups;

public partial class InfoPopUp : Popup
{
    // show info message and auto close
    public InfoPopUp(string message)
    {
        InitializeComponent();
        MessageLabel.Text = message;
        AutoClose();
    }

    // close popup after delay
    private async void AutoClose()
    {
        await Task.Delay(1500);
        Close();
    }
}
