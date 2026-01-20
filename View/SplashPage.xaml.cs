namespace UsFrameApp.View;

public partial class SplashPage : ContentPage
{
    // prevents multiple navigations
    bool _navigated;

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_navigated)
            return;

        _navigated = true;

        // splash delay
        await Task.Delay(2000);

        // navigate to home page
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Navigation.PushAsync(
                App.Services.GetRequiredService<HomePage>()
            );
        });
    }
}
