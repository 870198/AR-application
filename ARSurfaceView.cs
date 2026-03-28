using Android.Content;
using Android.Opengl;
using Google.AR.Core;
using Javax.Microedition.Khronos.Opengles;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

namespace ConelARClean;

public sealed class ARSurfaceView : GLSurfaceView, GLSurfaceView.IRenderer
{
    private readonly Session _session;
    private readonly ARManager _manager;
    private int _cameraTextureId;

    public ARSurfaceView(Context context, Session session, ARManager manager) : base(context)
    {
        _session = session;
        _manager = manager;

        PreserveEGLContextOnPause = true;
        SetEGLContextClientVersion(2);
        SetRenderer(this);
        RenderMode = Rendermode.Continuously;
    }

    public void OnSurfaceCreated(IGL10? gl, EGLConfig? config)
    {
        GLES20.GlClearColor(0f, 0f, 0f, 1f);

        var textures = new int[1];
        GLES20.GlGenTextures(1, textures, 0);
        _cameraTextureId = textures[0];

        GLES20.GlBindTexture(GLES11Ext.GlTextureExternalOes, _cameraTextureId);
        GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureMinFilter, GLES20.GlLinear);
        GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureMagFilter, GLES20.GlLinear);

        _session.SetCameraTextureName(_cameraTextureId);
    }

    public void OnSurfaceChanged(IGL10? gl, int width, int height)
    {
        GLES20.GlViewport(0, 0, width, height);
    }

    public void OnDrawFrame(IGL10? gl)
    {
        GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);

        try
        {
            _manager.ProcessFrame();
        }
        catch (Java.Lang.Exception)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("AR frame error: " + ex);
        }
    }
}