using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RaqsRecentSlicedClipBrowser : EditorWindow
{
    private const string SlicedFolder = "Assets/Animations/Raqs Sliced Source";

    private readonly List<AnimationClip> clips = new List<AnimationClip>();
    private Vector2 scrollPosition;
    private string searchText = "";
    private bool phrasesOnly = true;
    private int selectedIndex = -1;

    [MenuItem("Tools/Raqs/Recent Sliced Clip Browser")]
    public static void ShowWindow()
    {
        RaqsRecentSlicedClipBrowser window = GetWindow<RaqsRecentSlicedClipBrowser>("Recent Sliced Clips");
        window.RefreshClips();
        window.Show();
    }

    private void OnEnable()
    {
        RefreshClips();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Recent sliced Ch29 clips only", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Click Select to show the clip in the Inspector, then use the Preview panel at the bottom of the Inspector to watch it. Copy Name gives you the exact text to paste into DrumDanceController.",
            MessageType.Info
        );

        EditorGUILayout.BeginHorizontal();
        searchText = EditorGUILayout.TextField("Search", searchText);

        if (GUILayout.Button("Refresh", GUILayout.Width(80f)))
        {
            RefreshClips();
        }
        EditorGUILayout.EndHorizontal();

        phrasesOnly = EditorGUILayout.Toggle("Phrases Only", phrasesOnly);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            SelectNextVisibleClip(-1);
        }

        if (GUILayout.Button("Next"))
        {
            SelectNextVisibleClip(1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField($"Showing clips from: {SlicedFolder}");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < clips.Count; i++)
        {
            AnimationClip clip = clips[i];

            if (!ShouldShowClip(clip))
            {
                continue;
            }

            DrawClipRow(i, clip);
        }

        EditorGUILayout.EndScrollView();
    }

    private void RefreshClips()
    {
        clips.Clear();

        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { SlicedFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            foreach (Object asset in assets)
            {
                AnimationClip clip = asset as AnimationClip;

                if (clip == null || clip.name.StartsWith("__preview__", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                clips.Add(clip);
            }
        }

        clips.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
        selectedIndex = Mathf.Clamp(selectedIndex, -1, clips.Count - 1);
        Repaint();
    }

    private void DrawClipRow(int index, AnimationClip clip)
    {
        GUIStyle labelStyle = index == selectedIndex ? EditorStyles.boldLabel : EditorStyles.label;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(clip.name, labelStyle);

        if (GUILayout.Button("Select", GUILayout.Width(70f)))
        {
            SelectClip(index);
        }

        if (GUILayout.Button("Copy Name", GUILayout.Width(90f)))
        {
            EditorGUIUtility.systemCopyBuffer = clip.name;
        }

        EditorGUILayout.EndHorizontal();
    }

    private bool ShouldShowClip(AnimationClip clip)
    {
        if (clip == null)
        {
            return false;
        }

        if (phrasesOnly && !clip.name.Contains("_phrase_"))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(searchText) ||
               clip.name.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
    }

    private void SelectNextVisibleClip(int direction)
    {
        if (clips.Count == 0)
        {
            return;
        }

        int startIndex = selectedIndex < 0 ? 0 : selectedIndex;

        for (int offset = 1; offset <= clips.Count; offset++)
        {
            int index = (startIndex + direction * offset + clips.Count) % clips.Count;

            if (ShouldShowClip(clips[index]))
            {
                SelectClip(index);
                return;
            }
        }
    }

    private void SelectClip(int index)
    {
        selectedIndex = index;
        AnimationClip clip = clips[index];
        Selection.activeObject = clip;
        EditorGUIUtility.PingObject(clip);
        Repaint();
    }
}
