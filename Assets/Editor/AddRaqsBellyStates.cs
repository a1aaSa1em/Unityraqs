using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AddRaqsBellyStates
{
    private const string AnimationFolder = "Assets/Animations/FBX Dance data set";

    [MenuItem("Tools/Raqs/Add Belly FBX States To Selected Controller")]
    public static void AddBellyStates()
    {
        AnimatorController controller = Selection.activeObject as AnimatorController;

        if (controller == null)
        {
            EditorUtility.DisplayDialog(
                "Select controller",
                "Select LolaAnimator.controller in the Project window first, then run this again.",
                "OK"
            );
            return;
        }

        Undo.RegisterCompleteObjectUndo(controller, "Add Raqs belly states");

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        int added = 0;
        int skipped = 0;
        int x = 0;
        int y = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { AnimationFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string stateName = Path.GetFileNameWithoutExtension(path);

            if (!IsBellyStateName(stateName))
            {
                continue;
            }

            if (StateExists(stateMachine, stateName))
            {
                skipped++;
                continue;
            }

            AnimationClip clip = FindFirstAnimationClip(path);

            if (clip == null)
            {
                skipped++;
                continue;
            }

            AnimatorState state = stateMachine.AddState(stateName, new Vector3(450 + x * 220, 120 + y * 60, 0));
            state.motion = clip;

            added++;
            y++;

            if (y >= 12)
            {
                y = 0;
                x++;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Raqs states added",
            $"Added {added} belly/hip states to {controller.name}.\nSkipped {skipped} states that already existed or had no clip.",
            "OK"
        );
    }

    private static bool IsBellyStateName(string stateName)
    {
        string lowerName = stateName.ToLowerInvariant();

        return lowerName.StartsWith("belly") ||
               lowerName.StartsWith("accent_belly") ||
               lowerName == "hip_drops_double" ||
               lowerName.Contains("@belly");
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
}
