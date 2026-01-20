namespace UsFrameApp.View;

public partial class JoinAgainPage : ContentPage
{
    public JoinAgainPage()
    {
        InitializeComponent();
    }

    private async void OnJoinAgainClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ConnectingPage));
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Home");
    }

}
