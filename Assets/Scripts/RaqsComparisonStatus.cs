using UnityEngine;

public class RaqsComparisonStatus : MonoBehaviour
{
    public DrumDanceController MaxController;
    public BailandoPosePlayer BailandoPlayer;
    public TextMesh StatusText;

    private void Reset()
    {
        StatusText = GetComponent<TextMesh>();
    }

    private void Update()
    {
        if (StatusText == null)
        {
            return;
        }

        string maxStatus = "Input: mic or Max OSC UDP 7000";

        if (MaxController != null && MaxController.TotalProcessedStrokes > 0)
        {
            maxStatus = $"Max input: {MaxController.LastInputSource} / {MaxController.LastStrokeType} ({MaxController.TotalProcessedStrokes})";
        }

        string bailandoStatus = "Bailando: waiting for shared rhythm input";

        if (BailandoPlayer != null && BailandoPlayer.EnableRetargeting)
        {
            bailandoStatus = BailandoPlayer.DriveFromRhythmInput
                ? "Bailando: reacts to the same hits"
                : "Bailando: looping generated motion";
        }

        StatusText.text = $"{maxStatus}\n{bailandoStatus}\nTest keys: 1 Doum, 2 Tek, 3 Ka, 4 Trillo";
    }
}
