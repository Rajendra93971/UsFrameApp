//using CommunityToolkit.Maui.Views;
//using Microsoft.Maui.ApplicationModel;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Networking;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using UsFrameApp.Popups;
//using UsFrameApp.Services;
//using UsFrameApp.ViewModels;

//namespace UsFrameApp.View;

//public partial class ConnectingPage : ContentPage, INotifyPropertyChanged
//{
//    private readonly string _roomKey;

//    public event PropertyChangedEventHandler PropertyChanged;

//    void OnPropertyChanged([CallerMemberName] string name = null)
//        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

//    readonly Color SuccessGreen = Color.FromArgb("#1DB954");

//    public Color Step1Color { get; set; } = Color.FromArgb("#FF1B1B");
//    public Color Step2Color { get; set; } = Color.FromArgb("#FF1B1B");
//    public Color Step3Color { get; set; } = Color.FromArgb("#FF1B1B");
//    public Color Step4Color { get; set; } = Color.FromArgb("#FF1B1B");
//    public Color Line1Color { get; set; } = Color.FromArgb("#FFDBDB");

//    double _lineHeight;
//    public double LineHeight
//    {
//        get => _lineHeight;
//        set
//        {
//            _lineHeight = value;
//            OnPropertyChanged();
//        }
//    }

//    private bool _isAnimating;
//    private bool _started;

//    public ConnectingPage(string roomKey)
//    {
//        InitializeComponent();
//        _roomKey = roomKey;
//        BindingContext = this;
//    }

//    protected override async void OnAppearing()
//    {
//        base.OnAppearing();

//        //
//        _ = InitializeAsync();
//        //if (_started)
//        //    return;

//        //_started = true;

//        //StartRotation();
//        //await StartFlowAsync();
//    }
//    // Page lifecycle
//    async Task InitializeAsync()
//    {
//        StartRotation();
//        await StartFlowAsync();
//    }


//    // Main connection flow: connectivity, permissions, and navigation
//    public async Task StartFlowAsync()
//    {
//        // STEP 1: Verify internet connectivity
//        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
//        {
//            //StopRotation();

//            await Application.Current.MainPage
//                .ShowPopupAsync(new AppAlertPopup(
//                    "Not Connected",
//                    "Internet connection required"));

//            return;
//        }

//        // STEP 2: Simulate server connection progress
//        Line1Color = Color.FromArgb("#1DB954");
//        OnPropertyChanged(nameof(Line1Color));

//        await AnimateLineTo(60);

//        Line1Color = SuccessGreen;
//        Step1Color = SuccessGreen;
//        OnPropertyChanged(nameof(Line1Color));
//        OnPropertyChanged(nameof(Step1Color));

//        // STEP 3: Prepare room (simulated progress)
//        Line1Color = Color.FromArgb("#1DB954");
//        OnPropertyChanged(nameof(Line1Color));

//        await AnimateLineTo(110);

//        Line1Color = SuccessGreen;
//        Step2Color = SuccessGreen;
//        OnPropertyChanged(nameof(Line1Color));
//        OnPropertyChanged(nameof(Step2Color));

//        // STEP 4: Request required permissions
//        Line1Color = Color.FromArgb("#1DB954");
//        OnPropertyChanged(nameof(Line1Color));

//        await AnimateLineTo(148);

//        // Request camera permission
//        var cam = await Permissions.CheckStatusAsync<Permissions.Camera>();
//        if (cam != PermissionStatus.Granted)
//            cam = await Permissions.RequestAsync<Permissions.Camera>();

//        // Request microphone permission
//        var mic = await Permissions.CheckStatusAsync<Permissions.Microphone>();
//        if (mic != PermissionStatus.Granted)
//            mic = await Permissions.RequestAsync<Permissions.Microphone>();

//        // Abort flow if required permissions are not granted
//        if (cam != PermissionStatus.Granted || mic != PermissionStatus.Granted)
//        {
//            StopRotation();

//            await Application.Current.MainPage
//                .ShowPopupAsync(new AppAlertPopup(
//                    "Can't Connect",
//                    "Camera & Microphone permission required"));

//            return;
//        }

//        Line1Color = SuccessGreen;
//        Step3Color = SuccessGreen;
//        OnPropertyChanged(nameof(Line1Color));
//        OnPropertyChanged(nameof(Step3Color));

//        // STEP 5: Join room (simulated progress)
//        Line1Color = Color.FromArgb("#1DB954");
//        OnPropertyChanged(nameof(Line1Color));

//        await AnimateLineTo(230);

//        Line1Color = SuccessGreen;
//        Step4Color = SuccessGreen;
//        OnPropertyChanged(nameof(Line1Color));
//        OnPropertyChanged(nameof(Step4Color));

//        await MainThread.InvokeOnMainThreadAsync(async () =>
//        {
//            var nav = Application.Current?.MainPage?.Navigation;
//            if (nav == null)
//                return;

//            var roomVm = App.Services.GetRequiredService<RoomViewModel>();
//            await roomVm.InitializeAsync(_roomKey);
//            await nav.PushAsync(new RoomPage(roomVm));
//        });
//    }


//    async Task AnimateLineTo(double targetHeight)
//    {
//        for (double h = LineHeight; h <= targetHeight; h += 5)
//        {
//            LineHeight = h;
//            await Task.Delay(80);
//        }
//    }



//    // Rotating icon animation control
//    private async void StartRotation()
//    {
//        if (_isAnimating)
//            return;

//        _isAnimating = true;

//        while (_isAnimating)
//        {
//            await ConnectingIcon.RotateTo(360, 900, Easing.Linear);
//            ConnectingIcon.Rotation = 0;
//        }
//    }

//    public void StopRotation()
//    {
//        _isAnimating = false;
//        ConnectingIcon.AbortAnimation("Rotation");
//        ConnectingIcon.Rotation = 0;
//    }


//}

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UsFrameApp.Popups;
using UsFrameApp.Services;
using UsFrameApp.ViewModels;

namespace UsFrameApp.View;

public partial class ConnectingPage : ContentPage, INotifyPropertyChanged
{
    private readonly string _roomKey;

    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    readonly Color SuccessGreen = Color.FromArgb("#1DB954");

    public Color Step1Color { get; set; } = Color.FromArgb("#FF1B1B");
    public Color Step2Color { get; set; } = Color.FromArgb("#FF1B1B");
    public Color Step3Color { get; set; } = Color.FromArgb("#FF1B1B");
    public Color Step4Color { get; set; } = Color.FromArgb("#FF1B1B");
    public Color Line1Color { get; set; } = Color.FromArgb("#FFDBDB");

    double _lineHeight;
    public double LineHeight
    {
        get => _lineHeight;
        set
        {
            _lineHeight = value;
            OnPropertyChanged();
        }
    }

    private bool _isAnimating;
    private bool _started;

    public ConnectingPage(string roomKey)
    {
        InitializeComponent();
        _roomKey = roomKey;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        StartRotation();
        _ = StartFlowAsync();
    }



    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();

    //    //
    //    _ = InitializeAsync();
    //    //if (_started)
    //    //    return;

    //    //_started = true;

    //    //StartRotation();
    //    //await StartFlowAsync();
    //}
    //// Page lifecycle
    //async Task InitializeAsync()
    //{
    //    StartRotation();
    //    await StartFlowAsync();
    //}


    // Main connection flow: connectivity, permissions, and navigation
    public async Task StartFlowAsync()
    {
        // STEP 1: Verify internet connectivity
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            //StopRotation();

            await Application.Current.MainPage
                .ShowPopupAsync(new AppAlertPopup(
                    "Not Connected",
                    "Internet connection required"));

            return;
        }

        // STEP 2: Simulate server connection progress
        Line1Color = Color.FromArgb("#1DB954");
        OnPropertyChanged(nameof(Line1Color));

        await AnimateLineTo(GetLineHeight(60, 120, 180));

        Line1Color = SuccessGreen;
        Step1Color = SuccessGreen;
        OnPropertyChanged(nameof(Line1Color));
        OnPropertyChanged(nameof(Step1Color));

        // STEP 3: Prepare room (simulated progress)
        Line1Color = Color.FromArgb("#1DB954");
        OnPropertyChanged(nameof(Line1Color));

        await AnimateLineTo(GetLineHeight(110, 140, 220));

        Line1Color = SuccessGreen;
        Step2Color = SuccessGreen;
        OnPropertyChanged(nameof(Line1Color));
        OnPropertyChanged(nameof(Step2Color));

        // STEP 4: Request required permissions
        Line1Color = Color.FromArgb("#1DB954");
        OnPropertyChanged(nameof(Line1Color));

        await AnimateLineTo(GetLineHeight(135, 200, 280));

        // Request camera permission
        var cam = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (cam != PermissionStatus.Granted)
            cam = await Permissions.RequestAsync<Permissions.Camera>();

        // Request microphone permission
        var mic = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (mic != PermissionStatus.Granted)
            mic = await Permissions.RequestAsync<Permissions.Microphone>();

        // Abort flow if required permissions are not granted
        if (cam != PermissionStatus.Granted || mic != PermissionStatus.Granted)
        {
            StopRotation();

            await Application.Current.MainPage
                .ShowPopupAsync(new AppAlertPopup(
                    "Can't Connect",
                    "Camera & Microphone permission required"));

            return;
        }

        Line1Color = SuccessGreen;
        Step3Color = SuccessGreen;
        OnPropertyChanged(nameof(Line1Color));
        OnPropertyChanged(nameof(Step3Color));

        // STEP 5: Join room (simulated progress)
        Line1Color = Color.FromArgb("#1DB954");
        OnPropertyChanged(nameof(Line1Color));

        await AnimateLineTo(GetLineHeight(210, 340, 420));

        Line1Color = SuccessGreen;
        Step4Color = SuccessGreen;
        OnPropertyChanged(nameof(Line1Color));
        OnPropertyChanged(nameof(Step4Color));

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var nav = Application.Current?.MainPage?.Navigation;
            if (nav == null)
                return;

            var roomVm = App.Services.GetRequiredService<RoomViewModel>();
           // await roomVm.InitializeAsync(_roomKey);
            await nav.PushAsync(new RoomPage(roomVm));
        });
    }


    async Task AnimateLineTo(double targetHeight)
    {
        double step = DeviceInfo.Idiom == DeviceIdiom.Phone ? 5 : 8;
        int delay = DeviceInfo.Idiom == DeviceIdiom.Phone ? 80 : 50;

        for (double h = LineHeight; h <= targetHeight; h += step)
        {
            LineHeight = h;
            await Task.Delay(delay);
        }
    }




    double GetLineHeight(double phone, double tablet, double desktop)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            return tablet;

        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
            return desktop;

        return phone;
    }



    // Rotating icon animation control
    private async void StartRotation()
    {
        if (_isAnimating)
            return;

        _isAnimating = true;

        while (_isAnimating)
        {
            await ConnectingIcon.RotateTo(360, 800, Easing.Linear);
            ConnectingIcon.Rotation = 0;
        }
    }

    public void StopRotation()
    {
        _isAnimating = false;
        ConnectingIcon.AbortAnimation("Rotation");
        ConnectingIcon.Rotation = 0;
    }


}