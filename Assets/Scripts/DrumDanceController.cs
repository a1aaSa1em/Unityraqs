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
        BufferedPhrases,
        RhythmPatterns
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

        [Tooltip("Optional cue this phrase is best for: Any, Doum, Tek, Ka, Trillo, Sparse, Groove, Burst, or Roll.")]
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

    [Serializable]
    public class CharacterOption
    {
        [Tooltip("Label used by the on-screen switch button.")]
        public string Name = "Character";

        [Tooltip("Root GameObject to show/hide when switching.")]
        public GameObject Root;

        [Tooltip("Animator that receives the drum-driven dance states.")]
        public Animator Animator;
    }

    [Serializable]
    public class StrokeCountPhrase
    {
        [Tooltip("Inspector label only.")]
        public string Name = "8-hit phrase";

        [Tooltip("When the current rhythm buffer reaches this many strokes, this phrase group can fire.")]
        [Min(1)]
        public int StrokeCount = 8;

        [Tooltip("Animator state names for this stroke-count phrase. Keep these visually strong.")]
        public List<string> States = new List<string>();

        [Tooltip("Higher values make this phrase more likely when multiple groups use the same count.")]
        [Min(1)]
        public int Weight = 1;

        [Tooltip("Optional dominant stroke filter: Any, Doum, Tek, Ka, Trillo, Sparse, Groove, Burst, or Roll.")]
        public string PreferredStroke = "Any";
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

    private struct StrokeComposition
    {
        public int Doum;
        public int Tek;
        public int Ka;
        public int Trillo;

        public int Total => Doum + Tek + Ka + Trillo;
        public int DoumTekTotal => Doum + Tek;
    }

    private class RuntimeCharacterSlot
    {
        public string Name;
        public GameObject Root;
        public Animator Animator;
    }

    [Header("References")]
    public Animator LolaAnimator;

    [Tooltip("Controller assigned to every switchable character. Leave empty to reuse the current target animator's controller.")]
    public RuntimeAnimatorController SharedAnimatorController;

    [Tooltip("Must exactly match the idle state name in the Animator.")]
    public string IdleStateName = "Idle";

    [Header("Character Switching")]
    [Tooltip("Characters that can receive the same drum-driven dance moves.")]
    public List<CharacterOption> Characters = new List<CharacterOption>();

    [Tooltip("When enabled, auto-finds scene characters by name, including disabled Lola.")]
    public bool AutoFindSceneCharacters = true;

    [Tooltip("Scene object names to auto-find for the character switch buttons.")]
    public List<string> AutoFindCharacterNames = new List<string>
    {
        "Ch14_nonPBR",
        "Lola Bunny",
        "Lola Bunny (disabled)",
        "Elmo Rigged"
    };

    [Tooltip("Only the selected character stays visible.")]
    public bool HideInactiveCharacters = true;

    [Tooltip("Draws small character switch buttons in the Game view during Play Mode.")]
    public bool ShowCharacterSwitchButtons = true;

    [Tooltip("Keyboard shortcut for cycling to the next character.")]
    public KeyCode NextCharacterKey = KeyCode.Tab;

    [Tooltip("Allow number keys 1-9 to switch directly to a character.")]
    public bool EnableNumberKeyCharacterSwitching = true;

    [Header("OSC")]
    public int Port = 7000;

    [Header("Live Input Noise Filter")]
    [Tooltip("Ignores likely duplicate/noise OSC hits before they enter the phrase buffer.")]
    public bool FilterLikelyNoiseHits = true;

    [Tooltip("Fastest accepted spacing between any two live OSC hits from Max.")]
    public float MinimumSecondsBetweenLiveHits = 0.06f;

    [Tooltip("Fastest accepted spacing between repeated live hits of the same type.")]
    public float MinimumSecondsBetweenSameLiveHit = 0.10f;

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

    [Tooltip("Starts a phrase as soon as the first valid hit arrives. Keep this on for live demos so the character visibly reacts to sound.")]
    public bool PlayPhraseImmediatelyOnHit = true;

    [Tooltip("How long a phrase should play before the next buffered phrase is allowed to replace it.")]
    public float PhrasePlaySeconds = 2.5f;

    [Tooltip("Blend time when moving between full dance phrases.")]
    public float PhraseCrossfade = 0.45f;

    [Tooltip("Keep the last phrase moving while the next buffer is being collected.")]
    public bool HoldPhraseUntilNextPhrase = true;

    [Header("Idle Recovery")]
    [Tooltip("When the drum stops, smoothly return to the configured idle state even if HoldPhraseUntilNextPhrase is enabled.")]
    public bool ReturnToIdleWhenSilent = true;

    [Tooltip("How much silence after the last accepted drum hit before returning to idle.")]
    public float SecondsOfSilenceBeforeIdle = 1.0f;

    [Tooltip("Minimum time to let a newly-started move breathe before silence can pull it back to idle.")]
    public float MinimumDanceSecondsBeforeIdle = 0.45f;

    [Header("Smooth Transitions")]
    [Tooltip("Shortest blend used when changing from one dance move to another.")]
    public float MinimumMoveCrossfade = 0.35f;

    [Tooltip("Shortest blend used when returning to idle.")]
    public float MinimumIdleCrossfade = 0.6f;

    [Header("Rhythm Pattern Mode")]
    [Tooltip("Hits closer than this are treated as part of a burst or roll.")]
    public float FastGapSeconds = 0.22f;

    [Tooltip("Average gap below this, with enough hits, counts as a roll.")]
    public float RollAverageGapSeconds = 0.18f;

    [Tooltip("How many hits make a dense roll pattern.")]
    public int RollMinimumHits = 5;

    [Tooltip("How many hits make a short burst pattern.")]
    public int BurstMinimumHits = 3;

    [Tooltip("Treat very fast repeated hits as Trillo even if Max sends them as /doum.")]
    public bool InferTrilloFromFastHits = true;

    [Tooltip("Average gap below this is interpreted as Trillo/roll timing.")]
    public float InferredTrilloAverageGapSeconds = 0.16f;

    [Tooltip("At least this many fast gaps are needed before a phrase is treated as Trillo.")]
    public int InferredTrilloMinimumFastGaps = 4;

    [Header("Stroke Count Choreography")]
    [Tooltip("When enabled, the controller fires bigger showcase moves after exact stroke counts such as 4, 8, 12, or 16.")]
    public bool UseStrokeCountPhrases = true;

    [Tooltip("If true, hitting a configured stroke count plays that phrase immediately, instead of waiting for silence or the full buffer window.")]
    public bool PlayCountPhraseAsSoonAsReady = true;

    [Tooltip("Default number of strokes that triggers a showcase move. Set to 8 for 'every 8 strokes, play a bigger phrase'.")]
    [Min(1)]
    public int CountedPhraseHitTarget = 8;

    [Tooltip("Maximum count to keep in the current rhythm phrase before forcing a phrase selection.")]
    public int MaximumCountedPhraseHits = 16;

    [Tooltip("Curated phrase pools selected by number of strokes in the current phrase.")]
    public List<StrokeCountPhrase> StrokeCountPhrases = new List<StrokeCountPhrase>();

    [Tooltip("If StrokeCountPhrases is empty at runtime, automatically add the Ch29 stroke-count showcase presets.")]
    public bool AutoFillCountShowcaseIfEmpty = true;

    [Header("Showcase Rules")]
    [Tooltip("Every this many doum hits, play the dedicated doum accent state.")]
    [Min(1)]
    public int DoumPairMoveEvery = 2;

    [Tooltip("Animator state used once for each doum pair.")]
    public string DoumPairState = "Ch29_nonPBR_Dancing_phrase_01";

    [Tooltip("Fast-hit showcase states. Consecutive pairs or triplets are chosen from this list.")]
    public List<string> FastConsecutiveStates = new List<string>
    {
        "Ch29_nonPBR_Dancing_phrase_05",
        "Ch29_nonPBR_Dancing_phrase_06",
        "Ch29_nonPBR_Dancing_phrase_07",
        "Ch29_nonPBR_Dancing_phrase_08",
        "Ch29_nonPBR_Dancing_phrase_09",
        "Ch29_nonPBR_Dancing_phrase_10"
    };

    [Tooltip("Minimum fast hits before playing a consecutive fast-hit pair.")]
    public int FastConsecutiveMinimumHits = 5;

    [Tooltip("At this many fast hits, play three consecutive clips instead of two.")]
    public int FastConsecutiveLongMinimumHits = 9;

    [Tooltip("Time between each consecutive fast-hit clip.")]
    public float FastConsecutivePhraseSeconds = 0.52f;

    [Tooltip("Blend time between consecutive fast-hit clips.")]
    public float FastConsecutiveCrossfade = 0.16f;

    [Header("Curated Phrase Pools")]
    [Tooltip("Use these for grounded hip/belly material. Add only clips that look related.")]
    public List<DancePhrase> PhrasePools = new List<DancePhrase>();

    [Tooltip("When enabled, strips every animation choice that is not one of the latest Ch29 sliced phrase clips.")]
    public bool UseLatestSlicedCh29ClipsOnly = true;

    [Header("Beat Response")]
    [Tooltip("Normal animation playback speed. Raise this if retargeted clips feel too slow on the current character.")]
    public float BaseAnimatorSpeed = 1.0f;

    [Tooltip("Makes each incoming hit briefly push the current animation speed so the dance still reacts on the beat.")]
    public bool PulseAnimatorSpeedOnHits = true;

    [Tooltip("How quickly the animation speed settles back to normal after a hit.")]
    public float BeatPulseReturnSpeed = 8f;

    public float DoumSpeedPulse = 1.35f;
    public float TekSpeedPulse = 1.18f;
    public float KaSpeedPulse = 1.22f;
    public float TrilloSpeedPulse = 1.45f;

    [Header("Debug")]
    [Tooltip("Logs each OSC hit as soon as Unity receives it, before phrase buffering.")]
    public bool LogIncomingOsc = true;

    [Tooltip("During Play Mode, press D/T/K/L to test Doum/Tek/Ka/Trillo without Max.")]
    public bool EnableKeyboardDebug = true;

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
    public float DanceCrossfade = 0.3f;

    [Tooltip("Blend time when returning to idle.")]
    public float ReturnCrossfade = 0.6f;

    [Tooltip("Prevents Lola from changing moves too rapidly.")]
    public float MinimumSecondsBetweenMoves = 0.25f;

    [Header("Randomisation")]
    [Tooltip("Avoid choosing the same clip twice in a row for the same stroke.")]
    public bool AvoidImmediateRepeats = true;

    private OscServer server;
    private ConcurrentQueue<string> triggerQueue = new ConcurrentQueue<string>();

    private Dictionary<string, string> lastPlayedClip = new Dictionary<string, string>();
    private Dictionary<string, double> lastAcceptedLiveHitByType = new Dictionary<string, double>();
    private List<StrokeHit> phraseHits = new List<StrokeHit>();
    private List<RuntimeCharacterSlot> runtimeCharacters = new List<RuntimeCharacterSlot>();

    private readonly System.Diagnostics.Stopwatch liveHitClock = System.Diagnostics.Stopwatch.StartNew();
    private readonly object liveHitFilterLock = new object();

    private RuntimeAnimatorController resolvedSharedAnimatorController;
    private int activeCharacterIndex = -1;
    private double lastAcceptedLiveHitTime = -999.0;
    private int rejectedLiveHitCount = 0;

    private float returnToIdleTime = -1f;
    private float lastMoveTime = -999f;
    private float phraseStartTime = -1f;
    private float lastHitTime = -1f;
    private float lastAcceptedStrokeTime = -999f;
    private float nextPhraseAllowedTime = -999f;
    private float targetAnimatorSpeed = 1f;
    private float recordingStartTime = -1f;
    private float playbackStartTime = -1f;
    private int playbackIndex = 0;
    private string lastPhraseState = null;
    private string lastCountPhraseState = null;
    private bool isDancing = false;
    private List<string> queuedShowcaseStates = new List<string>();
    private int queuedShowcaseIndex = 0;
    private float nextQueuedShowcaseTime = -1f;

    void Start()
    {
        targetAnimatorSpeed = BaseAnimatorSpeed;
        InitializeCharacterSwitcher();

        if (AutoFillCountShowcaseIfEmpty && UseStrokeCountPhrases && (StrokeCountPhrases == null || StrokeCountPhrases.Count == 0))
        {
            FillCh29StrokeCountShowcase();
        }

        if (UseLatestSlicedCh29ClipsOnly)
        {
            KeepLatestSlicedCh29ClipsOnly();
        }

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
        FillCh29PhrasePools();
        KeepLatestSlicedCh29ClipsOnly();
    }

    [ContextMenu("Fill Ch29 Phrase Pools")]
    private void FillCh29PhrasePools()
    {
        PhrasePools.Clear();

        AddPhrase("Ch29 neutral flow", "Any", 3,
            "Ch29_nonPBR_Belly_Dance_phrase_01",
            "Ch29_nonPBR_Belly_Dance_phrase_03",
            "Ch29_nonPBR_Belly_Dance_phrase_04",
            "Ch29_nonPBR_Belly_Dance_phrase_05",
            "Ch29_nonPBR_Belly_Dance_phrase_07",
            "Ch29_nonPBR_Belly_Dance_phrase_09",
            "Ch29_nonPBR_Belly_Dance_phrase_10",
            "Ch29_nonPBR_Bellydancing_phrase_01",
            "Ch29_nonPBR_Bellydancing_phrase_03",
            "Ch29_nonPBR_Bellydancing_phrase_04",
            "Ch29_nonPBR_Bellydancing_phrase_06",
            "Ch29_nonPBR_Bellydancing_phrase_11",
            "Ch29_nonPBR_Bellydancing_phrase_12",
            "Ch29_nonPBR_Bellydancing_phrase_13",
            "Ch29_nonPBR_Belly_Dance_1_phrase_01",
            "Ch29_nonPBR_Belly_Dance_1_phrase_02",
            "Ch29_nonPBR_Belly_Dance_1_phrase_03",
            "Ch29_nonPBR_Belly_Dance_1_phrase_04",
            "Ch29_nonPBR_Belly_Dance_1_phrase_05");

        AddPhrase("Ch29 grounded hip drops", "Doum", 5,
            "Ch29_nonPBR_Belly_Dance_phrase_02",
            "Ch29_nonPBR_Bellydancing_phrase_02",
            "Ch29_nonPBR_Bellydancing_phrase_05",
            "Ch29_nonPBR_Bellydancing_phrase_10");

        AddPhrase("Ch29 sharp tek accents", "Tek", 4,
            "Ch29_nonPBR_Dancing_phrase_01",
            "Ch29_nonPBR_Dancing_phrase_02",
            "Ch29_nonPBR_Dancing_phrase_03",
            "Ch29_nonPBR_Dancing_phrase_04",
            "Ch29_nonPBR_Dancing_phrase_05",
            "Ch29_nonPBR_Dancing_phrase_06");

        AddPhrase("Ch29 fast shimmy and roll phrases", "Trillo", 5,
            "Ch29_nonPBR_Belly_Dance_phrase_06",
            "Ch29_nonPBR_Belly_Dance_phrase_08",
            "Ch29_nonPBR_Bellydancing_phrase_07",
            "Ch29_nonPBR_Bellydancing_phrase_08",
            "Ch29_nonPBR_Bellydancing_phrase_09",
            "Ch29_nonPBR_Dancing_phrase_07",
            "Ch29_nonPBR_Dancing_phrase_08",
            "Ch29_nonPBR_Dancing_phrase_09",
            "Ch29_nonPBR_Dancing_phrase_10",
            "Ch29_nonPBR_Dancing_phrase_11",
            "Ch29_nonPBR_Belly_Dance_1_phrase_06",
            "Ch29_nonPBR_Belly_Dance_1_phrase_07",
            "Ch29_nonPBR_Belly_Dance_1_phrase_08",
            "Ch29_nonPBR_Belly_Dance_1_phrase_09",
            "Ch29_nonPBR_Belly_Dance_1_phrase_10");
    }

    [ContextMenu("Fill Ch29 Rhythm Pattern Pools")]
    private void FillCh29RhythmPatternPools()
    {
        PhrasePools.Clear();

        AddPhrase("Flow phrases", "Any", 3,
            "Ch29_nonPBR_Belly_Dance_phrase_01",
            "Ch29_nonPBR_Belly_Dance_phrase_02",
            "Ch29_nonPBR_Belly_Dance_phrase_03",
            "Ch29_nonPBR_Belly_Dance_phrase_04",
            "Ch29_nonPBR_Belly_Dance_phrase_05",
            "Ch29_nonPBR_Belly_Dance_phrase_06",
            "Ch29_nonPBR_Belly_Dance_phrase_07",
            "Ch29_nonPBR_Belly_Dance_phrase_08",
            "Ch29_nonPBR_Belly_Dance_phrase_09",
            "Ch29_nonPBR_Belly_Dance_phrase_10");

        AddPhrase("Sparse phrases", "Sparse", 4,
            "Ch29_nonPBR_Belly_Dance_phrase_01",
            "Ch29_nonPBR_Belly_Dance_phrase_03",
            "Ch29_nonPBR_Belly_Dance_phrase_05",
            "Ch29_nonPBR_Bellydancing_phrase_01",
            "Ch29_nonPBR_Bellydancing_phrase_04",
            "Ch29_nonPBR_Bellydancing_phrase_08");

        AddPhrase("Groove phrases", "Groove", 5,
            "Ch29_nonPBR_Bellydancing_phrase_02",
            "Ch29_nonPBR_Bellydancing_phrase_03",
            "Ch29_nonPBR_Bellydancing_phrase_05",
            "Ch29_nonPBR_Bellydancing_phrase_06",
            "Ch29_nonPBR_Bellydancing_phrase_07",
            "Ch29_nonPBR_Bellydancing_phrase_09",
            "Ch29_nonPBR_Bellydancing_phrase_10",
            "Ch29_nonPBR_Bellydancing_phrase_11",
            "Ch29_nonPBR_Bellydancing_phrase_12",
            "Ch29_nonPBR_Bellydancing_phrase_13");

        AddPhrase("Burst phrases", "Burst", 4,
            "Ch29_nonPBR_Dancing_phrase_01",
            "Ch29_nonPBR_Dancing_phrase_02",
            "Ch29_nonPBR_Dancing_phrase_03",
            "Ch29_nonPBR_Dancing_phrase_04",
            "Ch29_nonPBR_Dancing_phrase_05",
            "Ch29_nonPBR_Dancing_phrase_06");

        AddPhrase("Roll phrases", "Roll", 4,
            "Ch29_nonPBR_Dancing_phrase_07",
            "Ch29_nonPBR_Dancing_phrase_08",
            "Ch29_nonPBR_Dancing_phrase_09",
            "Ch29_nonPBR_Dancing_phrase_10",
            "Ch29_nonPBR_Dancing_phrase_11",
            "Ch29_nonPBR_Belly_Dance_1_phrase_06",
            "Ch29_nonPBR_Belly_Dance_1_phrase_07",
            "Ch29_nonPBR_Belly_Dance_1_phrase_08",
            "Ch29_nonPBR_Belly_Dance_1_phrase_09",
            "Ch29_nonPBR_Belly_Dance_1_phrase_10");
    }

    [ContextMenu("Fill Ch29 Stroke Count Showcase")]
    private void FillCh29StrokeCountShowcase()
    {
        StrokeCountPhrases.Clear();

        AddCountPhrase("4-hit balanced setup", 4, "DoumTek", 4,
            "Ch29_nonPBR_Belly_Dance_phrase_01",
            "Ch29_nonPBR_Belly_Dance_phrase_03",
            "Ch29_nonPBR_Bellydancing_phrase_01",
            "Ch29_nonPBR_Bellydancing_phrase_04");

        AddCountPhrase("4-hit doum weight shift", 4, "Doum", 5,
            "Ch29_nonPBR_Belly_Dance_phrase_02",
            "Ch29_nonPBR_Bellydancing_phrase_02",
            "Ch29_nonPBR_Bellydancing_phrase_05");

        AddCountPhrase("4-hit tek accent setup", 4, "Tek", 5,
            "Ch29_nonPBR_Dancing_phrase_01",
            "Ch29_nonPBR_Dancing_phrase_02",
            "Ch29_nonPBR_Dancing_phrase_03");

        AddCountPhrase("8-hit neutral flow", 8, "Any", 3,
            "Ch29_nonPBR_Belly_Dance_phrase_04",
            "Ch29_nonPBR_Belly_Dance_phrase_05",
            "Ch29_nonPBR_Belly_Dance_phrase_07",
            "Ch29_nonPBR_Bellydancing_phrase_03",
            "Ch29_nonPBR_Bellydancing_phrase_06",
            "Ch29_nonPBR_Belly_Dance_1_phrase_01",
            "Ch29_nonPBR_Belly_Dance_1_phrase_02",
            "Ch29_nonPBR_Belly_Dance_1_phrase_03");

        AddCountPhrase("8-hit doum tek combo", 8, "DoumTek", 6,
            "Ch29_nonPBR_Belly_Dance_phrase_04",
            "Ch29_nonPBR_Belly_Dance_phrase_07",
            "Ch29_nonPBR_Bellydancing_phrase_03",
            "Ch29_nonPBR_Bellydancing_phrase_06",
            "Ch29_nonPBR_Belly_Dance_1_phrase_01",
            "Ch29_nonPBR_Belly_Dance_1_phrase_03");

        AddCountPhrase("8-hit doum drops", 8, "Doum", 4,
            "Ch29_nonPBR_Belly_Dance_phrase_02",
            "Ch29_nonPBR_Bellydancing_phrase_02",
            "Ch29_nonPBR_Bellydancing_phrase_05",
            "Ch29_nonPBR_Bellydancing_phrase_10");

        AddCountPhrase("8-hit tek accents", 8, "Tek", 4,
            "Ch29_nonPBR_Dancing_phrase_01",
            "Ch29_nonPBR_Dancing_phrase_03",
            "Ch29_nonPBR_Dancing_phrase_05",
            "Ch29_nonPBR_Dancing_phrase_06");

        AddCountPhrase("8-hit trillo roll", 8, "Trillo", 5,
            "Ch29_nonPBR_Belly_Dance_phrase_06",
            "Ch29_nonPBR_Belly_Dance_phrase_08",
            "Ch29_nonPBR_Bellydancing_phrase_07",
            "Ch29_nonPBR_Bellydancing_phrase_08",
            "Ch29_nonPBR_Bellydancing_phrase_09",
            "Ch29_nonPBR_Dancing_phrase_07",
            "Ch29_nonPBR_Dancing_phrase_08",
            "Ch29_nonPBR_Dancing_phrase_09",
            "Ch29_nonPBR_Belly_Dance_1_phrase_06",
            "Ch29_nonPBR_Belly_Dance_1_phrase_07");

        AddCountPhrase("8-hit roll flourish", 8, "Roll", 5,
            "Ch29_nonPBR_Dancing_phrase_08",
            "Ch29_nonPBR_Dancing_phrase_10",
            "Ch29_nonPBR_Dancing_phrase_11",
            "Ch29_nonPBR_Belly_Dance_1_phrase_08");

        AddCountPhrase("12-hit travelling phrase", 12, "Any", 3,
            "Ch29_nonPBR_Dancing_phrase_07",
            "Ch29_nonPBR_Dancing_phrase_08",
            "Ch29_nonPBR_Dancing_phrase_09",
            "Ch29_nonPBR_Belly_Dance_1_phrase_06");

        AddCountPhrase("12-hit doum tek travel", 12, "DoumTek", 5,
            "Ch29_nonPBR_Dancing_phrase_07",
            "Ch29_nonPBR_Dancing_phrase_09",
            "Ch29_nonPBR_Belly_Dance_1_phrase_06",
            "Ch29_nonPBR_Belly_Dance_1_phrase_07");

        AddCountPhrase("16-hit finale", 16, "Any", 2,
            "Ch29_nonPBR_Dancing_phrase_10",
            "Ch29_nonPBR_Dancing_phrase_11",
            "Ch29_nonPBR_Belly_Dance_1_phrase_08",
            "Ch29_nonPBR_Belly_Dance_1_phrase_09",
            "Ch29_nonPBR_Belly_Dance_1_phrase_10");

        AddCountPhrase("16-hit balanced finale", 16, "DoumTek", 5,
            "Ch29_nonPBR_Dancing_phrase_10",
            "Ch29_nonPBR_Dancing_phrase_11",
            "Ch29_nonPBR_Belly_Dance_1_phrase_08",
            "Ch29_nonPBR_Belly_Dance_1_phrase_09",
            "Ch29_nonPBR_Belly_Dance_1_phrase_10");
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

    private void AddCountPhrase(string phraseName, int strokeCount, string preferredStroke, int weight, params string[] states)
    {
        StrokeCountPhrases.Add(new StrokeCountPhrase
        {
            Name = phraseName,
            StrokeCount = strokeCount,
            PreferredStroke = preferredStroke,
            Weight = weight,
            States = new List<string>(states)
        });
    }

    [ContextMenu("Keep Latest Sliced Ch29 Clips Only")]
    private void KeepLatestSlicedCh29ClipsOnly()
    {
        FilterLatestSlicedClipList(DoumDanceStates);
        FilterLatestSlicedClipList(TekDanceStates);
        FilterLatestSlicedClipList(KaDanceStates);
        FilterLatestSlicedClipList(TrilloDanceStates);

        foreach (DancePhrase phrase in PhrasePools)
        {
            if (phrase != null)
            {
                FilterLatestSlicedClipList(phrase.States);
            }
        }

        foreach (StrokeCountPhrase phrase in StrokeCountPhrases)
        {
            if (phrase != null)
            {
                FilterLatestSlicedClipList(phrase.States);
            }
        }
    }

    private void FilterLatestSlicedClipList(List<string> states)
    {
        if (states == null)
        {
            return;
        }

        states.RemoveAll(stateName => !IsLatestSlicedCh29PhraseName(stateName));
    }

    private bool IsLatestSlicedCh29PhraseName(string stateName)
    {
        return !string.IsNullOrEmpty(stateName) &&
               stateName.StartsWith("Ch29_nonPBR_", StringComparison.Ordinal) &&
               stateName.Contains("_phrase_");
    }

    private void InitializeCharacterSwitcher()
    {
        resolvedSharedAnimatorController = SharedAnimatorController != null
            ? SharedAnimatorController
            : LolaAnimator != null ? LolaAnimator.runtimeAnimatorController : null;

        RefreshRuntimeCharacters();

        int initialIndex = FindCharacterIndexForAnimator(LolaAnimator);

        if (initialIndex < 0)
        {
            initialIndex = FindActiveCharacterIndex();
        }

        if (initialIndex < 0 && runtimeCharacters.Count > 0)
        {
            initialIndex = 0;
        }

        if (initialIndex >= 0)
        {
            SelectCharacter(initialIndex, false);
        }
    }

    private void RefreshRuntimeCharacters()
    {
        runtimeCharacters.Clear();

        foreach (CharacterOption option in Characters)
        {
            if (option == null)
            {
                continue;
            }

            Animator animator = option.Animator != null ? option.Animator : option.Root != null ? option.Root.GetComponentInChildren<Animator>(true) : null;
            GameObject root = option.Root != null ? option.Root : animator != null ? animator.gameObject : null;

            AddRuntimeCharacter(option.Name, root, animator);
        }

        if (AutoFindSceneCharacters)
        {
            foreach (string characterName in AutoFindCharacterNames)
            {
                GameObject root = FindSceneGameObjectByName(characterName);
                Animator animator = root != null ? root.GetComponentInChildren<Animator>(true) : null;
                AddRuntimeCharacter(characterName, root, animator);
            }
        }

        if (LolaAnimator != null)
        {
            AddRuntimeCharacter(CleanCharacterName(LolaAnimator.gameObject.name), LolaAnimator.gameObject, LolaAnimator);
        }
    }

    private void AddRuntimeCharacter(string displayName, GameObject root, Animator animator)
    {
        if (root == null && animator == null)
        {
            return;
        }

        if (animator == null && root != null)
        {
            animator = root.GetComponentInChildren<Animator>(true);
        }

        if (animator == null)
        {
            return;
        }

        if (root == null)
        {
            root = animator.gameObject;
        }

        foreach (RuntimeCharacterSlot existing in runtimeCharacters)
        {
            if (existing.Animator == animator || existing.Root == root)
            {
                return;
            }
        }

        EnsureAnimatorController(animator);

        runtimeCharacters.Add(new RuntimeCharacterSlot
        {
            Name = string.IsNullOrWhiteSpace(displayName) ? CleanCharacterName(root.name) : CleanCharacterName(displayName),
            Root = root,
            Animator = animator
        });
    }

    private void EnsureAnimatorController(Animator animator)
    {
        if (animator == null)
        {
            return;
        }

        if (animator.runtimeAnimatorController == null && resolvedSharedAnimatorController != null)
        {
            animator.runtimeAnimatorController = resolvedSharedAnimatorController;
        }

        if (resolvedSharedAnimatorController == null && animator.runtimeAnimatorController != null)
        {
            resolvedSharedAnimatorController = animator.runtimeAnimatorController;
        }
    }

    private GameObject FindSceneGameObjectByName(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            return null;
        }

        string cleanTarget = CleanCharacterName(targetName);
        GameObject bestMatch = null;

        foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (candidate == null || !candidate.scene.IsValid())
            {
                continue;
            }

            string cleanCandidate = CleanCharacterName(candidate.name);

            if (string.Equals(cleanCandidate, cleanTarget, StringComparison.OrdinalIgnoreCase))
            {
                if (candidate.GetComponentInChildren<Animator>(true) != null)
                {
                    return candidate;
                }

                bestMatch = bestMatch != null ? bestMatch : candidate;
            }
        }

        return bestMatch;
    }

    private string CleanCharacterName(string name)
    {
        return string.IsNullOrEmpty(name)
            ? "Character"
            : name.Replace(" (disabled)", string.Empty).Trim();
    }

    private int FindCharacterIndexForAnimator(Animator animator)
    {
        if (animator == null)
        {
            return -1;
        }

        for (int i = 0; i < runtimeCharacters.Count; i++)
        {
            if (runtimeCharacters[i].Animator == animator)
            {
                return i;
            }
        }

        return -1;
    }

    private int FindActiveCharacterIndex()
    {
        for (int i = 0; i < runtimeCharacters.Count; i++)
        {
            if (runtimeCharacters[i].Root != null && runtimeCharacters[i].Root.activeInHierarchy)
            {
                return i;
            }
        }

        return -1;
    }

    private void HandleCharacterSwitchInput()
    {
        if (runtimeCharacters.Count <= 1)
        {
            return;
        }

        if (NextCharacterKey != KeyCode.None && Input.GetKeyDown(NextCharacterKey))
        {
            SelectCharacter((activeCharacterIndex + 1 + runtimeCharacters.Count) % runtimeCharacters.Count, true);
        }

        if (!EnableNumberKeyCharacterSwitching)
        {
            return;
        }

        int keyCount = Mathf.Min(9, runtimeCharacters.Count);

        for (int i = 0; i < keyCount; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                SelectCharacter(i, true);
                return;
            }
        }
    }

    private void SelectCharacter(int index, bool blendToIdle)
    {
        if (index < 0 || index >= runtimeCharacters.Count)
        {
            return;
        }

        RuntimeCharacterSlot slot = runtimeCharacters[index];

        if (slot == null || slot.Animator == null)
        {
            return;
        }

        if (HideInactiveCharacters)
        {
            for (int i = 0; i < runtimeCharacters.Count; i++)
            {
                RuntimeCharacterSlot other = runtimeCharacters[i];

                if (other?.Root != null)
                {
                    other.Root.SetActive(i == index);
                }
            }
        }
        else if (slot.Root != null)
        {
            slot.Root.SetActive(true);
        }

        LolaAnimator = slot.Animator;
        EnsureAnimatorController(LolaAnimator);
        LolaAnimator.speed = BaseAnimatorSpeed;
        targetAnimatorSpeed = BaseAnimatorSpeed;
        activeCharacterIndex = index;
        isDancing = false;
        returnToIdleTime = -1f;
        ClearPhraseBuffer();

        if (blendToIdle)
        {
            CrossFadeState(IdleStateName, ReturnCrossfade);
        }
        else if (HasAnimatorState(IdleStateName))
        {
            LolaAnimator.Play(IdleStateName, 0, 0f);
        }

        Debug.Log("Active drum character: " + slot.Name);
    }

    private void OnGUI()
    {
        if (!ShowCharacterSwitchButtons || runtimeCharacters.Count <= 1)
        {
            return;
        }

        const float left = 16f;
        const float top = 16f;
        const float width = 190f;
        const float rowHeight = 30f;
        float height = 34f + runtimeCharacters.Count * rowHeight;

        GUI.Box(new Rect(left, top, width, height), "Character");

        for (int i = 0; i < runtimeCharacters.Count; i++)
        {
            RuntimeCharacterSlot slot = runtimeCharacters[i];
            string prefix = i == activeCharacterIndex ? "* " : $"{i + 1}. ";
            Rect buttonRect = new Rect(left + 10f, top + 26f + i * rowHeight, width - 20f, 24f);

            if (GUI.Button(buttonRect, prefix + slot.Name))
            {
                SelectCharacter(i, true);
            }
        }
    }

    void Update()
    {
        HandleCharacterSwitchInput();
        HandleRecordingShortcuts();
        HandleKeyboardDebug();
        ReplayRecordedPattern();
        UpdateQueuedShowcaseStates();

        while (triggerQueue.TryDequeue(out string strokeType))
        {
            if (IgnoreLiveInputDuringPlayback && PlaybackRecordedPattern)
            {
                continue;
            }

            ProcessStroke(strokeType, true);
        }

        if (Mode == DanceMode.BufferedPhrases || Mode == DanceMode.RhythmPatterns)
        {
            TryFlushPhraseBuffer();
        }

        UpdateBeatPulse();

        UpdateIdleRecovery();
    }

    private void HandleKeyboardDebug()
    {
        if (!EnableKeyboardDebug)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            ProcessStroke("Doum", false);
            Debug.Log("Keyboard debug hit: Doum");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            ProcessStroke("Tek", false);
            Debug.Log("Keyboard debug hit: Tek");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            ProcessStroke("Ka", false);
            Debug.Log("Keyboard debug hit: Ka");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ProcessStroke("Trillo", false);
            Debug.Log("Keyboard debug hit: Trillo");
        }
    }

    private void ProcessStroke(string strokeType, bool canRecord)
    {
        lastAcceptedStrokeTime = Time.time;

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

            bool countPhraseReady = IsCountPhraseReady();
            bool shouldPlayImmediateFallback = PlayPhraseImmediatelyOnHit && !HasUsableCountPhrases();

            if ((countPhraseReady || shouldPlayImmediateFallback) && phraseHits.Count >= MinimumHitsForPhrase && Time.time >= nextPhraseAllowedTime)
            {
                PlayBufferedPhrase();
            }
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
        targetAnimatorSpeed = BaseAnimatorSpeed;
        ClearPhraseBuffer();
    }

    private void UpdateIdleRecovery()
    {
        if (!isDancing)
        {
            return;
        }

        float now = Time.time;
        bool legacyIdleExpired = (Mode == DanceMode.ImmediateReactions || !HoldPhraseUntilNextPhrase) &&
                                 returnToIdleTime > 0f &&
                                 now >= returnToIdleTime;

        bool silenceIdleExpired = ReturnToIdleWhenSilent &&
                                  lastAcceptedStrokeTime > 0f &&
                                  now - lastAcceptedStrokeTime >= Mathf.Max(0f, SecondsOfSilenceBeforeIdle) &&
                                  now - lastMoveTime >= Mathf.Max(0f, MinimumDanceSecondsBeforeIdle);

        if (legacyIdleExpired || silenceIdleExpired)
        {
            ReturnToIdle();
        }
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
        ClearQueuedShowcaseStates();
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
        bool countPhraseReady = IsCountPhraseReady();
        bool bufferFull = phraseStartTime > 0f && now - phraseStartTime >= PhraseBufferSeconds;
        bool rhythmPaused = lastHitTime > 0f && now - lastHitTime >= SilenceFlushSeconds;
        bool countLimitReached = MaximumCountedPhraseHits > 0 && phraseHits.Count >= MaximumCountedPhraseHits;
        bool phraseCanChange = now >= nextPhraseAllowedTime;

        if (!phraseCanChange || (!countPhraseReady && !bufferFull && !rhythmPaused && !countLimitReached))
        {
            return;
        }

        PlayBufferedPhrase();
    }

    private void PlayBufferedPhrase()
    {
        StrokeComposition composition = GetStrokeComposition();
        string phraseCue = Mode == DanceMode.RhythmPatterns ? GetRhythmPatternCue() : GetShowcaseCue(composition);
        int rawHitCount = phraseHits.Count;
        int hitCount = GetCountPhraseSelectionCount(rawHitCount);
        string dominantStroke = GetDominantStroke();
        List<string> fastSequence = PickFastConsecutiveSequence(composition);
        string chosenState = null;
        string phraseSource = "count";

        if (IsDoumPairShowcase(composition))
        {
            chosenState = DoumPairState;
            phraseSource = "doum-pair";
        }
        else if (fastSequence.Count > 0)
        {
            PlayShowcaseSequence(fastSequence, rawHitCount, phraseCue, dominantStroke, composition);
            return;
        }
        else
        {
            chosenState = PickCountPhraseState(hitCount, phraseCue, dominantStroke);
        }

        if (string.IsNullOrEmpty(chosenState))
        {
            chosenState = PickPhraseState(phraseCue);
            phraseSource = "cue";
        }

        if (string.IsNullOrEmpty(chosenState))
        {
            Debug.LogWarning("No buffered phrase state found. Add curated states to PhrasePools, or fill the legacy stroke pools.");
            phraseHits.Clear();
            return;
        }

        Debug.Log($"Phrase [{phraseSource}: {rawHitCount} hits, selected count {hitCount}, cue {phraseCue}, dominant {dominantStroke}, doum {composition.Doum}, tek {composition.Tek}] -> {chosenState}");

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

    private bool IsCountPhraseReady()
    {
        if (!UseStrokeCountPhrases || !PlayCountPhraseAsSoonAsReady)
        {
            return false;
        }

        StrokeComposition composition = GetStrokeComposition();

        if (IsDoumPairShowcase(composition))
        {
            return true;
        }

        if (IsFastConsecutiveCandidate(composition))
        {
            return false;
        }

        if (StrokeCountPhrases == null || StrokeCountPhrases.Count == 0)
        {
            return false;
        }

        int hitCount = phraseHits.Count;

        if (hitCount < CountedPhraseHitTarget)
        {
            return false;
        }

        int targetCount = GetCountPhraseSelectionCount(hitCount);

        foreach (StrokeCountPhrase phrase in StrokeCountPhrases)
        {
            if (phrase != null && phrase.StrokeCount == targetCount && CountPhraseHasExistingState(phrase))
            {
                return true;
            }
        }

        return false;
    }

    private int GetCountPhraseSelectionCount(int hitCount)
    {
        if (HasCountPhraseForCount(hitCount))
        {
            return hitCount;
        }

        if (HasCountPhraseForCount(CountedPhraseHitTarget))
        {
            return CountedPhraseHitTarget;
        }

        return hitCount;
    }

    private bool HasCountPhraseForCount(int hitCount)
    {
        if (StrokeCountPhrases == null)
        {
            return false;
        }

        foreach (StrokeCountPhrase phrase in StrokeCountPhrases)
        {
            if (phrase != null && phrase.StrokeCount == hitCount && CountPhraseHasExistingState(phrase))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasUsableCountPhrases()
    {
        if (!UseStrokeCountPhrases || StrokeCountPhrases == null || StrokeCountPhrases.Count == 0)
        {
            return false;
        }

        foreach (StrokeCountPhrase phrase in StrokeCountPhrases)
        {
            if (phrase != null && phrase.States != null && phrase.States.Count > 0 && CountPhraseHasExistingState(phrase))
            {
                return true;
            }
        }

        return false;
    }

    private string GetDominantStroke()
    {
        if (InferTrilloFromFastHits && IsTrilloLikePattern())
        {
            return "Trillo";
        }

        StrokeComposition composition = GetStrokeComposition();

        if (composition.Tek >= composition.Doum && composition.Tek >= composition.Ka && composition.Tek >= composition.Trillo)
        {
            return "Tek";
        }

        if (composition.Ka >= composition.Doum && composition.Ka >= composition.Tek && composition.Ka >= composition.Trillo)
        {
            return "Ka";
        }

        if (composition.Trillo >= composition.Doum && composition.Trillo >= composition.Tek && composition.Trillo >= composition.Ka)
        {
            return "Trillo";
        }

        return "Doum";
    }

    private StrokeComposition GetStrokeComposition()
    {
        StrokeComposition composition = new StrokeComposition();

        foreach (StrokeHit hit in phraseHits)
        {
            switch (hit.Type)
            {
                case "Doum":
                    composition.Doum++;
                    break;

                case "Tek":
                    composition.Tek++;
                    break;

                case "Ka":
                    composition.Ka++;
                    break;

                case "Trillo":
                    composition.Trillo++;
                    break;
            }
        }

        return composition;
    }

    private string GetShowcaseCue(StrokeComposition composition)
    {
        if (InferTrilloFromFastHits && IsTrilloLikePattern())
        {
            return "Trillo";
        }

        if (composition.Trillo > 0 && composition.Trillo >= composition.Doum && composition.Trillo >= composition.Tek)
        {
            return "Trillo";
        }

        if (composition.DoumTekTotal >= 4)
        {
            float doumShare = composition.Doum / (float)composition.DoumTekTotal;
            float tekShare = composition.Tek / (float)composition.DoumTekTotal;

            if (doumShare >= 0.65f)
            {
                return "Doum";
            }

            if (tekShare >= 0.65f)
            {
                return "Tek";
            }

            return "DoumTek";
        }

        return GetDominantStroke();
    }

    private bool IsDoumPairShowcase(StrokeComposition composition)
    {
        return DoumPairMoveEvery > 0 &&
               composition.Doum >= DoumPairMoveEvery &&
               composition.Doum % DoumPairMoveEvery == 0 &&
               composition.Tek == 0 &&
               composition.Ka == 0 &&
               composition.Trillo == 0 &&
               GetFastGapCount() == 0 &&
               HasAnimatorState(DoumPairState);
    }

    private bool IsFastConsecutiveCandidate(StrokeComposition composition)
    {
        if (composition.Total < FastConsecutiveMinimumHits || FastConsecutiveStates == null || FastConsecutiveStates.Count < 2)
        {
            return false;
        }

        return IsTrilloLikePattern() || GetFastGapCount() >= Mathf.Max(1, FastConsecutiveMinimumHits - 2);
    }

    private List<string> PickFastConsecutiveSequence(StrokeComposition composition)
    {
        List<string> sequence = new List<string>();

        if (!IsFastConsecutiveCandidate(composition))
        {
            return sequence;
        }

        List<string> existingStates = new List<string>();

        foreach (string stateName in FastConsecutiveStates)
        {
            if (HasAnimatorState(stateName))
            {
                existingStates.Add(stateName);
            }
        }

        int sequenceLength = composition.Total >= FastConsecutiveLongMinimumHits ? 3 : 2;

        if (existingStates.Count < sequenceLength)
        {
            return sequence;
        }

        int maxStart = existingStates.Count - sequenceLength;
        int startIndex = UnityEngine.Random.Range(0, maxStart + 1);

        for (int i = 0; i < sequenceLength; i++)
        {
            sequence.Add(existingStates[startIndex + i]);
        }

        return sequence;
    }

    private int GetFastGapCount()
    {
        int fastGaps = 0;

        for (int i = 1; i < phraseHits.Count; i++)
        {
            float gap = phraseHits[i].Time - phraseHits[i - 1].Time;

            if (gap <= FastGapSeconds)
            {
                fastGaps++;
            }
        }

        return fastGaps;
    }

    private void PlayShowcaseSequence(List<string> sequence, int rawHitCount, string phraseCue, string dominantStroke, StrokeComposition composition)
    {
        if (sequence == null || sequence.Count == 0)
        {
            return;
        }

        Debug.Log($"Phrase [fast-run: {rawHitCount} hits, cue {phraseCue}, dominant {dominantStroke}, doum {composition.Doum}, tek {composition.Tek}] -> {string.Join(" -> ", sequence)}");

        if (!CrossFadeState(sequence[0], FastConsecutiveCrossfade))
        {
            phraseHits.Clear();
            return;
        }

        lastPhraseState = sequence[0];
        lastMoveTime = Time.time;
        returnToIdleTime = Time.time + FastConsecutivePhraseSeconds * sequence.Count;
        nextPhraseAllowedTime = returnToIdleTime;
        isDancing = true;

        queuedShowcaseStates.Clear();

        for (int i = 1; i < sequence.Count; i++)
        {
            queuedShowcaseStates.Add(sequence[i]);
        }

        queuedShowcaseIndex = 0;
        nextQueuedShowcaseTime = queuedShowcaseStates.Count > 0 ? Time.time + FastConsecutivePhraseSeconds : -1f;

        phraseHits.Clear();
        phraseStartTime = -1f;
        lastHitTime = -1f;
    }

    private void UpdateQueuedShowcaseStates()
    {
        if (queuedShowcaseIndex >= queuedShowcaseStates.Count || nextQueuedShowcaseTime < 0f || Time.time < nextQueuedShowcaseTime)
        {
            return;
        }

        string nextState = queuedShowcaseStates[queuedShowcaseIndex];

        if (CrossFadeState(nextState, FastConsecutiveCrossfade))
        {
            lastPhraseState = nextState;
            lastMoveTime = Time.time;
        }

        queuedShowcaseIndex++;
        nextQueuedShowcaseTime = queuedShowcaseIndex < queuedShowcaseStates.Count ? Time.time + FastConsecutivePhraseSeconds : -1f;
    }

    private void ClearQueuedShowcaseStates()
    {
        queuedShowcaseStates.Clear();
        queuedShowcaseIndex = 0;
        nextQueuedShowcaseTime = -1f;
    }

    private bool IsTrilloLikePattern()
    {
        if (phraseHits.Count < RollMinimumHits)
        {
            return false;
        }

        float firstTime = phraseHits[0].Time;
        float lastTime = phraseHits[phraseHits.Count - 1].Time;
        float duration = Mathf.Max(0.01f, lastTime - firstTime);
        float averageGap = duration / Mathf.Max(1, phraseHits.Count - 1);
        int fastGaps = 0;

        for (int i = 1; i < phraseHits.Count; i++)
        {
            float gap = phraseHits[i].Time - phraseHits[i - 1].Time;

            if (gap <= FastGapSeconds)
            {
                fastGaps++;
            }
        }

        return averageGap <= InferredTrilloAverageGapSeconds || fastGaps >= InferredTrilloMinimumFastGaps;
    }

    private string GetRhythmPatternCue()
    {
        if (phraseHits.Count <= 1)
        {
            return "Sparse";
        }

        float firstTime = phraseHits[0].Time;
        float lastTime = phraseHits[phraseHits.Count - 1].Time;
        float duration = Mathf.Max(0.01f, lastTime - firstTime);
        float averageGap = duration / Mathf.Max(1, phraseHits.Count - 1);
        int fastGaps = 0;

        for (int i = 1; i < phraseHits.Count; i++)
        {
            float gap = phraseHits[i].Time - phraseHits[i - 1].Time;

            if (gap <= FastGapSeconds)
            {
                fastGaps++;
            }
        }

        if (phraseHits.Count >= RollMinimumHits && averageGap <= RollAverageGapSeconds)
        {
            return "Trillo";
        }

        if (fastGaps >= RollMinimumHits - 1)
        {
            return "Trillo";
        }

        if (phraseHits.Count >= BurstMinimumHits && duration <= 0.9f)
        {
            return "Burst";
        }

        if (phraseHits.Count <= 2 && duration > 0.65f)
        {
            return "Sparse";
        }

        return "Groove";
    }

    private string PickPhraseState(string dominantStroke)
    {
        List<DancePhrase> exactCandidates = new List<DancePhrase>();
        List<DancePhrase> fallbackCandidates = new List<DancePhrase>();

        foreach (DancePhrase phrase in PhrasePools)
        {
            if (phrase == null || phrase.States == null || phrase.States.Count == 0 || !PhraseHasExistingState(phrase))
            {
                continue;
            }

            if (string.Equals(phrase.PreferredStroke, dominantStroke, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(dominantStroke, "Trillo", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(phrase.PreferredStroke, "Roll", StringComparison.OrdinalIgnoreCase)))
            {
                exactCandidates.Add(phrase);
            }
            else if (string.Equals(phrase.PreferredStroke, "Any", StringComparison.OrdinalIgnoreCase))
            {
                fallbackCandidates.Add(phrase);
            }
        }

        if (exactCandidates.Count > 0)
        {
            DancePhrase phrase = PickWeightedPhrase(exactCandidates);
            return PickExistingStateFromList(phrase.States, lastPhraseState);
        }

        if (fallbackCandidates.Count > 0)
        {
            DancePhrase phrase = PickWeightedPhrase(fallbackCandidates);
            return PickExistingStateFromList(phrase.States, lastPhraseState);
        }

        List<string> fallbackPool = GetPoolForStroke(dominantStroke);
        return PickExistingStateFromList(fallbackPool, lastPhraseState);
    }

    private string PickCountPhraseState(int hitCount, string phraseCue, string dominantStroke)
    {
        if (!UseStrokeCountPhrases || StrokeCountPhrases == null || StrokeCountPhrases.Count == 0)
        {
            return null;
        }

        List<StrokeCountPhrase> cueCandidates = new List<StrokeCountPhrase>();
        List<StrokeCountPhrase> dominantCandidates = new List<StrokeCountPhrase>();
        List<StrokeCountPhrase> fallbackCandidates = new List<StrokeCountPhrase>();

        foreach (StrokeCountPhrase phrase in StrokeCountPhrases)
        {
            if (phrase == null || phrase.States == null || phrase.States.Count == 0 || phrase.StrokeCount != hitCount || !CountPhraseHasExistingState(phrase))
            {
                continue;
            }

            if (string.Equals(phrase.PreferredStroke, phraseCue, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(phraseCue, "Trillo", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(phrase.PreferredStroke, "Roll", StringComparison.OrdinalIgnoreCase)))
            {
                cueCandidates.Add(phrase);
            }
            else if (string.Equals(phrase.PreferredStroke, dominantStroke, StringComparison.OrdinalIgnoreCase))
            {
                dominantCandidates.Add(phrase);
            }
            else if (string.Equals(phrase.PreferredStroke, "Any", StringComparison.OrdinalIgnoreCase))
            {
                fallbackCandidates.Add(phrase);
            }
        }

        List<StrokeCountPhrase> candidates = cueCandidates.Count > 0 ? cueCandidates : dominantCandidates;

        if (candidates.Count == 0)
        {
            candidates = fallbackCandidates;
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        StrokeCountPhrase chosenPhrase = PickWeightedCountPhrase(candidates);
        string chosenState = PickExistingStateFromList(chosenPhrase.States, lastCountPhraseState);
        lastCountPhraseState = chosenState;
        return chosenState;
    }

    private StrokeCountPhrase PickWeightedCountPhrase(List<StrokeCountPhrase> candidates)
    {
        int totalWeight = 0;

        foreach (StrokeCountPhrase phrase in candidates)
        {
            totalWeight += Mathf.Max(1, phrase.Weight);
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);

        foreach (StrokeCountPhrase phrase in candidates)
        {
            roll -= Mathf.Max(1, phrase.Weight);

            if (roll < 0)
            {
                return phrase;
            }
        }

        return candidates[candidates.Count - 1];
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

        float resolvedCrossfadeSeconds = GetResolvedCrossfadeSeconds(stateName, crossfadeSeconds);
        LolaAnimator.CrossFadeInFixedTime(stateName, resolvedCrossfadeSeconds, 0, 0f);
        Debug.Log($"Animator crossfade started: {stateName} ({resolvedCrossfadeSeconds:0.00}s)");
        return true;
    }

    private float GetResolvedCrossfadeSeconds(string stateName, float requestedCrossfadeSeconds)
    {
        float requested = Mathf.Max(0f, requestedCrossfadeSeconds);

        if (string.Equals(stateName, IdleStateName, StringComparison.Ordinal))
        {
            return Mathf.Max(requested, MinimumIdleCrossfade);
        }

        return Mathf.Max(requested, MinimumMoveCrossfade);
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

    private bool CountPhraseHasExistingState(StrokeCountPhrase phrase)
    {
        if (phrase == null || phrase.States == null)
        {
            return false;
        }

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
                targetAnimatorSpeed = Mathf.Max(targetAnimatorSpeed, BaseAnimatorSpeed * DoumSpeedPulse);
                break;

            case "Tek":
                targetAnimatorSpeed = Mathf.Max(targetAnimatorSpeed, BaseAnimatorSpeed * TekSpeedPulse);
                break;

            case "Ka":
                targetAnimatorSpeed = Mathf.Max(targetAnimatorSpeed, BaseAnimatorSpeed * KaSpeedPulse);
                break;

            case "Trillo":
                targetAnimatorSpeed = Mathf.Max(targetAnimatorSpeed, BaseAnimatorSpeed * TrilloSpeedPulse);
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
            LolaAnimator.speed = BaseAnimatorSpeed;
            targetAnimatorSpeed = BaseAnimatorSpeed;
            return;
        }

        LolaAnimator.speed = targetAnimatorSpeed;
        targetAnimatorSpeed = Mathf.Lerp(targetAnimatorSpeed, BaseAnimatorSpeed, Time.deltaTime * BeatPulseReturnSpeed);
    }

    private void OnDoumReceived(OscMessageValues values)
    {
        EnqueueLiveHit("Doum");
    }

    private void OnTekReceived(OscMessageValues values)
    {
        EnqueueLiveHit("Tek");
    }

    private void OnKaReceived(OscMessageValues values)
    {
        EnqueueLiveHit("Ka");
    }

    private void OnTrilloReceived(OscMessageValues values)
    {
        EnqueueLiveHit("Trillo");
    }

    private void EnqueueLiveHit(string strokeType)
    {
        if (!TryAcceptLiveHit(strokeType))
        {
            return;
        }

        LogOscHit(strokeType);
        triggerQueue.Enqueue(strokeType);
    }

    private bool TryAcceptLiveHit(string strokeType)
    {
        if (!FilterLikelyNoiseHits)
        {
            return true;
        }

        double now = liveHitClock.Elapsed.TotalSeconds;
        float minimumAnyGap = Mathf.Max(0f, MinimumSecondsBetweenLiveHits);
        float minimumSameGap = Mathf.Max(0f, MinimumSecondsBetweenSameLiveHit);

        lock (liveHitFilterLock)
        {
            if (now - lastAcceptedLiveHitTime < minimumAnyGap)
            {
                LogRejectedLiveHit(strokeType, "too soon after previous hit");
                return false;
            }

            if (lastAcceptedLiveHitByType.TryGetValue(strokeType, out double lastSameHitTime) && now - lastSameHitTime < minimumSameGap)
            {
                LogRejectedLiveHit(strokeType, "duplicate hit");
                return false;
            }

            lastAcceptedLiveHitTime = now;
            lastAcceptedLiveHitByType[strokeType] = now;
            return true;
        }
    }

    private void LogRejectedLiveHit(string strokeType, string reason)
    {
        rejectedLiveHitCount++;

        if (!LogIncomingOsc)
        {
            return;
        }

        if (rejectedLiveHitCount <= 8)
        {
            Debug.Log($"OSC hit ignored by noise filter: {strokeType} ({reason})");
        }
        else if (rejectedLiveHitCount == 9)
        {
            Debug.Log("Further OSC noise-filter logs suppressed.");
        }
    }

    private void LogOscHit(string strokeType)
    {
        if (LogIncomingOsc)
        {
            Debug.Log("OSC hit received: " + strokeType);
        }
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
