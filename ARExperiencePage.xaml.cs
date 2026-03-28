using Microsoft.Maui.ApplicationModel;

namespace ConelARClean;

public partial class ARExperiencePage : ContentPage
{
    private readonly string _targetName;

    private const string BaseModelUrl = "https://870198.github.io/conel-ar-models/models/";

    public ARExperiencePage(string targetName)
    {
        InitializeComponent();
        _targetName = targetName;

        TitleLabel.Text = "CONEL Esports AR Experience";
        SubtitleLabel.Text = "Detected target: " + targetName;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await PlayAudioMessage();
    }

    private async void OnViewModelClicked(object sender, EventArgs e)
    {
        try
        {
            var modelUrl = BaseModelUrl + _targetName + ".glb";

            var uri = new Uri(
                "https://arvr.google.com/scene-viewer/1.0" +
                "?file=" + Uri.EscapeDataString(modelUrl) +
                "&mode=ar_preferred" +
                "&title=" + Uri.EscapeDataString("CONEL Esports Avatar"));

            await Launcher.Default.OpenAsync(uri);

            bool goToVideo = await DisplayAlert(
                "Avatar Experience",
                "Would you like to continue to the promotional video after viewing the 3D avatar?",
                "Yes",
                "No");

            if (goToVideo)
            {
                await Navigation.PushAsync(new VideoPage());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Model Error", "Could not open the 3D avatar.\n" + ex.Message, "OK");
        }
    }

    private async void OnVideoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new VideoPage());
    }

    private async void OnAudioClicked(object sender, EventArgs e)
    {
        await PlayAudioMessage();
    }

    private async void OnDiscordClicked(object sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync("https://discord.gg/ZYP8DQJW");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private Task PlayAudioMessage()
    {
        try
        {
#if ANDROID
            Android.Speech.Tts.TextToSpeech? tts = null;

            tts = new Android.Speech.Tts.TextToSpeech(
                Android.App.Application.Context,
                new TtsInitListener(() =>
                {
                    tts?.Speak(
                        "Welcome to the CONEL Esports Studio. Explore our three dimensional avatar, watch the esports promotional video, and learn more about the studio using the interactive controls on screen.",
                        Android.Speech.Tts.QueueMode.Flush,
                        null,
                        "conel_welcome");
                }));
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Text to speech failed: " + ex);
        }

        return Task.CompletedTask;
    }

#if ANDROID
    private sealed class TtsInitListener : Java.Lang.Object, Android.Speech.Tts.TextToSpeech.IOnInitListener
    {
        private readonly Action _onReady;

        public TtsInitListener(Action onReady)
        {
            _onReady = onReady;
        }

        public void OnInit(Android.Speech.Tts.OperationResult status)
        {
            if (status == Android.Speech.Tts.OperationResult.Success)
                _onReady?.Invoke();
        }
    }
#endif
}