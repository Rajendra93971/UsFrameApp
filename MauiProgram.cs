using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using UsFrameApp.View;
using UsFrameApp.ViewModels;
using UsFrameApp.Popups;
using UsFrameApp.Services;

#if ANDROID
using Android.Webkit;
#endif

namespace UsFrameApp
{
    public static class MauiProgram
    {
        // maui app configuration entry point
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // core services
            builder.Services.AddSingleton<SignalRService>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<WebRtcBridgeService>();

            // viewmodels
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddTransient<RoomViewModel>();

            // pages
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<RoomPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<SplashPage>();
            builder.Services.AddTransient<ConnectingPage>();

            // popups & overlays
            builder.Services.AddTransient<AppAlertPopup>();
            builder.Services.AddTransient<CopiedPopup>();
            builder.Services.AddTransient<EmojiOverlayView>();
            builder.Services.AddTransient<InfoPopUp>();
            builder.Services.AddTransient<QrPopup>();
            builder.Services.AddTransient<SpeakerVolumeOverlay>();

#if ANDROID
            // android webview config for webrtc
            WebViewHandler.Mapper.AppendToMapping("WebRTC", (handler, view) =>
            {
                if (handler.PlatformView is Android.Webkit.WebView androidWebView)
                {
                    androidWebView.Settings.JavaScriptEnabled = true;
                    androidWebView.Settings.MediaPlaybackRequiresUserGesture = false;
                    androidWebView.SetWebChromeClient(new PermissionWebChromeClient());
                }
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }

#if ANDROID
    // auto grants camera & mic permissions to webview
    class PermissionWebChromeClient : WebChromeClient
    {
        public override void OnPermissionRequest(PermissionRequest request)
        {
            request.Grant(request.GetResources());
        }
    }
#endif
}
