using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ImportLatestMixamoDance
{
    private const string SourceFbxPath = "/Users/alaasalem/Downloads/Ch14_nonPBR@Dancing.fbx";
    private const string ImportFolder = "Assets/Animations/Mixamo Imports";
    private const string ImportedFbxPath = ImportFolder + "/Ch14_nonPBR@Dancing.fbx";
    private const string ControllerPath = "Assets/Animations/LolaAnimator.controller";
    private const string StateName = "Mixamo_Ch14_Dancing_Showreel";

    [MenuItem("Tools/Raqs/Import Latest Mixamo Dance")]
    public static void ImportForMenu()
    {
        RunForBatch();
        EditorUtility.DisplayDialog(
            "Mixamo dance imported",
            "Imported the latest Ch14 dance, added it to LolaAnimator.controller, and set it as the default state.",
            "OK"
        );
    }

    public static void RunForBatch()
    {
        EnsureFolder("Assets/Animations", "Mixamo Imports");
        CopyFbxIntoProject();
        AssetDatabase.ImportAsset(ImportedFbxPath, ImportAssetOptions.ForceUpdate);
        ConfigureModelImporter();

        AnimationClip clip = FindFirstAnimationClip(ImportedFbxPath);
        if (clip == null)
        {
            Debug.LogError($"No animation clip found in {ImportedFbxPath}");
            return;
        }

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError($"Animator controller not found: {ControllerPath}");
            return;
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState state = FindState(stateMachine, StateName);
        if (state == null)
        {
            state = stateMachine.AddState(StateName, new Vector3(700, 80, 0));
        }

        state.motion = clip;
        state.speed = 1f;
        stateMachine.defaultState = state;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        TryPointSceneAnimatorsAtController(controller);
        Debug.Log($"Imported {ImportedFbxPath}, assigned clip '{clip.name}' to state '{StateName}', and made it the default state.");
    }

    private static void CopyFbxIntoProject()
    {
        if (!File.Exists(SourceFbxPath))
        {
            Debug.LogError($"Source FBX not found: {SourceFbxPath}");
            return;
        }

        File.Copy(SourceFbxPath, ImportedFbxPath, true);
        File.SetLastWriteTimeUtc(ImportedFbxPath, File.GetLastWriteTimeUtc(SourceFbxPath));
    }

    private static void ConfigureModelImporter()
    {
        ModelImporter importer = AssetImporter.GetAtPath(ImportedFbxPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError($"Could not get ModelImporter for {ImportedFbxPath}");
            return;
        }

        bool changed = false;
        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            changed = true;
        }

        if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            changed = true;
        }

        importer.importAnimation = true;
        importer.importCameras = false;
        importer.importLights = false;

        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        if (clips != null && clips.Length > 0)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = true;
                clips[i].loopPose = true;
                clips[i].keepOriginalPositionY = true;
                clips[i].keepOriginalOrientation = true;
            }

            importer.clipAnimations = clips;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static AnimationClip FindFirstAnimationClip(string assetPath)
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

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (ChildAnimatorState child in stateMachine.states)
        {
            if (child.state != null && child.state.name == stateName)
            {
                return child.state;
            }
        }

        return null;
    }

    private static void TryPointSceneAnimatorsAtController(RuntimeAnimatorController controller)
    {
        string scenePath = "Assets/RaqsModel.unity";
        if (File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);
        }

        Animator[] animators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (Animator animator in animators)
        {
            if (animator.gameObject.name == "Ch14_nonPBR" ||
                animator.gameObject.name == "Elmo Rigged" ||
                animator.gameObject.name.Contains("Lola"))
            {
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = true;
                EditorUtility.SetDirty(animator);
            }
        }

        DrumDanceController danceController = Object.FindAnyObjectByType<DrumDanceController>();
        if (danceController != null)
        {
            danceController.BaseAnimatorSpeed = 1f;
            danceController.PulseAnimatorSpeedOnHits = false;

            Animator preferred = GameObject.Find("Ch14_nonPBR")?.GetComponentInChildren<Animator>();
            if (preferred == null)
            {
                preferred = GameObject.Find("Elmo Rigged")?.GetComponentInChildren<Animator>();
            }

            if (preferred != null)
            {
                danceController.LolaAnimator = preferred;
                EditorUtility.SetDirty(danceController);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void EnsureFolder(string parent, string child)
    {
        string folder = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
