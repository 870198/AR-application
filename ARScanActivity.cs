using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.AR.Core;

namespace ConelARClean;

[Activity(
    Theme = "@style/Maui.MainTheme.NoActionBar",
    ScreenOrientation = ScreenOrientation.Landscape)]
public class ARScanActivity : Activity
{
    private const int CameraPermissionCode = 100;

    private ARManager? _arManager;
    private Session? _arSession;
    private ARSurfaceView? _arSurfaceView;
    private bool _isSessionConfigured;
    private bool _isInitializing;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _arManager = new ARManager();

        var root = new FrameLayout(this);
        root.SetBackgroundColor(Android.Graphics.Color.Black);

        var label = new TextView(this);
        label.Text = "Scanning target image...\nPoint the tablet at the poster.";
        label.SetTextColor(Android.Graphics.Color.White);
        label.TextSize = 22f;
        label.Gravity = GravityFlags.Center;

        root.AddView(label, new FrameLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.MatchParent));

        SetContentView(root);
    }

    protected override void OnResume()
    {
        base.OnResume();

        if (!HasCameraPermission())
        {
            RequestPermissions(new[] { Android.Manifest.Permission.Camera }, CameraPermissionCode);
            return;
        }

        TryInitializeAR();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode != CameraPermissionCode)
            return;

        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
        {
            TryInitializeAR();
        }
        else
        {
            Toast.MakeText(this, "Camera permission is required for AR.", ToastLength.Long).Show();
            Finish();
        }
    }

    private bool HasCameraPermission()
    {
        if ((int)Build.VERSION.SdkInt < 23)
            return true;

        return CheckSelfPermission(Android.Manifest.Permission.Camera) == Permission.Granted;
    }

    private void TryInitializeAR()
    {
        if (_isInitializing)
            return;

        _isInitializing = true;

        try
        {
            var availability = ArCoreApk.Instance.CheckAvailability(this);

            if (availability.IsTransient)
            {
                new Handler(Looper.MainLooper).PostDelayed(() =>
                {
                    _isInitializing = false;
                    TryInitializeAR();
                }, 300);
                return;
            }

            if (availability != ArCoreApk.Availability.SupportedInstalled)
            {
                var installStatus = ArCoreApk.Instance.RequestInstall(this, true);
                if (installStatus == ArCoreApk.InstallStatus.InstallRequested)
                {
                    _isInitializing = false;
                    return;
                }

                Toast.MakeText(this, "ARCore is required.", ToastLength.Long).Show();
                Finish();
                return;
            }

            if (_arSession == null)
                _arSession = new Session(this);

            if (!_isSessionConfigured && _arManager != null)
            {
                _arManager.Setup(this, _arSession);
                _isSessionConfigured = true;
            }

            _arSession.Resume();

            if (_arSurfaceView == null && _arManager != null)
            {
                _arSurfaceView = new ARSurfaceView(this, _arSession, _arManager);
                AddContentView(_arSurfaceView, new FrameLayout.LayoutParams(1, 1));
            }

            _arSurfaceView?.OnResume();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("AR init failed: " + ex);
            Toast.MakeText(this, "AR initialization failed.", ToastLength.Long).Show();
            Finish();
        }
        finally
        {
            _isInitializing = false;
        }
    }

    protected override void OnPause()
    {
        _arSurfaceView?.OnPause();
        _arSession?.Pause();
        base.OnPause();
    }

    protected override void OnDestroy()
    {
        _arManager?.Dispose();

        if (_arSession != null)
        {
            _arSession.Close();
            _arSession.Dispose();
            _arSession = null;
        }

        base.OnDestroy();
    }
}