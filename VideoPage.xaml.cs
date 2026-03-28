using CommunityToolkit.Maui.Views;

namespace ConelARClean;

public partial class VideoPage : ContentPage
{
    private readonly List<string> videos = new()
    {
        "conel_esports_promo.mp4",
        "vid2.mp4",
        "vid3.mp4"
    };

    private int currentIndex = 0;

    public VideoPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (PromoPlayer != null)
            PromoPlayer.Play();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (PromoPlayer != null)
        {
            PromoPlayer.Stop();
            PromoPlayer.Handler?.DisconnectHandler();
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnNextClicked(object sender, EventArgs e)
    {
        currentIndex = (currentIndex + 1) % videos.Count;
        PromoPlayer.Source = MediaSource.FromResource(videos[currentIndex]);
        PromoPlayer.Play();
    }

    private void OnPreviousClicked(object sender, EventArgs e)
    {
        currentIndex = (currentIndex - 1 + videos.Count) % videos.Count;
        PromoPlayer.Source = MediaSource.FromResource(videos[currentIndex]);
        PromoPlayer.Play();
    }

    private async void OnDiscordClicked(object sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync("https://discord.gg/ZYP8DQJW");
    }
}