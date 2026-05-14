using UnityEngine;
using OscCore;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class DrumDanceController : MonoBehaviour
{
    [Header("References")]
    public Animator LolaAnimator;

    [Tooltip("Must exactly match the idle state name in the Animator.")]
    public string IdleStateName = "Idle";

    [Header("OSC")]
    public int Port = 7000;

    [Header("Dance Clip Pools")]
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

    private float returnToIdleTime = -1f;
    private float lastMoveTime = -999f;
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
    }

    void Update()
    {
        while (triggerQueue.TryDequeue(out string strokeType))
        {
            TryPlayRandomDance(strokeType);
        }

        if (isDancing && returnToIdleTime > 0f && Time.time >= returnToIdleTime)
        {
            ReturnToIdle();
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
        LolaAnimator.CrossFadeInFixedTime(chosenState, DanceCrossfade, 0, 0f);

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

        LolaAnimator.CrossFadeInFixedTime(IdleStateName, ReturnCrossfade, 0, 0f);

        isDancing = false;
        returnToIdleTime = -1f;
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

        string chosen = pool[Random.Range(0, pool.Count)];

        if (AvoidImmediateRepeats)
        {
            int attempts = 0;

            while (chosen == lastClip && attempts < 10)
            {
                chosen = pool[Random.Range(0, pool.Count)];
                attempts++;
            }
        }

        return chosen;
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