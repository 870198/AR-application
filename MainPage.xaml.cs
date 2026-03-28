namespace ConelARClean;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnStartARClicked(object sender, EventArgs e)
    {
#if ANDROID
        var context = Android.App.Application.Context;
        var intent = new Android.Content.Intent(context, typeof(ARScanActivity));
        intent.AddFlags(Android.Content.ActivityFlags.NewTask);
        context.StartActivity(intent);
#else
        await DisplayAlert("Unsupported", "AR scan is only available on Android.", "OK");
#endif
    }

    private async void OnOpenVideoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new VideoPage());
    }

    private async void OnViewLogClicked(object sender, EventArgs e)
    {
        var detections = DetectionReader.GetAll();
        var message = detections.Count == 0
            ? "No detections yet."
            : string.Join("\n", detections.Take(10).Select(x => $"{x.TargetName} - {x.Timestamp:u}"));

        await DisplayAlert("Detection Log", message, "OK");
    }
}