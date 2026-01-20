using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using UsFrameApp.View;

namespace UsFrameApp.Popups;

public partial class SettingsPanel : ContentView
{
	public SettingsPanel()
	{
		InitializeComponent();

		// initialize state from Preferences
		SelfViewSwitch.IsToggled = Preferences.Get("SelfView", false);
		DebugSwitch.IsToggled = Preferences.Get("DebugMode", false);
		ExpandSwitch.IsToggled = Preferences.Get("ExpandVideo", false);
		NightSwitch.IsToggled = Preferences.Get("NightMode", false);
	}

	// Slide down animation to show panel
	public async Task ShowAsync()
	{
		try
		{
			var pageHeight = Application.Current?.MainPage?.Height ?? 800;
			var targetHeight = Math.Max(360, pageHeight * 0.62);

			Panel.HeightRequest = targetHeight;
			this.TranslationY = -targetHeight;
			this.IsVisible = true;

			await Task.Yield();
			await this.TranslateTo(0, 0, 320, Easing.CubicOut);
		}
		catch
		{
			this.IsVisible = true;
			this.TranslationY = 0;
		}
	}

	// Reverse animation to hide
	public async Task HideAsync()
	{
		try
		{
			var h = Panel.HeightRequest;
			if (h <= 0) h = Application.Current?.MainPage?.Height ?? 420;
			await this.TranslateTo(0, -h, 260, Easing.CubicIn);
			this.IsVisible = false;
		}
		catch
		{
			this.IsVisible = false;
		}
	}

	private async void OnCloseClicked(object sender, EventArgs e)
	{
		await HideAsync();
	}

	private void SelfViewSwitch_Toggled(object sender, ToggledEventArgs e)
	{
		Preferences.Set("SelfView", e.Value);
		var room = Application.Current?.MainPage?.Navigation?.NavigationStack?.OfType<RoomPage>()?.LastOrDefault();
		room?.ApplySelfViewFromSettings();
	}

	private void DebugSwitch_Toggled(object sender, ToggledEventArgs e)
	{
		Preferences.Set("DebugMode", e.Value);
	}

	private void ExpandSwitch_Toggled(object sender, ToggledEventArgs e)
	{
		Preferences.Set("ExpandVideo", e.Value);
		var room = Application.Current?.MainPage?.Navigation?.NavigationStack?.OfType<RoomPage>()?.LastOrDefault();
		room?.ApplySelfViewFromSettings();
	}

	private void NightSwitch_Toggled(object sender, ToggledEventArgs e)
	{
		Preferences.Set("NightMode", e.Value);
	}
}