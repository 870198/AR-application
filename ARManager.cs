using Android.App;
using Android.Graphics;
using Google.AR.Core;
using SQLite;

namespace ConelARClean;

public sealed class ARManager : IDisposable
{
    private Session? _session;
    private SQLiteConnection? _db;
    private Activity? _activity;
    private bool _disposed;
    private readonly HashSet<string> _firedTargets = new();

    public void Setup(Activity activity, Session session)
    {
        _activity = activity;
        _session = session;

        SetupDatabase();
        ConfigureImageTracking();
    }

    private void SetupDatabase()
    {
        try
        {
            var dbPath = System.IO.Path.Combine(
                Microsoft.Maui.Storage.FileSystem.AppDataDirectory,
                "detections.db3");

            _db = new SQLiteConnection(dbPath, true);
            _db.CreateTable<DetectionEvent>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Database setup failed: " + ex);
            _db = null;
        }
    }

    private void ConfigureImageTracking()
    {
        if (_session == null || _activity == null)
            return;

        try
        {
            var config = new Config(_session);
            var imageDb = new AugmentedImageDatabase(_session);

            AddTarget(imageDb, "campus_poster", 0.30f);

            config.SetAugmentedImageDatabase(imageDb);
            config.SetFocusMode(Config.FocusMode.Auto);
            config.SetUpdateMode(Config.UpdateMode.LatestCameraImage);

            _session.Configure(config);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("ConfigureImageTracking failed: " + ex);
        }
    }

    private void AddTarget(AugmentedImageDatabase db, string name, float physicalWidthM)
    {
        try
        {
            if (_activity == null)
                return;

            int resourceId = _activity.Resources.GetIdentifier(
                "target_image",
                "raw",
                _activity.PackageName);

            if (resourceId == 0)
            {
                System.Diagnostics.Debug.WriteLine("Android raw resource not found: target_image");
                return;
            }

            using var stream = _activity.Resources.OpenRawResource(resourceId);
            using var bitmap = BitmapFactory.DecodeStream(stream);

            if (bitmap == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to decode Android raw resource: target_image");
                return;
            }

            db.AddImage(name, bitmap, physicalWidthM);
            System.Diagnostics.Debug.WriteLine("Added AR target: " + name);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Failed to load target image from Android raw resource: " + ex);
        }
    }

    public void ProcessFrame()
    {
        if (_session == null)
            return;

        var frame = _session.Update();
        var updated = frame.GetUpdatedTrackables(Java.Lang.Class.FromType(typeof(AugmentedImage)));

        foreach (var obj in updated)
        {
            var image = obj as AugmentedImage;
            if (image == null)
                continue;

            if (image.TrackingState != TrackingState.Tracking)
                continue;

            var targetName = image.Name;
            if (string.IsNullOrEmpty(targetName))
                continue;

            if (_firedTargets.Contains(targetName))
                continue;

            _firedTargets.Add(targetName);
            LogDetection(targetName);

            _activity?.RunOnUiThread(() =>
            {
                try
                {
                    var intent = new Android.Content.Intent(_activity, typeof(MainActivity));
                    intent.AddFlags(Android.Content.ActivityFlags.ClearTop | Android.Content.ActivityFlags.SingleTop);
                    _activity.StartActivity(intent);

                    Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(async () =>
                    {
                        try
                        {
                            if (Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0)
                            {
                                var page = Microsoft.Maui.Controls.Application.Current.Windows[0].Page;
                                if (page is NavigationPage navPage)
                                {
                                    await navPage.Navigation.PushAsync(new ARExperiencePage(targetName));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Navigation failed: " + ex);
                        }
                    });

                    _activity.Finish();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Detection handling failed: " + ex);
                }
            });
        }
    }

    private void LogDetection(string targetName)
    {
        try
        {
            _db?.Insert(new DetectionEvent
            {
                TargetName = targetName,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("LogDetection failed: " + ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _db?.Close();
        _disposed = true;
    }
}