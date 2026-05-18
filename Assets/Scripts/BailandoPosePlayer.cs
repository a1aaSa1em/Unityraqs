using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BailandoPosePlayer : MonoBehaviour
{
    [Serializable]
    public class PoseVector
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class PoseFrame
    {
        public PoseVector[] joints;
    }

    [Serializable]
    public class PoseSequence
    {
        public float fps = 30f;
        public PoseFrame[] frames;
    }

    private struct BoneBinding
    {
        public HumanBodyBones Bone;
        public int ParentJoint;
        public int ChildJoint;
        public Transform Transform;
        public Quaternion BindRotation;
        public Vector3 BindDirection;
        public Vector3 SourceBindDirection;
    }

    [Header("Source")]
    public TextAsset PoseJson;

    [Tooltip("If set, this file is used before PoseJson. Use this for freshly generated Bailando output.")]
    public string ExternalPoseJsonPath = @"C:\Users\23028727\Downloads\Unityraqs-main (1)\Unityraqs\Assets\StreamingAssets\Bailando\latest_pose.json";

    public KeyCode ReloadKey = KeyCode.B;

    [Header("Target")]
    public Animator TargetAnimator;

    [Tooltip("Leave this off while checking the raw Bailando skeleton. Turn it on only after the joint axes and scale look correct.")]
    public bool EnableRetargeting = false;

    public bool DisableAnimatorWhilePlaying = true;
    public bool PlayOnStart = true;
    public bool Loop = true;

    [Header("Rhythm Reactivity")]
    public DrumDanceController RhythmSource;
    public bool DriveFromRhythmInput = true;
    public bool PauseWhenNoRecentInput = true;
    public bool JumpToPhraseOnHit = true;
    public float RecentInputSeconds = 1.1f;
    public float IdlePlaybackSpeed = 0f;
    public float ActivePlaybackSpeed = 1.15f;
    public float HitSpeedPulse = 1.6f;
    public float HitPulseReturnSpeed = 6f;

    [Header("Retarget")]
    [Tooltip("Scales Bailando joint positions before applying hips motion.")]
    public float PoseScale = 1f;

    [Tooltip("Use a small value like 0.04 if the generated root motion slides Elmo too much.")]
    public float RootMotionScale = 0.15f;

    public bool ApplyRootMotion = true;
    public bool FlipZ = false;
    public bool SwapYZ = false;
    public bool InvertY = false;
    public float RotationSmoothing = 18f;
    [Range(0f, 1f)]
    public float RetargetWeight = 0.65f;
    public Vector3 RootOffset = Vector3.zero;

    private PoseSequence sequence;
    private readonly List<BoneBinding> bindings = new List<BoneBinding>();
    private Vector3 initialRootPosition;
    private Quaternion initialRootRotation;
    private float playbackTime;
    private float playbackSpeedPulse = 1f;
    private int lastSeenStrokeCount = 0;
    private bool isLoaded;

    private void Reset()
    {
        TargetAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (TargetAnimator == null)
        {
            TargetAnimator = GetComponentInChildren<Animator>();
        }

        if (RhythmSource == null)
        {
            RhythmSource = FindAnyObjectByType<DrumDanceController>();
        }

        initialRootPosition = transform.position;
        initialRootRotation = transform.rotation;

        LoadPoseSequence();

        if (!EnableRetargeting)
        {
            Debug.Log("BailandoPosePlayer loaded, but Elmo retargeting is disabled. Use the skeleton preview to verify the raw motion first.");
            return;
        }

        BindHumanoidBones();

        if (DisableAnimatorWhilePlaying && TargetAnimator != null)
        {
            TargetAnimator.enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(ReloadKey))
        {
            LoadPoseSequence();
            playbackTime = 0f;
        }

        if (!EnableRetargeting || !PlayOnStart || !isLoaded || sequence.frames == null || sequence.frames.Length == 0)
        {
            return;
        }

        UpdateRhythmResponse();

        float playbackSpeed = ActivePlaybackSpeed;

        if (DriveFromRhythmInput && PauseWhenNoRecentInput && RhythmSource != null && !RhythmSource.HasRecentInput(RecentInputSeconds))
        {
            playbackSpeed = IdlePlaybackSpeed;
        }

        playbackSpeedPulse = Mathf.Lerp(playbackSpeedPulse, 1f, 1f - Mathf.Exp(-HitPulseReturnSpeed * Time.deltaTime));
        playbackTime += Time.deltaTime * playbackSpeed * playbackSpeedPulse;
        ApplyPlaybackFrame();
    }

    [ContextMenu("Load Pose Sequence")]
    public void LoadPoseSequence()
    {
        string json = null;

        if (!string.IsNullOrWhiteSpace(ExternalPoseJsonPath) && File.Exists(ExternalPoseJsonPath))
        {
            json = File.ReadAllText(ExternalPoseJsonPath);
        }
        else if (PoseJson != null)
        {
            json = PoseJson.text;
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            isLoaded = false;
            Debug.LogWarning("No Bailando pose JSON found. Generate one with live_perf.py first, or assign a PoseJson TextAsset.");
            return;
        }

        sequence = JsonUtility.FromJson<PoseSequence>(json);
        isLoaded = sequence != null && sequence.frames != null && sequence.frames.Length > 0;

        if (!isLoaded)
        {
            Debug.LogWarning("Bailando pose JSON loaded, but no frames were found.");
            return;
        }

        if (sequence.fps <= 0f)
        {
            sequence.fps = 30f;
        }

        Debug.Log($"Loaded Bailando pose sequence: {sequence.frames.Length} frames at {sequence.fps:0.##} fps.");
    }

    private void BindHumanoidBones()
    {
        bindings.Clear();

        if (TargetAnimator == null || !TargetAnimator.isHuman)
        {
            Debug.LogWarning("BailandoPosePlayer needs a Humanoid Animator on the Elmo target.");
            return;
        }

        AddBinding(HumanBodyBones.Hips, 0, 3);
        AddBinding(HumanBodyBones.Spine, 3, 6);
        AddBinding(HumanBodyBones.Chest, 6, 9);
        AddBinding(HumanBodyBones.UpperChest, 9, 12);
        AddBinding(HumanBodyBones.Neck, 12, 15);

        AddBinding(HumanBodyBones.LeftUpperLeg, 1, 4);
        AddBinding(HumanBodyBones.LeftLowerLeg, 4, 7);
        AddBinding(HumanBodyBones.LeftFoot, 7, 10);

        AddBinding(HumanBodyBones.RightUpperLeg, 2, 5);
        AddBinding(HumanBodyBones.RightLowerLeg, 5, 8);
        AddBinding(HumanBodyBones.RightFoot, 8, 11);

        AddBinding(HumanBodyBones.LeftShoulder, 13, 16);
        AddBinding(HumanBodyBones.LeftUpperArm, 16, 18);
        AddBinding(HumanBodyBones.LeftLowerArm, 18, 20);
        AddBinding(HumanBodyBones.LeftHand, 20, 22);

        AddBinding(HumanBodyBones.RightShoulder, 14, 17);
        AddBinding(HumanBodyBones.RightUpperArm, 17, 19);
        AddBinding(HumanBodyBones.RightLowerArm, 19, 21);
        AddBinding(HumanBodyBones.RightHand, 21, 23);
    }

    private void AddBinding(HumanBodyBones bone, int parentJoint, int childJoint)
    {
        Transform boneTransform = TargetAnimator.GetBoneTransform(bone);

        if (boneTransform == null)
        {
            return;
        }

        Vector3 bindDirection = GetExistingChildDirection(boneTransform);

        bindings.Add(new BoneBinding
        {
            Bone = bone,
            ParentJoint = parentJoint,
            ChildJoint = childJoint,
            Transform = boneTransform,
            BindRotation = boneTransform.rotation,
            BindDirection = bindDirection.normalized,
            SourceBindDirection = GetPoseDirection(sequence != null && sequence.frames != null && sequence.frames.Length > 0 ? sequence.frames[0] : null, parentJoint, childJoint)
        });
    }

    private Vector3 GetExistingChildDirection(Transform boneTransform)
    {
        if (boneTransform.childCount > 0)
        {
            Vector3 childDirection = boneTransform.GetChild(0).position - boneTransform.position;

            if (childDirection.sqrMagnitude > 0.0001f)
            {
                return childDirection;
            }
        }

        return boneTransform.rotation * Vector3.up;
    }

    private void ApplyPlaybackFrame()
    {
        int frameCount = sequence.frames.Length;
        float fps = Mathf.Max(1f, sequence.fps);
        int frameIndex = Mathf.FloorToInt(playbackTime * fps);

        if (Loop)
        {
            frameIndex %= frameCount;
        }
        else
        {
            frameIndex = Mathf.Min(frameIndex, frameCount - 1);
        }

        ApplyFrame(sequence.frames[frameIndex]);
    }

    private void UpdateRhythmResponse()
    {
        if (!DriveFromRhythmInput || RhythmSource == null || RhythmSource.TotalProcessedStrokes == lastSeenStrokeCount)
        {
            return;
        }

        lastSeenStrokeCount = RhythmSource.TotalProcessedStrokes;
        playbackSpeedPulse = HitSpeedPulse;

        if (JumpToPhraseOnHit)
        {
            playbackTime = GetStrokeStartTime(RhythmSource.LastStrokeType);
        }
    }

    private float GetStrokeStartTime(string strokeType)
    {
        if (sequence == null || sequence.frames == null || sequence.frames.Length == 0)
        {
            return 0f;
        }

        float fps = Mathf.Max(1f, sequence.fps);
        float duration = sequence.frames.Length / fps;
        float normalizedStart = 0f;

        switch ((strokeType ?? "").ToLowerInvariant())
        {
            case "doum":
                normalizedStart = 0.05f;
                break;
            case "tek":
                normalizedStart = 0.28f;
                break;
            case "ka":
                normalizedStart = 0.52f;
                break;
            case "trillo":
                normalizedStart = 0.74f;
                break;
        }

        float jitter = (lastSeenStrokeCount % 3) * 0.06f;
        return Mathf.Repeat((normalizedStart + jitter) * duration, duration);
    }

    private void ApplyFrame(PoseFrame frame)
    {
        if (frame == null || frame.joints == null || frame.joints.Length < 24)
        {
            return;
        }

        if (ApplyRootMotion)
        {
            Vector3 root = ConvertPosePoint(frame.joints[0].ToVector3());
            transform.position = initialRootPosition + RootOffset + initialRootRotation * (root * RootMotionScale);
        }

        float smoothing = RotationSmoothing <= 0f ? 1f : 1f - Mathf.Exp(-RotationSmoothing * Time.deltaTime);
        float weight = Mathf.Clamp01(RetargetWeight);

        foreach (BoneBinding binding in bindings)
        {
            Vector3 poseDirection = GetPoseDirection(frame, binding.ParentJoint, binding.ChildJoint);

            if (poseDirection.sqrMagnitude < 0.0001f || binding.SourceBindDirection.sqrMagnitude < 0.0001f)
            {
                continue;
            }

            Quaternion sourceDelta = Quaternion.FromToRotation(binding.SourceBindDirection, poseDirection.normalized);
            Quaternion worldDelta = initialRootRotation * sourceDelta * Quaternion.Inverse(initialRootRotation);
            Quaternion targetRotation = worldDelta * binding.BindRotation;
            targetRotation = Quaternion.Slerp(binding.BindRotation, targetRotation, weight);
            binding.Transform.rotation = Quaternion.Slerp(binding.Transform.rotation, targetRotation, smoothing);
        }
    }

    private Vector3 GetPoseDirection(PoseFrame frame, int parentJoint, int childJoint)
    {
        if (frame == null || frame.joints == null || frame.joints.Length <= Mathf.Max(parentJoint, childJoint))
        {
            return Vector3.zero;
        }

        Vector3 parent = ConvertPosePoint(frame.joints[parentJoint].ToVector3());
        Vector3 child = ConvertPosePoint(frame.joints[childJoint].ToVector3());
        return (child - parent).normalized;
    }

    private Vector3 ConvertPosePoint(Vector3 point)
    {
        if (SwapYZ)
        {
            point = new Vector3(point.x, point.z, point.y);
        }

        if (InvertY)
        {
            point.y *= -1f;
        }

        point *= PoseScale;

        if (FlipZ)
        {
            point.z *= -1f;
        }

        return point;
    }
}
