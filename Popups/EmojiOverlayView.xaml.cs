using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Devices;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UsFrameApp.Popups;

public partial class EmojiOverlayView : ContentView
{
    static readonly Random _random = new();
    CancellationTokenSource? _autoCloseCts;

    public EmojiOverlayView()
    {
        InitializeComponent();
        // Picker is hidden by default; TogglePicker controls visibility
        Picker.IsVisible = false;
    }

    // Toggle the emoji picker visibility and manage the outside-tap layer
    public void TogglePicker()
    {
        bool show = !Picker.IsVisible;

        Picker.IsVisible = show;
        OutsideTapLayer.IsVisible = show;

        if (!show)
            CancelAutoClose();
    }

    private void OnOutsideTapped(object sender, EventArgs e)
    {
        ClosePicker();
    }

    // Spawn multiple animated emojis and close the picker after a single tap
    private void OnEmojiTapped(object sender, EventArgs e)
    {
        if (sender is not Label emoji)
            return;

        FireMultipleEmojis(emoji.Text);

        ClosePicker();
    }

    private async void FireMultipleEmojis(string emoji)
    {
        int count = 12;

        for (int i = 0; i < count; i++)
        {
            SpawnFlyingEmoji(emoji, i, count);

        }
    }
    private void SpawnFlyingEmoji(string emoji, int index, int total)
    {
        var display = DeviceDisplay.MainDisplayInfo;
        var screenHeight = display.Height / display.Density;


        double x = (double)index / (total - 1);
        double horizontalGap = 0.03;
        double offset = (index % 2 == 0) ? horizontalGap : -horizontalGap;

        x = Math.Clamp(x + offset, 0.02, 0.98);

        double travelExtra = _random.Next(0, 200);

        var flyingEmoji = new Label
        {
            Text = emoji,
            FontSize = 30,
            Opacity = 1
        };

        // Place the emoji at the bottom of the layout using proportional coordinates
        AbsoluteLayout.SetLayoutBounds(
            flyingEmoji,
            new Rect(x, 1.05, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize)
        );

        // Use proportional positioning so the emoji's X coordinate is relative to width
        AbsoluteLayout.SetLayoutFlags(
            flyingEmoji,
            AbsoluteLayoutFlags.PositionProportional
        );

        Root.Children.Add(flyingEmoji);

        _ = AnimateAndRemoveAsync(flyingEmoji, screenHeight + travelExtra);
    }

    private async Task AnimateAndRemoveAsync(Microsoft.Maui.Controls.View emoji, double travelDistance)
    {
        //  slow to fast line code
        uint duration = (uint)_random.Next(3200, 3400);

        await Task.WhenAll(
            emoji.TranslateTo(
                0,
                -travelDistance,
                duration,
                Easing.SinOut
            ),
            emoji.FadeTo(
                0,
                duration,
                Easing.Linear
            )
        );

        Root.Children.Remove(emoji);
    }

    private void RestartAutoCloseTimer()
    {
        CancelAutoClose();

        _autoCloseCts = new CancellationTokenSource();
        var token = _autoCloseCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(700, token);
                MainThread.BeginInvokeOnMainThread(ClosePicker);
            }
            catch (TaskCanceledException) { }
        });
    }


    private void CancelAutoClose()
    {
        _autoCloseCts?.Cancel();
        _autoCloseCts = null;
    }

    private void ClosePicker()
    {
        Picker.IsVisible = false;
        OutsideTapLayer.IsVisible = false;
        CancelAutoClose();
    }
}
