using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BailandoSkeletonPreview : MonoBehaviour
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

    private static readonly int[] Parents =
    {
        -1, 0, 0, 0, 1, 2, 3, 4, 5,
        6, 7, 8, 9, 9, 9, 12, 13, 14,
        16, 17, 18, 19, 20, 21
    };

    public string ExternalPoseJsonPath = @"C:\Users\23028727\Downloads\Unityraqs-main (1)\Unityraqs\Assets\StreamingAssets\Bailando\latest_pose.json";
    public Animator TargetAnimator;
    public bool AutoFitToTarget = false;
    public float PoseScale = 1.15f;
    public Vector3 Offset = new Vector3(0f, 0.75f, -0.65f);
    public Vector3 TargetLocalOffset = new Vector3(0f, 0f, -0.55f);
    public float AutoFitScaleMultiplier = 0.92f;
    public bool InvertY = false;
    public bool SwapYZ = false;
    public Color BoneColor = new Color(0.2f, 0.95f, 1f);
    public Color JointColor = new Color(1f, 0.85f, 0.25f);
    public float BoneWidth = 0.018f;
    public KeyCode ReloadKey = KeyCode.B;

    private PoseSequence sequence;
    private readonly List<LineRenderer> boneLines = new List<LineRenderer>();
    private readonly List<Transform> jointDots = new List<Transform>();
    private float playbackTime;
    private float fittedPoseScale = 1f;

    private void Start()
    {
        if (TargetAnimator == null)
        {
            TargetAnimator = GetComponentInParent<Animator>();
        }

        Load();
    }

    private void Update()
    {
        if (Input.GetKeyDown(ReloadKey))
        {
            Load();
            playbackTime = 0f;
        }

        if (sequence == null || sequence.frames == null || sequence.frames.Length == 0)
        {
            return;
        }

        playbackTime += Time.deltaTime;
        int frameIndex = Mathf.FloorToInt(playbackTime * Mathf.Max(1f, sequence.fps)) % sequence.frames.Length;
        DrawFrame(sequence.frames[frameIndex]);
    }

    [ContextMenu("Load Bailando Skeleton Preview")]
    public void Load()
    {
        if (!File.Exists(ExternalPoseJsonPath))
        {
            Debug.LogWarning($"Bailando skeleton preview JSON not found: {ExternalPoseJsonPath}");
            return;
        }

        sequence = JsonUtility.FromJson<PoseSequence>(File.ReadAllText(ExternalPoseJsonPath));

        if (sequence == null || sequence.frames == null || sequence.frames.Length == 0)
        {
            Debug.LogWarning("Bailando skeleton preview loaded no frames.");
            return;
        }

        EnsureRenderers();
        UpdateFittedScale();
        Debug.Log($"Loaded Bailando skeleton preview: {sequence.frames.Length} frames at {sequence.fps:0.##} fps.");
    }

    private void UpdateFittedScale()
    {
        fittedPoseScale = PoseScale;

        if (!AutoFitToTarget || TargetAnimator == null || sequence.frames == null || sequence.frames.Length == 0)
        {
            return;
        }

        float sourceHeight = EstimateSourceHeight(sequence.frames[0]);
        float targetHeight = EstimateTargetHeight();

        if (sourceHeight > 0.001f && targetHeight > 0.001f)
        {
            fittedPoseScale = targetHeight / sourceHeight * AutoFitScaleMultiplier;
        }
    }

    private float EstimateSourceHeight(PoseFrame frame)
    {
        if (frame == null || frame.joints == null || frame.joints.Length < 24)
        {
            return 0f;
        }

        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        foreach (PoseVector joint in frame.joints)
        {
            float y = joint.y;
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        return maxY - minY;
    }

    private float EstimateTargetHeight()
    {
        Transform head = TargetAnimator.GetBoneTransform(HumanBodyBones.Head);
        Transform leftFoot = TargetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFoot = TargetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);

        if (head == null || leftFoot == null || rightFoot == null)
        {
            return Mathf.Max(0.1f, TargetAnimator.transform.lossyScale.y);
        }

        float footY = Mathf.Min(leftFoot.position.y, rightFoot.position.y);
        return Mathf.Abs(head.position.y - footY);
    }

    private void EnsureRenderers()
    {
        while (boneLines.Count < 23)
        {
            GameObject lineObject = new GameObject($"Bailando bone {boneLines.Count:00}");
            lineObject.transform.SetParent(transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = BoneWidth;
            line.endWidth = BoneWidth;
            line.material = MakeMaterial(BoneColor);
            boneLines.Add(line);
        }

        while (jointDots.Count < 24)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = $"Bailando joint {jointDots.Count:00}";
            dot.transform.SetParent(transform, false);
            dot.transform.localScale = Vector3.one * 0.045f;
            dot.GetComponent<Renderer>().sharedMaterial = MakeMaterial(JointColor);
            jointDots.Add(dot.transform);
        }
    }

    private Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private void DrawFrame(PoseFrame frame)
    {
        if (frame == null || frame.joints == null || frame.joints.Length < 24)
        {
            return;
        }

        if (boneLines.Count < 23 || jointDots.Count < 24)
        {
            EnsureRenderers();
        }

        Vector3[] points = new Vector3[24];
        Vector3 root = GetPreviewRoot();
        Quaternion rotation = GetPreviewRotation();

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = root + rotation * ConvertPoint(frame.joints[i].ToVector3());
            jointDots[i].position = points[i];
        }

        int lineIndex = 0;

        for (int child = 0; child < Parents.Length; child++)
        {
            int parent = Parents[child];

            if (parent < 0)
            {
                continue;
            }

            if (lineIndex >= boneLines.Count)
            {
                return;
            }

            boneLines[lineIndex].SetPosition(0, points[parent]);
            boneLines[lineIndex].SetPosition(1, points[child]);
            lineIndex++;
        }
    }

    private Vector3 GetPreviewRoot()
    {
        if (!AutoFitToTarget || TargetAnimator == null)
        {
            return transform.position;
        }

        Transform hips = TargetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        Vector3 basePosition = hips != null ? hips.position : TargetAnimator.transform.position;
        return basePosition + TargetAnimator.transform.TransformVector(TargetLocalOffset);
    }

    private Quaternion GetPreviewRotation()
    {
        if (!AutoFitToTarget || TargetAnimator == null)
        {
            return transform.rotation;
        }

        return TargetAnimator.transform.rotation;
    }

    private Vector3 ConvertPoint(Vector3 point)
    {
        if (SwapYZ)
        {
            point = new Vector3(point.x, point.z, point.y);
        }

        if (InvertY)
        {
            point.y *= -1f;
        }

        return Offset + point * fittedPoseScale;
    }
}
