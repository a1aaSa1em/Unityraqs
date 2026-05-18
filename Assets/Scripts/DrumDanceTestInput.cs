using UnityEngine;

public class DrumDanceTestInput : MonoBehaviour
{
    public DrumDanceController Target;

    [Tooltip("Manual keys still work, but automatic fake beats are permanently blocked unless this is deliberately enabled in code.")]
    public bool EnableAutomaticPattern = false;

    [Tooltip("Deprecated scene value kept only so old serialized scenes do not break. Ignored unless EnableAutomaticPattern is also enabled.")]
    public bool AutoPlayWhenNoLiveInput = false;

    public float LiveInputGraceSeconds = 2f;
    public float TestHitInterval = 0.38f;

    public KeyCode DoumKey = KeyCode.Alpha1;
    public KeyCode TekKey = KeyCode.Alpha2;
    public KeyCode KaKey = KeyCode.Alpha3;
    public KeyCode TrilloKey = KeyCode.Alpha4;

    private readonly string[] pattern = { "Doum", "Tek", "Ka", "Tek", "Doum", "Trillo", "Tek", "Ka" };
    private int patternIndex = 0;
    private float nextTestHitTime = 0f;

    private void Reset()
    {
        Target = GetComponent<DrumDanceController>();
    }

    private void Update()
    {
        if (Target == null)
        {
            return;
        }

        HandleManualKeys();
        HandleAutoPattern();
    }

    private void HandleManualKeys()
    {
        if (Input.GetKeyDown(DoumKey))
        {
            Target.InjectTestStroke("Doum");
        }

        if (Input.GetKeyDown(TekKey))
        {
            Target.InjectTestStroke("Tek");
        }

        if (Input.GetKeyDown(KaKey))
        {
            Target.InjectTestStroke("Ka");
        }

        if (Input.GetKeyDown(TrilloKey))
        {
            Target.InjectTestStroke("Trillo");
        }
    }

    private void HandleAutoPattern()
    {
        if (!EnableAutomaticPattern || !AutoPlayWhenNoLiveInput || Time.time < nextTestHitTime)
        {
            return;
        }

        bool recentLiveInput = Target.HasRecentInput(LiveInputGraceSeconds) && Target.LastInputSource == "OSC";

        if (recentLiveInput)
        {
            nextTestHitTime = Time.time + TestHitInterval;
            return;
        }

        Target.InjectTestStroke(pattern[patternIndex]);
        patternIndex = (patternIndex + 1) % pattern.Length;
        nextTestHitTime = Time.time + TestHitInterval;
    }
}
