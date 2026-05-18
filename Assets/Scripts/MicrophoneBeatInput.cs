using UnityEngine;

public class MicrophoneBeatInput : MonoBehaviour
{
    public DrumDanceController Target;
    public bool EnableMicrophoneInput = true;
    public string DeviceName = "";
    public int SampleRate = 44100;
    public int SampleWindow = 512;
    public float MinimumRms = 0.018f;
    public float Sensitivity = 2.8f;
    public float BeatCooldownSeconds = 0.24f;
    public float NoiseFloorRiseSpeed = 1.8f;
    public float NoiseFloorFallSpeed = 0.35f;

    private AudioClip microphoneClip;
    private float[] samples;
    private float noiseFloor = 0.01f;
    private float lastBeatTime = -999f;
    private int alternatingHit = 0;

    private void Start()
    {
        if (Target == null)
        {
            Target = GetComponent<DrumDanceController>();
        }

        samples = new float[Mathf.Max(64, SampleWindow)];

        if (!EnableMicrophoneInput)
        {
            return;
        }

        if (Microphone.devices == null || Microphone.devices.Length == 0)
        {
            Debug.LogWarning("MicrophoneBeatInput found no microphone devices. Use Max OSC or keys 1-4 instead.");
            EnableMicrophoneInput = false;
            return;
        }

        string selectedDevice = string.IsNullOrWhiteSpace(DeviceName) ? Microphone.devices[0] : DeviceName;
        microphoneClip = Microphone.Start(selectedDevice, true, 1, SampleRate);
        Debug.Log($"MicrophoneBeatInput listening to: {selectedDevice}");
    }

    private void OnDisable()
    {
        if (microphoneClip != null)
        {
            Microphone.End(string.IsNullOrWhiteSpace(DeviceName) ? null : DeviceName);
        }
    }

    private void Update()
    {
        if (!EnableMicrophoneInput || microphoneClip == null || Target == null)
        {
            return;
        }

        int position = Microphone.GetPosition(string.IsNullOrWhiteSpace(DeviceName) ? null : DeviceName);

        if (position <= samples.Length)
        {
            return;
        }

        microphoneClip.GetData(samples, position - samples.Length);
        float rms = GetRms(samples);
        float floorSpeed = rms > noiseFloor ? NoiseFloorRiseSpeed : NoiseFloorFallSpeed;
        noiseFloor = Mathf.Lerp(noiseFloor, rms, 1f - Mathf.Exp(-floorSpeed * Time.deltaTime));

        float threshold = Mathf.Max(MinimumRms, noiseFloor * Sensitivity);

        if (rms < threshold || Time.time - lastBeatTime < BeatCooldownSeconds)
        {
            return;
        }

        lastBeatTime = Time.time;
        Target.InjectStroke(ClassifyStroke(rms / threshold), "Mic");
    }

    private static float GetRms(float[] buffer)
    {
        float sum = 0f;

        for (int i = 0; i < buffer.Length; i++)
        {
            sum += buffer[i] * buffer[i];
        }

        return Mathf.Sqrt(sum / buffer.Length);
    }

    private string ClassifyStroke(float ratio)
    {
        if (ratio > 2.2f)
        {
            return "Doum";
        }

        if (ratio > 1.55f)
        {
            return "Trillo";
        }

        alternatingHit++;
        return alternatingHit % 2 == 0 ? "Tek" : "Ka";
    }
}
