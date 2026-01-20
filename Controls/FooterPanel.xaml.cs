using Microsoft.Maui.Controls;

namespace UsFrameApp.Controls;

public partial class FooterPanel : ContentView
{
    public event EventHandler? LocalMicTapped;
    public event EventHandler? LocalVideoTapped;
    public event EventHandler? RemoteMicTapped;
    public event EventHandler? RemoteVideoTapped;
    public event EventHandler? SpeakerTapped;
    public event EventHandler? LeaveClicked;

    public FooterPanel()
    {
        InitializeComponent();
    }

    void OnLocalMicTapped(object? sender, EventArgs e) => LocalMicTapped?.Invoke(this, EventArgs.Empty);
    void OnLocalVideoTapped(object? sender, EventArgs e) => LocalVideoTapped?.Invoke(this, EventArgs.Empty);
    void OnRemoteMicTapped(object? sender, EventArgs e) => RemoteMicTapped?.Invoke(this, EventArgs.Empty);
    void OnRemoteVideoTapped(object? sender, EventArgs e) => RemoteVideoTapped?.Invoke(this, EventArgs.Empty);
    void OnSpeakerTapped(object? sender, EventArgs e) => SpeakerTapped?.Invoke(this, EventArgs.Empty);
    void OnLeaveClicked(object? sender, EventArgs e) => LeaveClicked?.Invoke(this, EventArgs.Empty);

    // Allow pages to update visuals
    public void SetLocalMicState(bool muted) => LocalMicBorder.BackgroundColor = muted ? Colors.Red : Color.FromArgb("#146CFF");
    public void SetLocalVideoState(bool off) => LocalVideoBorder.BackgroundColor = off ? Colors.Red : Color.FromArgb("#146CFF");
    public void SetRemoteMicState(bool muted) => RemoteMicBorder.BackgroundColor = muted ? Colors.Red : Color.FromArgb("#146CFF");
    public void SetRemoteVideoState(bool off) => RemoteVideoBorder.BackgroundColor = off ? Colors.Red : Color.FromArgb("#146CFF");
}