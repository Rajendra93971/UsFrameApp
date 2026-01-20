using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Graphics;

namespace UsFrameApp;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize,
    SupportsPictureInPicture = true,
    ResizeableActivity = true,
    LaunchMode = LaunchMode.SingleTask)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        ApplyStatusBar();
        HandleDeepLink(Intent);
    }

    protected override void OnResume()
    {
        base.OnResume();
        ApplyStatusBar();
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        if (intent != null)
            HandleDeepLink(intent);
    }

    private void HandleDeepLink(Intent intent)
    {
        var data = intent.Data;
        if (data == null)
            return;

        // https://usframe.com/room/{roomKey}
        var segments = data.PathSegments;
        if (segments == null || segments.Count < 2)
            return;

        // match "/room/{key}"
        if (segments[0].Equals("room", StringComparison.OrdinalIgnoreCase))
        {
            var roomKey = segments[1];

            if (!string.IsNullOrWhiteSpace(roomKey))
            {
                // store room key for App.xaml.cs to consume
                App.PendingRoomKey = roomKey;
            }
        }
    }

    private void ApplyStatusBar()
    {
        Window.ClearFlags(WindowManagerFlags.Fullscreen);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            Window.SetDecorFitsSystemWindows(true);
            Window.InsetsController?.Show(WindowInsets.Type.StatusBars());
        }

        Window.SetStatusBarColor(Android.Graphics.Color.White);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            Window.InsetsController?.SetSystemBarsAppearance(
                (int)WindowInsetsControllerAppearance.LightStatusBars,
                (int)WindowInsetsControllerAppearance.LightStatusBars
            );
        }
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            Window.DecorView.SystemUiVisibility =
                (StatusBarVisibility)SystemUiFlags.LightStatusBar;
        }
    }

    // WebRTC permissions auto-allow
    public class WebRtcWebChromeClient : WebChromeClient
    {
        public override void OnPermissionRequest(PermissionRequest request)
        {
            request.Grant(request.GetResources());
        }
    }
}
