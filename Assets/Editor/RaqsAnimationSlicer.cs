using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class RaqsAnimationSlicer : EditorWindow
{
    private const string DefaultSourceFolder = "Assets/Animations/Raqs Sliced Source";

    private struct SourceTake
    {
        public string TakeName;
        public float FirstFrame;
        public float LastFrame;
        public float FrameRate;

        public float LengthSeconds
        {
            get { return (LastFrame - FirstFrame) / Mathf.Max(1f, FrameRate); }
        }
    }

    private DefaultAsset targetFolder;
    private AnimatorController targetController;
    private float sliceSeconds = 0.75f;
    private float stepSeconds = 0f;
    private float minimumSliceSeconds = 0.3f;
    private float startSeconds = 0f;
    private float endSeconds = 0f;
    private string sliceLabel = "slice";
    private bool addStatesToController = true;
    private bool loopSlices = false;
    private bool copyDownloadsIntoProject = true;

    [MenuItem("Tools/Raqs/Animation Slicer")]
    public static void ShowWindow()
    {
        GetWindow<RaqsAnimationSlicer>("Raqs Animation Slicer");
    }

    [MenuItem("Tools/Raqs/Slice All Raqs Source FBX")]
    public static void SliceAllSourceFbxQuick()
    {
        SliceAllSourceFbxWithPreset("beat", 0.6f, 0.6f, 0.28f, false);
    }

    [MenuItem("Tools/Raqs/Slice All Raqs Source FBX As Phrases")]
    public static void SliceAllSourceFbxAsPhrases()
    {
        SliceAllSourceFbxWithPreset("phrase", 2.4f, 2.0f, 1.1f, true);
    }

    [MenuItem("Tools/Raqs/Report Raqs Source FBX Lengths")]
    public static void ReportSourceFbxLengths()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { DefaultSourceFolder });
        int count = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (importer == null || !TryGetSourceTake(assetPath, importer, out SourceTake sourceTake))
            {
                Debug.LogWarning($"No animation clip found in {assetPath}");
                continue;
            }

            string warning = sourceTake.LengthSeconds < 0.2f ? "  <-- very short / probably not useful" : "";
            Debug.Log($"{assetPath}: {sourceTake.LengthSeconds:0.00}s at {sourceTake.FrameRate:0.#} fps ({sourceTake.LastFrame - sourceTake.FirstFrame:0.#} frames){warning}");
            count++;
        }

        EditorUtility.DisplayDialog("Raqs FBX report", $"Logged lengths for {count} FBX file(s) in the Console.", "OK");
    }

    private static void SliceAllSourceFbxWithPreset(string label, float sliceLength, float stepLength, float minimumLength, bool loop)
    {
        RaqsAnimationSlicer slicer = CreateInstance<RaqsAnimationSlicer>();
        slicer.targetController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/LolaAnimator.controller");
        slicer.targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultSourceFolder);
        slicer.sliceLabel = label;
        slicer.sliceSeconds = sliceLength;
        slicer.stepSeconds = stepLength;
        slicer.minimumSliceSeconds = minimumLength;
        slicer.startSeconds = 0f;
        slicer.endSeconds = 0f;
        slicer.loopSlices = loop;
        slicer.addStatesToController = true;
        slicer.SliceAllInFolder();
        DestroyImmediate(slicer);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Slice selected FBX animations into beat-sized moves", EditorStyles.boldLabel);
        EditorGUILayout.Space(8f);

        targetController = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller",
            targetController,
            typeof(AnimatorController),
            false
        );

        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Import Folder",
            targetFolder,
            typeof(DefaultAsset),
            false
        );

        copyDownloadsIntoProject = EditorGUILayout.Toggle("Copy Downloads FBX", copyDownloadsIntoProject);
        addStatesToController = EditorGUILayout.Toggle("Add Animator States", addStatesToController);
        loopSlices = EditorGUILayout.Toggle("Loop Slices", loopSlices);
        sliceLabel = EditorGUILayout.TextField("Clip Label", sliceLabel);
        sliceSeconds = EditorGUILayout.FloatField("Slice Seconds", sliceSeconds);
        stepSeconds = EditorGUILayout.FloatField("Step Seconds (0 = slice)", stepSeconds);
        minimumSliceSeconds = EditorGUILayout.FloatField("Minimum Slice Seconds", minimumSliceSeconds);
        startSeconds = EditorGUILayout.FloatField("Start Seconds", startSeconds);
        endSeconds = EditorGUILayout.FloatField("End Seconds (0 = full)", endSeconds);

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox(
            "Use short beat slices for accents and longer phrase slices for moves that need several beats to read properly. Step Seconds can be shorter than Slice Seconds to create overlap for smoother choices.",
            MessageType.Info
        );

        if (GUILayout.Button("Slice Selected FBX"))
        {
            SliceSelection();
        }

        if (GUILayout.Button("Slice All FBX In Folder"))
        {
            SliceAllInFolder();
        }
    }

    private void OnEnable()
    {
        if (targetController == null)
        {
            targetController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/LolaAnimator.controller");
        }

        if (targetFolder == null)
        {
            EnsureFolder("Assets/Animations", "Raqs Sliced Source");
            targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultSourceFolder);
        }
    }

    private void SliceAllInFolder()
    {
        string folderPath = GetFolderPath();

        if (string.IsNullOrEmpty(folderPath))
        {
            EditorUtility.DisplayDialog("Missing folder", "Choose a folder under Assets for imported/sliced FBX files.", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folderPath });
        int totalSlices = 0;
        int filesProcessed = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int slices = SliceFbx(assetPath);

            if (slices > 0)
            {
                filesProcessed++;
                totalSlices += slices;
            }
        }

        EditorUtility.DisplayDialog(
            "Slicing complete",
            $"Processed {filesProcessed} FBX file(s).\nCreated {totalSlices} slice clip(s).",
            "OK"
        );
    }

    private void SliceSelection()
    {
        if (sliceSeconds <= 0.05f)
        {
            EditorUtility.DisplayDialog("Slice length too small", "Use a slice length above 0.05 seconds.", "OK");
            return;
        }

        if (GetStepSeconds() <= 0.05f)
        {
            EditorUtility.DisplayDialog("Step length too small", "Use a step length above 0.05 seconds, or set it to 0 to match Slice Seconds.", "OK");
            return;
        }

        string folderPath = GetFolderPath();

        if (string.IsNullOrEmpty(folderPath))
        {
            EditorUtility.DisplayDialog("Missing folder", "Choose a folder under Assets for imported/sliced FBX files.", "OK");
            return;
        }

        Object[] selectedObjects = Selection.objects;
        int totalSlices = 0;
        int filesProcessed = 0;

        foreach (Object selectedObject in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObject);

            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string projectPath = assetPath;

            if (copyDownloadsIntoProject && !assetPath.StartsWith("Assets/"))
            {
                projectPath = CopyFbxIntoProject(assetPath, folderPath);
            }

            int slices = SliceFbx(projectPath);

            if (slices > 0)
            {
                filesProcessed++;
                totalSlices += slices;
            }
        }

        EditorUtility.DisplayDialog(
            "Slicing complete",
            $"Processed {filesProcessed} FBX file(s).\nCreated {totalSlices} slice clip(s).",
            "OK"
        );
    }

    private string GetFolderPath()
    {
        string path = targetFolder != null ? AssetDatabase.GetAssetPath(targetFolder) : "Assets/Animations/Raqs Sliced Source";

        if (!AssetDatabase.IsValidFolder(path))
        {
            return null;
        }

        return path;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";

        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static string CopyFbxIntoProject(string sourcePath, string folderPath)
    {
        string destinationPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{Path.GetFileName(sourcePath)}");
        File.Copy(sourcePath, destinationPath);
        AssetDatabase.ImportAsset(destinationPath, ImportAssetOptions.ForceUpdate);
        return destinationPath;
    }

    private int SliceFbx(string assetPath)
    {
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

        if (importer == null)
        {
            Debug.LogWarning($"Selected asset is not an imported FBX model: {assetPath}");
            return 0;
        }

        ConfigureImporter(importer);

        if (!TryGetSourceTake(assetPath, importer, out SourceTake sourceTake))
        {
            Debug.LogWarning($"No animation clip found inside {assetPath}.");
            return 0;
        }

        float frameRate = Mathf.Max(1f, sourceTake.FrameRate);
        float clipLength = sourceTake.LengthSeconds;
        float start = Mathf.Clamp(startSeconds, 0f, clipLength);
        float end = endSeconds > 0f ? Mathf.Clamp(endSeconds, start, clipLength) : clipLength;

        if (end - start <= 0.05f)
        {
            Debug.LogWarning($"Slice range is too short for {assetPath}.");
            return 0;
        }

        string baseName = Path.GetFileNameWithoutExtension(assetPath).Replace(" ", "_").Replace("@", "_");
        List<ModelImporterClipAnimation> clips = new List<ModelImporterClipAnimation>();
        int sliceIndex = 1;
        float step = GetStepSeconds();
        float minimumLength = Mathf.Max(0.05f, minimumSliceSeconds);

        for (float current = start; current < end - 0.02f; current += step)
        {
            float sliceEnd = Mathf.Min(current + sliceSeconds, end);

            if (sliceEnd - current < minimumLength)
            {
                continue;
            }

            ModelImporterClipAnimation clip = new ModelImporterClipAnimation
            {
                name = $"{baseName}_{GetSafeLabel()}_{sliceIndex:00}",
                takeName = sourceTake.TakeName,
                firstFrame = sourceTake.FirstFrame + current * frameRate,
                lastFrame = sourceTake.FirstFrame + sliceEnd * frameRate,
                loopTime = loopSlices,
                loopPose = loopSlices,
                lockRootRotation = true,
                lockRootHeightY = true,
                lockRootPositionXZ = true,
                keepOriginalOrientation = true,
                keepOriginalPositionY = true,
                keepOriginalPositionXZ = true
            };

            clips.Add(clip);
            sliceIndex++;
        }

        importer.clipAnimations = clips.ToArray();
        importer.SaveAndReimport();

        if (addStatesToController && targetController != null)
        {
            AddSliceStates(assetPath, clips);
        }

        return clips.Count;
    }

    private float GetStepSeconds()
    {
        return stepSeconds > 0f ? stepSeconds : sliceSeconds;
    }

    private string GetSafeLabel()
    {
        if (string.IsNullOrWhiteSpace(sliceLabel))
        {
            return "slice";
        }

        return sliceLabel.Trim().Replace(" ", "_");
    }

    private static void ConfigureImporter(ModelImporter importer)
    {
        bool changed = false;

        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            changed = true;
        }

        if (!importer.importAnimation)
        {
            importer.importAnimation = true;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static AnimationClip FindSourceClip(string assetPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

        foreach (Object asset in assets)
        {
            AnimationClip clip = asset as AnimationClip;

            if (clip != null && !clip.name.StartsWith("__preview__", System.StringComparison.OrdinalIgnoreCase))
            {
                return clip;
            }
        }

        return null;
    }

    private static bool TryGetSourceTake(string assetPath, ModelImporter importer, out SourceTake sourceTake)
    {
        AnimationClip clip = FindSourceClip(assetPath);
        float frameRate = clip != null ? Mathf.Max(1f, clip.frameRate) : 60f;

        ModelImporterClipAnimation[] defaultClips = importer.defaultClipAnimations;

        if (defaultClips != null && defaultClips.Length > 0)
        {
            ModelImporterClipAnimation defaultClip = defaultClips[0];

            sourceTake = new SourceTake
            {
                TakeName = !string.IsNullOrEmpty(defaultClip.takeName) ? defaultClip.takeName : defaultClip.name,
                FirstFrame = defaultClip.firstFrame,
                LastFrame = defaultClip.lastFrame,
                FrameRate = frameRate
            };

            return sourceTake.LastFrame > sourceTake.FirstFrame;
        }

        if (clip != null)
        {
            sourceTake = new SourceTake
            {
                TakeName = clip.name,
                FirstFrame = 0f,
                LastFrame = clip.length * frameRate,
                FrameRate = frameRate
            };

            return sourceTake.LastFrame > sourceTake.FirstFrame;
        }

        sourceTake = default;
        return false;
    }

    private void AddSliceStates(string assetPath, List<ModelImporterClipAnimation> slices)
    {
        AnimationClip[] clips = LoadAnimationClips(assetPath);

        if (clips.Length == 0)
        {
            return;
        }

        AnimatorStateMachine stateMachine = targetController.layers[0].stateMachine;
        Undo.RegisterCompleteObjectUndo(targetController, "Add sliced animation states");

        int x = 0;
        int y = 0;

        foreach (AnimationClip clip in clips)
        {
            if (!IsSliceName(clip.name, slices) || StateExists(stateMachine, clip.name))
            {
                continue;
            }

            AnimatorState state = stateMachine.AddState(clip.name, new Vector3(700f + x * 220f, 120f + y * 60f, 0f));
            state.motion = clip;

            y++;

            if (y >= 14)
            {
                y = 0;
                x++;
            }
        }

        EditorUtility.SetDirty(targetController);
        AssetDatabase.SaveAssets();
    }

    private static AnimationClip[] LoadAnimationClips(string assetPath)
    {
        List<AnimationClip> clips = new List<AnimationClip>();
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

        foreach (Object asset in assets)
        {
            AnimationClip clip = asset as AnimationClip;

            if (clip != null && !clip.name.StartsWith("__preview__", System.StringComparison.OrdinalIgnoreCase))
            {
                clips.Add(clip);
            }
        }

        return clips.ToArray();
    }

    private static bool IsSliceName(string clipName, List<ModelImporterClipAnimation> slices)
    {
        foreach (ModelImporterClipAnimation slice in slices)
        {
            if (slice.name == clipName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool StateExists(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (ChildAnimatorState child in stateMachine.states)
        {
            if (child.state != null && child.state.name == stateName)
            {
                return true;
            }
        }

        return false;
    }
}
