using UnityEngine;
using OscCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class DrumDanceController : MonoBehaviour
{
    public enum DanceMode
    {
        ImmediateReactions,
        BufferedPhrases
    }

    [Serializable]
    public class DancePhrase
    {
        [Tooltip("Inspector label only.")]
        public string Name = "Phrase";

        [Tooltip("Animator state names that belong to this phrase family. Keep this curated and stylistically similar.")]
        public List<string> States = new List<string>();

        [Tooltip("Higher values make this phrase more likely.")]
        [Min(1)]
        public int Weight = 1;

        [Tooltip("Optional stroke this phrase is best for: Any, Doum, Tek, Ka, or Trillo.")]
        public string PreferredStroke = "Any";
    }

    [Serializable]
    public class RecordedStroke
    {
        public string Type;
        public float Time;

        public RecordedStroke(string type, float time)
        {
            Type = type;
            Time = time;
        }
    }

    private struct StrokeHit
    {
        public string Type;
        public float Time;

        public StrokeHit(string type, float time)
        {
            Type = type;
            Time = time;
        }
    }

    [Header("References")]
    public Animator LolaAnimator;

    [Tooltip("Must exactly match the idle state name in the Animator.")]
    public string IdleStateName = "Idle";

    [Header("OSC")]
    public int Port = 7000;

    [Header("Dance Mode")]
    [Tooltip("BufferedPhrases waits briefly, reads the rhythm shape, then plays a coherent 2-4 second movement.")]
    public DanceMode Mode = DanceMode.BufferedPhrases;

    [Header("Phrase Buffer")]
    [Tooltip("How much rhythm to collect before choosing the next phrase.")]
    public float PhraseBufferSeconds = 2.5f;

    [Tooltip("If the rhythm stops before the buffer fills, play what has been collected after this much silence.")]
    public float SilenceFlushSeconds = 0.55f;

    [Tooltip("Minimum hits needed before a buffered phrase can be played.")]
    public int MinimumHitsForPhrase = 1;

    [Tooltip("How long a phrase should play before the next buffered phrase is allowed to replace it.")]
    public float PhrasePlaySeconds = 2.5f;

    [Tooltip("Blend time when moving between full dance phrases.")]
    public float PhraseCrossfade = 0.28f;

    [Tooltip("Keep the last phrase moving while the next buffer is being collected.")]
    public bool HoldPhraseUntilNextPhrase = true;

    [Header("Curated Phrase Pools")]
    [Tooltip("Use these for grounded hip/belly material. Add only clips that look related.")]
    public List<DancePhrase> PhrasePools = new List<DancePhrase>();

    [Header("Beat Response")]
    [Tooltip("Makes each incoming hit briefly push the current animation speed so the dance still reacts on the beat.")]
    public bool PulseAnimatorSpeedOnHits = true;

    [Tooltip("How quickly the animation speed settles back to normal after a hit.")]
    public float BeatPulseReturnSpeed = 8f;

    public float DoumSpeedPulse = 1.35f;
    public float TekSpeedPulse = 1.18f;
    public float KaSpeedPulse = 1.22f;
    public float TrilloSpeedPulse = 1.45f;

    [Header("Record / Playback")]
    [Tooltip("Press this during Play Mode to start/stop recording OSC hits from Max.")]
    public KeyCode ToggleRecordingKey = KeyCode.R;

    [Tooltip("Press this during Play Mode to start/stop replaying the recorded rhythm.")]
    public KeyCode TogglePlaybackKey = KeyCode.P;

    [Tooltip("Press this during Play Mode to clear the recorded rhythm.")]
    public KeyCode ClearRecordingKey = KeyCode.C;

    [Tooltip("When enabled, live Max hits are saved with timing.")]
    public bool RecordLiveInput = false;

    [Tooltip("When enabled, Unity replays the recorded hit timing through the same dance logic.")]
    public bool PlaybackRecordedPattern = false;

    public bool LoopRecordedPattern = true;

    [Tooltip("Ignore live Max input while replaying, so the playback is exactly the recorded pattern.")]
    public bool IgnoreLiveInputDuringPlayback = true;

    [Tooltip("Small delay before playback starts, useful for watching from the beginning.")]
    public float PlaybackStartDelay = 0.25f;

    public List<RecordedStroke> RecordedPattern = new List<RecordedStroke>();

    [Header("Dance Clip Pools")]
    [Tooltip("Legacy immediate-hit pool. Also used as fallback if PhrasePools is empty.")]
    public List<string> DoumDanceStates = new List<string>();
    public List<string> TekDanceStates = new List<string>();
    public List<string> KaDanceStates = new List<string>();
    public List<string> TrilloDanceStates = new List<string>();

    [Header("Reaction Timing")]
    [Tooltip("How long each reaction plays before returning to idle.")]
    public float ReactionSeconds = 0.65f;

    [Tooltip("Blend time when starting a reaction.")]
    public float DanceCrossfade = 0.12f;

    [Tooltip("Blend time when returning to idle.")]
    public float ReturnCrossfade = 0.35f;

    [Tooltip("Prevents Lola from changing moves too rapidly.")]
    public float MinimumSecondsBetweenMoves = 0.25f;

    [Header("Randomisation")]
    [Tooltip("Avoid choosing the same clip twice in a row for the same stroke.")]
    public bool AvoidImmediateRepeats = true;

    private OscServer server;
    private ConcurrentQueue<string> triggerQueue = new ConcurrentQueue<string>();

    private Dictionary<string, string> lastPlayedClip = new Dictionary<string, string>();
    private List<StrokeHit> phraseHits = new List<StrokeHit>();

    private float returnToIdleTime = -1f;
    private float lastMoveTime = -999f;
    private float phraseStartTime = -1f;
    private float lastHitTime = -1f;
    private float nextPhraseAllowedTime = -999f;
    private float targetAnimatorSpeed = 1f;
    private float recordingStartTime = -1f;
    private float playbackStartTime = -1f;
    private int playbackIndex = 0;
    private string lastPhraseState = null;
    private bool isDancing = false;

    void Start()
    {
        server = new OscServer(Port);

        server.TryAddMethod("/doum", OnDoumReceived);
        server.TryAddMethod("/tek", OnTekReceived);
        server.TryAddMethod("/ka", OnKaReceived);
        server.TryAddMethod("/trillo", OnTrilloReceived);

        Debug.Log("OSC Listening on port " + Port);
        Debug.Log($"Pools — Doum: {DoumDanceStates.Count} | Tek: {TekDanceStates.Count} | Ka: {KaDanceStates.Count} | Trillo: {TrilloDanceStates.Count}");
        Debug.Log($"Phrase pools: {PhrasePools.Count} | Mode: {Mode}");
    }

    [ContextMenu("Fill Suggested Belly Phrase Pools")]
    private void FillSuggestedBellyPhrasePools()
    {
        PhrasePools.Clear();

        AddPhrase("Grounded belly flow", "Any", 4,
            "Bellydancing",
            "Belly Dance",
            "belly_continuous",
            "belly_b_continuous",
            "belly_a_flow",
            "belly_extended");

        AddPhrase("Doum hip drops", "Doum", 3,
            "hip_drops_double",
            "belly_a_dropA",
            "belly_a_dropB",
            "accent_belly_pop_1",
            "accent_belly_pop_2");

        AddPhrase("Tek rolls", "Tek", 2,
            "accent_belly_roll_1",
            "accent_belly_roll_2",
            "belly_roll_slow",
            "belly_roll_varied",
            "belly_a_roll_open",
            "belly_a_roll_mid");

        AddPhrase("Ka sways", "Ka", 2,
            "accent_belly_sway_1",
            "accent_belly_sway_2",
            "belly_a_undulation",
            "belly_b_open",
            "belly_b_returnA",
            "belly_b_returnB");

        AddPhrase("Trillo ornaments", "Trillo", 1,
            "accent_bellydance_1",
            "accent_bellydance_2",
            "accent_bellydance_3",
            "accent_bellydance_4",
            "accent_bellydance_5",
            "accent_bellydance_6",
            "accent_bellydance_7",
            "accent_belly_final_1",
            "accent_belly_final_2");
    }

    private void AddPhrase(string phraseName, string preferredStroke, int weight, params string[] states)
    {
        PhrasePools.Add(new DancePhrase
        {
            Name = phraseName,
            PreferredStroke = preferredStroke,
            Weight = weight,
            States = new List<string>(states)
        });
    }

    void Update()
    {
        HandleRecordingShortcuts();
        ReplayRecordedPattern();

        while (triggerQueue.TryDequeue(out string strokeType))
        {
            if (IgnoreLiveInputDuringPlayback && PlaybackRecordedPattern)
            {
                continue;
            }

            ProcessStroke(strokeType, true);
        }

        if (Mode == DanceMode.BufferedPhrases)
        {
            TryFlushPhraseBuffer();
        }

        UpdateBeatPulse();

        if ((Mode == DanceMode.ImmediateReactions || !HoldPhraseUntilNextPhrase) && isDancing && returnToIdleTime > 0f && Time.time >= returnToIdleTime)
        {
            ReturnToIdle();
        }
    }

    private void ProcessStroke(string strokeType, bool canRecord)
    {
        if (canRecord && RecordLiveInput)
        {
            RecordStroke(strokeType);
        }

        if (Mode == DanceMode.ImmediateReactions)
        {
            TryPlayRandomDance(strokeType);
        }
        else
        {
            BufferStroke(strokeType);
        }
    }

    private void TryPlayRandomDance(string strokeType)
    {
        if (LolaAnimator == null)
        {
            Debug.LogWarning("LolaAnimator not assigned.");
            return;
        }

        if (Time.time - lastMoveTime < MinimumSecondsBetweenMoves)
        {
            return;
        }

        List<string> pool = GetPoolForStroke(strokeType);

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"No clips defined for stroke '{strokeType}'.");
            return;
        }

        string chosenState = PickRandomClip(pool, strokeType);

        Debug.Log($"{strokeType} -> {chosenState}");

        // Play selected dance reaction smoothly from the beginning.
        if (!CrossFadeState(chosenState, DanceCrossfade))
        {
            return;
        }

        lastPlayedClip[strokeType] = chosenState;
        lastMoveTime = Time.time;

        // Fixed reaction time. This avoids broken clip-length warnings.
        returnToIdleTime = Time.time + ReactionSeconds;
        isDancing = true;
    }

    private void ReturnToIdle()
    {
        if (LolaAnimator == null)
        {
            return;
        }

        Debug.Log("Returning to idle: " + IdleStateName);

        CrossFadeState(IdleStateName, ReturnCrossfade);

        isDancing = false;
        returnToIdleTime = -1f;
    }

    [ContextMenu("Recording/Start Recording")]
    private void StartRecording()
    {
        RecordedPattern.Clear();
        recordingStartTime = Time.time;
        RecordLiveInput = true;
        PlaybackRecordedPattern = false;
        playbackIndex = 0;
        playbackStartTime = -1f;
        Debug.Log("Recording drum pattern. Press R again to stop.");
    }

    [ContextMenu("Recording/Stop Recording")]
    private void StopRecording()
    {
        RecordLiveInput = false;
        recordingStartTime = -1f;
        Debug.Log($"Stopped recording drum pattern: {RecordedPattern.Count} hits.");
    }

    [ContextMenu("Recording/Start Playback")]
    private void StartRecordedPlayback()
    {
        if (RecordedPattern.Count == 0)
        {
            Debug.LogWarning("No recorded drum pattern yet. Press R, play the drum, then press R again.");
            return;
        }

        RecordLiveInput = false;
        PlaybackRecordedPattern = true;
        playbackIndex = 0;
        playbackStartTime = Time.time;
        ClearPhraseBuffer();
        Debug.Log($"Playing recorded drum pattern: {RecordedPattern.Count} hits.");
    }

    [ContextMenu("Recording/Stop Playback")]
    private void StopRecordedPlayback()
    {
        PlaybackRecordedPattern = false;
        playbackIndex = 0;
        playbackStartTime = -1f;
        ClearPhraseBuffer();
        Debug.Log("Stopped recorded drum pattern playback.");
    }

    [ContextMenu("Recording/Clear Recording")]
    private void ClearRecordedPattern()
    {
        RecordedPattern.Clear();
        RecordLiveInput = false;
        PlaybackRecordedPattern = false;
        recordingStartTime = -1f;
        playbackStartTime = -1f;
        playbackIndex = 0;
        ClearPhraseBuffer();
        Debug.Log("Cleared recorded drum pattern.");
    }

    private void HandleRecordingShortcuts()
    {
        if (Input.GetKeyDown(ToggleRecordingKey))
        {
            if (RecordLiveInput)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        if (Input.GetKeyDown(TogglePlaybackKey))
        {
            if (PlaybackRecordedPattern)
            {
                StopRecordedPlayback();
            }
            else
            {
                StartRecordedPlayback();
            }
        }

        if (Input.GetKeyDown(ClearRecordingKey))
        {
            ClearRecordedPattern();
        }
    }

    private void RecordStroke(string strokeType)
    {
        if (recordingStartTime < 0f)
        {
            recordingStartTime = Time.time;
        }

        RecordedPattern.Add(new RecordedStroke(strokeType, Time.time - recordingStartTime));
    }

    private void ReplayRecordedPattern()
    {
        if (!PlaybackRecordedPattern)
        {
            return;
        }

        if (RecordedPattern.Count == 0)
        {
            StopRecordedPlayback();
            return;
        }

        if (playbackStartTime < 0f)
        {
            playbackStartTime = Time.time;
            playbackIndex = 0;
        }

        float playbackTime = Time.time - playbackStartTime - PlaybackStartDelay;

        if (playbackTime < 0f)
        {
            return;
        }

        while (playbackIndex < RecordedPattern.Count && RecordedPattern[playbackIndex].Time <= playbackTime)
        {
            ProcessStroke(RecordedPattern[playbackIndex].Type, false);
            playbackIndex++;
        }

        if (playbackIndex >= RecordedPattern.Count)
        {
            if (LoopRecordedPattern)
            {
                playbackStartTime = Time.time;
                playbackIndex = 0;
                ClearPhraseBuffer();
            }
            else
            {
                StopRecordedPlayback();
            }
        }
    }

    private void ClearPhraseBuffer()
    {
        phraseHits.Clear();
        phraseStartTime = -1f;
        lastHitTime = -1f;
    }

    private List<string> GetPoolForStroke(string strokeType)
    {
        switch (strokeType)
        {
            case "Doum":
                return DoumDanceStates;

            case "Tek":
                return TekDanceStates;

            case "Ka":
                return KaDanceStates;

            case "Trillo":
                return TrilloDanceStates;

            default:
                return null;
        }
    }

    private string PickRandomClip(List<string> pool, string strokeType)
    {
        if (pool.Count == 1)
        {
            return pool[0];
        }

        string lastClip = lastPlayedClip.ContainsKey(strokeType) ? lastPlayedClip[strokeType] : null;

        string chosen = pool[UnityEngine.Random.Range(0, pool.Count)];

        if (AvoidImmediateRepeats)
        {
            int attempts = 0;

            while (chosen == lastClip && attempts < 10)
            {
                chosen = pool[UnityEngine.Random.Range(0, pool.Count)];
                attempts++;
            }
        }

        return chosen;
    }

    private void BufferStroke(string strokeType)
    {
        float now = Time.time;

        if (phraseHits.Count == 0)
        {
            phraseStartTime = now;
        }

        phraseHits.Add(new StrokeHit(strokeType, now));
        lastHitTime = now;
        ApplyBeatPulse(strokeType);
    }

    private void TryFlushPhraseBuffer()
    {
        if (phraseHits.Count < MinimumHitsForPhrase)
        {
            return;
        }

        float now = Time.time;
        bool bufferFull = phraseStartTime > 0f && now - phraseStartTime >= PhraseBufferSeconds;
        bool rhythmPaused = lastHitTime > 0f && now - lastHitTime >= SilenceFlushSeconds;
        bool phraseCanChange = now >= nextPhraseAllowedTime;

        if (!phraseCanChange || (!bufferFull && !rhythmPaused))
        {
            return;
        }

        PlayBufferedPhrase();
    }

    private void PlayBufferedPhrase()
    {
        string dominantStroke = GetDominantStroke();
        string chosenState = PickPhraseState(dominantStroke);

        if (string.IsNullOrEmpty(chosenState))
        {
            Debug.LogWarning("No buffered phrase state found. Add curated states to PhrasePools, or fill the legacy stroke pools.");
            phraseHits.Clear();
            return;
        }

        Debug.Log($"Phrase [{dominantStroke}, {phraseHits.Count} hits] -> {chosenState}");

        if (!CrossFadeState(chosenState, PhraseCrossfade))
        {
            phraseHits.Clear();
            return;
        }

        lastPhraseState = chosenState;
        lastMoveTime = Time.time;
        nextPhraseAllowedTime = Time.time + PhrasePlaySeconds;
        returnToIdleTime = Time.time + PhrasePlaySeconds;
        isDancing = true;

        phraseHits.Clear();
        phraseStartTime = -1f;
        lastHitTime = -1f;
    }

    private string GetDominantStroke()
    {
        int doum = 0;
        int tek = 0;
        int ka = 0;
        int trillo = 0;

        foreach (StrokeHit hit in phraseHits)
        {
            switch (hit.Type)
            {
                case "Doum":
                    doum++;
                    break;

                case "Tek":
                    tek++;
                    break;

                case "Ka":
                    ka++;
                    break;

                case "Trillo":
                    trillo++;
                    break;
            }
        }

        if (tek >= doum && tek >= ka && tek >= trillo)
        {
            return "Tek";
        }

        if (ka >= doum && ka >= tek && ka >= trillo)
        {
            return "Ka";
        }

        if (trillo >= doum && trillo >= tek && trillo >= ka)
        {
            return "Trillo";
        }

        return "Doum";
    }

    private string PickPhraseState(string dominantStroke)
    {
        List<DancePhrase> candidates = new List<DancePhrase>();

        foreach (DancePhrase phrase in PhrasePools)
        {
            if (phrase == null || phrase.States == null || phrase.States.Count == 0 || !PhraseHasExistingState(phrase))
            {
                continue;
            }

            bool strokeMatches = string.Equals(phrase.PreferredStroke, "Any", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(phrase.PreferredStroke, dominantStroke, StringComparison.OrdinalIgnoreCase);

            if (strokeMatches)
            {
                candidates.Add(phrase);
            }
        }

        if (candidates.Count > 0)
        {
            DancePhrase phrase = PickWeightedPhrase(candidates);
            return PickExistingStateFromList(phrase.States, lastPhraseState);
        }

        List<string> fallbackPool = GetPoolForStroke(dominantStroke);
        return PickExistingStateFromList(fallbackPool, lastPhraseState);
    }

    private DancePhrase PickWeightedPhrase(List<DancePhrase> candidates)
    {
        int totalWeight = 0;

        foreach (DancePhrase phrase in candidates)
        {
            totalWeight += Mathf.Max(1, phrase.Weight);
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);

        foreach (DancePhrase phrase in candidates)
        {
            roll -= Mathf.Max(1, phrase.Weight);

            if (roll < 0)
            {
                return phrase;
            }
        }

        return candidates[candidates.Count - 1];
    }

    private string PickStateFromList(List<string> states, string avoidState)
    {
        if (states == null || states.Count == 0)
        {
            return null;
        }

        if (states.Count == 1)
        {
            return states[0];
        }

        string chosen = states[UnityEngine.Random.Range(0, states.Count)];
        int attempts = 0;

        while (chosen == avoidState && attempts < 10)
        {
            chosen = states[UnityEngine.Random.Range(0, states.Count)];
            attempts++;
        }

        return chosen;
    }

    private bool CrossFadeState(string stateName, float crossfadeSeconds)
    {
        if (LolaAnimator == null)
        {
            Debug.LogWarning("LolaAnimator not assigned.");
            return false;
        }

        string layerName = LolaAnimator.GetLayerName(0);
        int shortStateHash = Animator.StringToHash(stateName);
        int fullStateHash = Animator.StringToHash($"{layerName}.{stateName}");

        if (!LolaAnimator.HasState(0, shortStateHash) && !LolaAnimator.HasState(0, fullStateHash))
        {
            Debug.LogWarning($"Animator state '{stateName}' was not found on layer 0. Check spelling or run Tools > Raqs > Add Belly FBX States To Selected Controller.");
            return false;
        }

        LolaAnimator.CrossFadeInFixedTime(stateName, crossfadeSeconds, 0, 0f);
        return true;
    }

    private string PickExistingStateFromList(List<string> states, string avoidState)
    {
        List<string> existingStates = new List<string>();

        if (states == null)
        {
            return null;
        }

        foreach (string stateName in states)
        {
            if (HasAnimatorState(stateName))
            {
                existingStates.Add(stateName);
            }
        }

        if (existingStates.Count == 0)
        {
            return null;
        }

        return PickStateFromList(existingStates, avoidState);
    }

    private bool PhraseHasExistingState(DancePhrase phrase)
    {
        foreach (string stateName in phrase.States)
        {
            if (HasAnimatorState(stateName))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAnimatorState(string stateName)
    {
        if (LolaAnimator == null || string.IsNullOrEmpty(stateName))
        {
            return false;
        }

        string layerName = LolaAnimator.GetLayerName(0);
        int shortStateHash = Animator.StringToHash(stateName);
        int fullStateHash = Animator.StringToHash($"{layerName}.{stateName}");

        return LolaAnimator.HasState(0, shortStateHash) || LolaAnimator.HasState(0, fullStateHash);
    }

    private void ApplyBeatPulse(string strokeType)
    {
        if (!PulseAnimatorSpeedOnHits || LolaAnimator == null)
        {
            return;
        }

        switch (strokeType)
        {
            case "Doum":
                targetAnimatorSpeed = Mathf.Max(targetAnimatorSpeed, DoumSpeedPulse);
                break;

            case "Tek":
                targetAnimatorSpeed = Mathf.Max(targetAnimatorSpeed, TekSpeedPulse);
                break;

            case "Ka":
                targetAnimatorSpeed = Mathf.Max(targetAnimatorSpeed, KaSpeedPulse);
                break;

            case "Trillo":
                targetAnimatorSpeed = Mathf.Max(targetAnimatorSpeed, TrilloSpeedPulse);
                break;
        }
    }

    private void UpdateBeatPulse()
    {
        if (LolaAnimator == null)
        {
            return;
        }

        if (!PulseAnimatorSpeedOnHits)
        {
            LolaAnimator.speed = 1f;
            targetAnimatorSpeed = 1f;
            return;
        }

        LolaAnimator.speed = targetAnimatorSpeed;
        targetAnimatorSpeed = Mathf.Lerp(targetAnimatorSpeed, 1f, Time.deltaTime * BeatPulseReturnSpeed);
    }

    private void OnDoumReceived(OscMessageValues values)
    {
        triggerQueue.Enqueue("Doum");
    }

    private void OnTekReceived(OscMessageValues values)
    {
        triggerQueue.Enqueue("Tek");
    }

    private void OnKaReceived(OscMessageValues values)
    {
        triggerQueue.Enqueue("Ka");
    }

    private void OnTrilloReceived(OscMessageValues values)
    {
        triggerQueue.Enqueue("Trillo");
    }

    void OnDestroy()
    {
        if (server != null)
        {
            server.Dispose();
            server = null;
        }
    }
}
