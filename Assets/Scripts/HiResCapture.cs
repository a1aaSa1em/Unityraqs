using System.IO;
using UnityEngine;

public class HiResCapture : MonoBehaviour
{
    public Camera captureCamera;
    public int width = 3840;
    public int height = 2160;
    public int startFrame = 0;
    public int endFrame = 120;
    public string outputDir = "Recordings";
    public bool transparentBackground = true;
    public bool captureOnStart = false;
    public KeyCode stillCaptureKey = KeyCode.F9;
    public KeyCode alternateStillCaptureKey = KeyCode.X;

    int frame;
    RenderTexture rt;
    Texture2D tex;
    string fullDir;
    bool isCapturing;

    void Start()
    {
        if (captureOnStart)
        {
            BeginCapture();
            return;
        }

        Time.captureFramerate = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(stillCaptureKey) || Input.GetKeyDown(alternateStillCaptureKey))
        {
            CaptureStillFrame();
        }
    }

    [ContextMenu("Begin Capture")]
    public void BeginCapture()
    {
        EnsureCaptureSetup();

        if (transparentBackground)
        {
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0, 0, 0, 0);
        }

        Time.captureFramerate = 60;
        isCapturing = true;
        frame = 0;
        Debug.Log($"HiResCapture: writing PNGs to {Path.GetFullPath(fullDir)}");
    }

    [ContextMenu("Capture Still Frame")]
    public void CaptureStillFrame()
    {
        EnsureCaptureSetup();
        CaptureFrame($"dance_still_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
    }

    void LateUpdate()
    {
        if (!isCapturing)
        {
            return;
        }

        if (frame < startFrame) { frame++; return; }
        if (frame > endFrame) { StopCapture(); return; }

        CaptureFrame($"dance_{frame:D4}.png");

        frame++;
    }

    private void EnsureCaptureSetup()
    {
        if (captureCamera == null) captureCamera = Camera.main;

        fullDir = Path.Combine(Application.dataPath, "..", outputDir);
        Directory.CreateDirectory(fullDir);

        if (rt == null || rt.width != width || rt.height != height)
        {
            rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        }

        if (tex == null || tex.width != width || tex.height != height)
        {
            tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        }
    }

    private void CaptureFrame(string fileName)
    {
        var prev = captureCamera.targetTexture;
        captureCamera.targetTexture = rt;
        captureCamera.Render();
        captureCamera.targetTexture = prev;

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        var path = Path.Combine(fullDir, fileName);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log($"HiResCapture: saved {Path.GetFullPath(path)}");
    }

    [ContextMenu("Stop Capture")]
    public void StopCapture()
    {
        isCapturing = false;
        Time.captureFramerate = 0;
        Debug.Log("HiResCapture: done.");
    }

    void OnDisable()
    {
        if (isCapturing)
        {
            StopCapture();
        }
        else
        {
            Time.captureFramerate = 0;
        }
    }
}
