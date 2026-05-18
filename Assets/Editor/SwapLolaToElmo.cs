using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SwapLolaToElmo
{
    private const string ElmoFbxPath = "Assets/Characters/ElmoRigged/source/Nintendo 64 - Elmos Letter Adventure - Elmo.fbx";

    [MenuItem("Tools/Raqs/Swap Lola To Elmo Rigged")]
    public static void Swap()
    {
        AssetDatabase.ImportAsset(ElmoFbxPath, ImportAssetOptions.ForceUpdate);
        ConfigureElmoImporter();

        GameObject elmoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ElmoFbxPath);

        if (elmoPrefab == null)
        {
            EditorUtility.DisplayDialog(
                "Elmo not found",
                $"Could not load {ElmoFbxPath}. Check that the FBX exists in the project.",
                "OK"
            );
            return;
        }

        DrumDanceController danceController = Object.FindAnyObjectByType<DrumDanceController>();

        if (danceController == null)
        {
            EditorUtility.DisplayDialog(
                "Controller not found",
                "Could not find DrumDanceController in the open scene. Open RaqsModel or LolaStage first.",
                "OK"
            );
            return;
        }

        Animator oldAnimator = danceController.LolaAnimator;
        GameObject oldCharacter = oldAnimator != null ? oldAnimator.gameObject : GameObject.Find("Lola Bunny");

        if (oldAnimator == null && oldCharacter != null)
        {
            oldAnimator = oldCharacter.GetComponentInChildren<Animator>();
        }

        RuntimeAnimatorController animatorController = oldAnimator != null
            ? oldAnimator.runtimeAnimatorController
            : AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/LolaAnimator.controller");

        Vector3 position = oldCharacter != null ? oldCharacter.transform.position : Vector3.zero;
        Quaternion rotation = oldCharacter != null ? oldCharacter.transform.rotation : Quaternion.identity;
        Vector3 scale = oldCharacter != null ? oldCharacter.transform.localScale : Vector3.one;
        bool applyRootMotion = oldAnimator != null && oldAnimator.applyRootMotion;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        GameObject elmo = PrefabUtility.InstantiatePrefab(elmoPrefab) as GameObject;

        if (elmo == null)
        {
            EditorUtility.DisplayDialog("Swap failed", "Unity could not instantiate the Elmo FBX prefab.", "OK");
            return;
        }

        Undo.RegisterCreatedObjectUndo(elmo, "Create Elmo rigged character");
        elmo.name = "Elmo Rigged";
        elmo.transform.SetPositionAndRotation(position, rotation);
        elmo.transform.localScale = scale;

        Animator elmoAnimator = elmo.GetComponentInChildren<Animator>();

        if (elmoAnimator == null)
        {
            elmoAnimator = elmo.AddComponent<Animator>();
        }

        Undo.RecordObject(elmoAnimator, "Assign Elmo animator controller");
        elmoAnimator.runtimeAnimatorController = animatorController;
        elmoAnimator.applyRootMotion = applyRootMotion;

        Undo.RecordObject(danceController, "Point DrumDanceController to Elmo");
        danceController.LolaAnimator = elmoAnimator;
        EditorUtility.SetDirty(danceController);

        if (oldCharacter != null)
        {
            Undo.RecordObject(oldCharacter, "Disable Lola character");
            oldCharacter.name = "Lola Bunny (disabled)";
            oldCharacter.SetActive(false);
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        string avatarMessage = elmoAnimator.avatar != null
            ? "Elmo has an Avatar assigned."
            : "Elmo does not have a valid Avatar yet. If animations do not move him, open the FBX import Rig tab and set Animation Type to Humanoid.";

        EditorUtility.DisplayDialog(
            "Elmo swapped in",
            $"Elmo Rigged is now using {animatorController?.name} and DrumDanceController points to Elmo.\n\n{avatarMessage}",
            "OK"
        );
    }

    private static void ConfigureElmoImporter()
    {
        ModelImporter importer = AssetImporter.GetAtPath(ElmoFbxPath) as ModelImporter;

        if (importer == null)
        {
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

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }
}
