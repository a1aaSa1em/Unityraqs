// Place this file in: Assets/Editor/RenameStatesToClips.cs
// (Create the "Editor" folder if it doesn't exist — that's important!
//  Editor scripts ONLY work if they're inside a folder called Editor.)
//
// After saving, this adds a menu item: Tools > Rename Animator States to Clip Names
// Click that with the LolaAnimator controller selected to auto-rename all states.

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class RenameStatesToClips
{
    [MenuItem("Tools/Rename Animator States to Clip Names")]
    public static void RenameStates()
    {
        // The selected object should be the AnimatorController asset
        AnimatorController controller = Selection.activeObject as AnimatorController;
        if (controller == null)
        {
            EditorUtility.DisplayDialog(
                "Wrong selection",
                "Select the AnimatorController asset in the Project window first " +
                "(usually 'LolaAnimator.controller' under Assets/Animations/).",
                "OK"
            );
            return;
        }

        int renameCount = 0;
        int skipCount = 0;

        foreach (AnimatorControllerLayer layer in controller.layers)
        {
            foreach (ChildAnimatorState child in layer.stateMachine.states)
            {
                AnimatorState state = child.state;

                // Get the motion (animation clip) attached to this state
                if (state.motion == null)
                {
                    Debug.Log($"Skipping '{state.name}' — no motion attached.");
                    skipCount++;
                    continue;
                }

                string desiredName = state.motion.name;

                // Don't rename if already correct
                if (state.name == desiredName)
                {
                    skipCount++;
                    continue;
                }

                Debug.Log($"Renaming '{state.name}' -> '{desiredName}'");
                state.name = desiredName;
                renameCount++;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Done",
            $"Renamed {renameCount} states.\n" +
            $"Skipped {skipCount} states (already correct or no motion).",
            "OK"
        );
    }
}