using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AddRaqsBellyStates
{
    private const string AnimationFolder = "Assets/Animations/Raqs Sliced Source";

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
            List<AnimationClip> clips = FindLatestSlicedClips(path);

            foreach (AnimationClip clip in clips)
            {
                string stateName = clip.name;

                if (StateExists(stateMachine, stateName))
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
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Raqs states added",
            $"Added {added} latest sliced Ch29 phrase states to {controller.name}.\nSkipped {skipped} states that already existed.",
            "OK"
        );
    }

    private static bool IsLatestSlicedClipName(string stateName)
    {
        return !string.IsNullOrEmpty(stateName) &&
               stateName.StartsWith("Ch29_nonPBR_", System.StringComparison.Ordinal) &&
               stateName.Contains("_phrase_");
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

    private static List<AnimationClip> FindLatestSlicedClips(string assetPath)
    {
        List<AnimationClip> clips = new List<AnimationClip>();
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

        foreach (Object asset in assets)
        {
            AnimationClip clip = asset as AnimationClip;

            if (clip != null &&
                !clip.name.StartsWith("__preview__", System.StringComparison.OrdinalIgnoreCase) &&
                IsLatestSlicedClipName(clip.name))
            {
                clips.Add(clip);
            }
        }

        return clips;
    }
}
