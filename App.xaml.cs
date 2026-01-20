using Microsoft.Maui.Controls;
using System;
using UsFrameApp.View;

namespace UsFrameApp;

public partial class App : Application
{
    // stores raw deep link path
    public static string? DeepLink { get; private set; }

    // holds room key received from deep link
    public static string? PendingRoomKey { get; set; }

    // global service provider access
    public static IServiceProvider Services { get; private set; } = null!;

    public App(IServiceProvider services)
    {
        InitializeComponent();

        // register global crash handlers
        RegisterGlobalExceptionHandlers();

        Services = services;

        // initial app page
        MainPage = new NavigationPage(
            Services.GetRequiredService<SplashPage>()
        );
    }

    private void RegisterGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            // optional logging
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            e.SetObserved();
        };
    }

    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        DeepLink = uri.AbsolutePath;

        // extract room key from deep link
        if (!string.IsNullOrWhiteSpace(DeepLink) &&
            DeepLink.StartsWith("/room/", StringComparison.OrdinalIgnoreCase))
        {
            PendingRoomKey = DeepLink.Replace("/room/", string.Empty).Trim();
        }
    }

    protected override void OnStart()
    {
        base.OnStart();

        // handle deep link on cold start
        HandlePendingDeepLink();
    }

    protected override void OnResume()
    {
        base.OnResume();

        // handle deep link on resume
        HandlePendingDeepLink();
    }

    private async void HandlePendingDeepLink()
    {
        if (string.IsNullOrWhiteSpace(PendingRoomKey))
            return;

        // consume room key only once
        var roomKey = PendingRoomKey;
        PendingRoomKey = null;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // navigate directly to room page
            await Shell.Current.GoToAsync(
                $"{nameof(View.RoomPage)}?roomKey={Uri.EscapeDataString(roomKey)}"
            );
        });
    }
}
