//using UsFrameApp.ViewModels;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Networking;
//using System.Windows.Input;
//using CommunityToolkit.Maui.Views;
//using UsFrameApp.Popups;

//namespace UsFrameApp.View;

//public partial class HomePage : ContentPage
//{
//    // start session button command
//    public ICommand StartSessionCommand { get; }

//    public HomePage(HomeViewModel vm)
//    {
//        InitializeComponent();

//        // bind start session action
//        StartSessionCommand = new Command(async () => await OnStartSession());
//        BindingContext = vm;

//#if ANDROID
//        // fullscreen immersive ui on android
//        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
//        if (activity != null)
//        {
//            var window = activity.Window;
//            window.SetStatusBarColor(Android.Graphics.Color.Transparent);

//            var decorView = window.DecorView;
//            decorView.SystemUiVisibility =
//                (Android.Views.StatusBarVisibility)(
//                    Android.Views.SystemUiFlags.LayoutStable |
//                    Android.Views.SystemUiFlags.LayoutFullscreen |
//                    Android.Views.SystemUiFlags.HideNavigation |
//                    Android.Views.SystemUiFlags.ImmersiveSticky
//                );
//        }
//#endif
//    }

//    // start video session flow
//    private async Task OnStartSession()
//    {
//        // check internet connectivity
//        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
//        {
//            await Application.Current.MainPage
//                .ShowPopupAsync(
//                    new AppAlertPopup("No Internet", "Please check your connection.")
//                );
//            return;
//        }

//        // navigate to connecting page
//        await Shell.Current.GoToAsync(nameof(ConnectingPage));
//    }
//}



using UsFrameApp.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using UsFrameApp.Popups;


namespace UsFrameApp.View;

public partial class HomePage : ContentPage
{
    public ICommand StartSessionCommand { get; }

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();

        // Initialize command
        StartSessionCommand = new Command(async () => await OnStartSession());
        BindingContext = vm;

#if ANDROID
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity != null)
        {
            var window = activity.Window;
            window.SetStatusBarColor(Android.Graphics.Color.Transparent);
            var decorView = window.DecorView;
            decorView.SystemUiVisibility =
                (Android.Views.StatusBarVisibility)(
                    Android.Views.SystemUiFlags.LayoutStable |
                    Android.Views.SystemUiFlags.LayoutFullscreen |
                    Android.Views.SystemUiFlags.HideNavigation |
                    Android.Views.SystemUiFlags.ImmersiveSticky
                );
        }
#endif

    }

    // Handle platform back button on Android to move the app into background

    protected override bool OnBackButtonPressed()
    {
#if ANDROID
        var activity = Platform.CurrentActivity;
        activity?.MoveTaskToBack(true);
        return true;
#else
    return base.OnBackButtonPressed();
#endif
    }

    //    protected override bool OnBackButtonPressed()
    //    {
    //#if ANDROID
    //        Android.App.Application.Context
    //            .GetSystemService(Android.Content.Context.ActivityService);

    //        var activity = Platform.CurrentActivity;
    //        activity?.MoveTaskToBack(true); 
    //        return true;
    //#else
    //        return base.OnBackButtonPressed();
    //#endif
    //    }

    // Start session logic and pre-navigation checks
    private async Task OnStartSession()
    {
        // Initial check
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) 
        {
            await Application.Current.MainPage.ShowPopupAsync(
                new AppAlertPopup("No Internet", "Please check your connection.")
            );

            // WAIT for internet instead of navigating
            await WaitForInternetAndProceed();
            return;
        }

        // Safe navigation
        await Shell.Current.GoToAsync(nameof(ConnectingPage));
    }

    //Wait for intwork connection

    private async Task WaitForInternetAndProceed()
    {
        TaskCompletionSource<bool> tcs = new();

        void ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                Connectivity.ConnectivityChanged -= ConnectivityChanged;
                tcs.TrySetResult(true);
            }
        }

        Connectivity.ConnectivityChanged += ConnectivityChanged;

        // Wait here until internet is back
        await tcs.Task;

        // Small delay to stabilize network
        await Task.Delay(500);

        // Now safe to navigate
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync(nameof(ConnectingPage));
        });
    }


}

//    private async Task OnStartSession()
//    {
//        // Check internet
//        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
//        {
//            await Application.Current.MainPage
//                .ShowPopupAsync(
//                    new AppAlertPopup("No Internet", "Please check your connection.")
//                );
//            return;
//        }
//        await Shell.Current.GoToAsync(nameof(ConnectingPage));
//    }
//}