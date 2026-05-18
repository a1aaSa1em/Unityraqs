using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CreateBailandoComparisonScene
{
    private const string ScenePath = "Assets/Scenes/BailandoCompare.unity";
    private const string ElmoFbxPath = "Assets/Characters/ElmoRigged/source/Nintendo 64 - Elmos Letter Adventure - Elmo.fbx";
    private const string AnimatorControllerPath = "Assets/Animations/LolaAnimator.controller";
    private const string PoseJsonPath = @"C:\Users\23028727\Downloads\Unityraqs-main (1)\Unityraqs\Assets\StreamingAssets\Bailando\latest_pose.json";
    private const string AutoRepairSessionKey = "Raqs.BailandoCompare.AutoRepairDone";

    static CreateBailandoComparisonScene()
    {
        EditorApplication.delayCall += AutoRepairOpenComparisonScene;
    }

    private static void AutoRepairOpenComparisonScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(AutoRepairSessionKey, false))
        {
            return;
        }

        Scene activeScene = EditorSceneManager.GetActiveScene();

        if (!activeScene.path.EndsWith("BailandoCompare.unity", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (GameObject.Find("Comparison Input Status") != null &&
            GameObject.Find("Bailando Generated Skeleton Preview") == null &&
            Object.FindAnyObjectByType<MicrophoneBeatInput>() != null)
        {
            SessionState.SetBool(AutoRepairSessionKey, true);
            return;
        }

        SessionState.SetBool(AutoRepairSessionKey, true);
        RepairOpenScene(false);
    }

    [MenuItem("Tools/Raqs/Create Bailando Comparison Scene")]
    public static void CreateScene()
    {
        ConfigureElmoImporter();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateLighting();
        CreateStage();

        GameObject maxElmo = CreateElmo("Elmo - Max Mixamo", new Vector3(-1.55f, 0f, 0f));
        GameObject bailandoElmo = CreateElmo("Elmo - Bailando Generated", new Vector3(1.55f, 0f, 0f));

        DrumDanceController maxController = ConfigureMaxMixamoSide(maxElmo);
        BailandoPosePlayer bailandoPlayer = ConfigureBailandoSide(bailandoElmo);
        bailandoPlayer.RhythmSource = maxController;

        CreateLabel("MAX + MIXAMO", new Vector3(-1.55f, 1.9f, 0.25f), 0.052f);
        CreateLabel("BAILANDO TRAINED", new Vector3(1.55f, 1.9f, 0.25f), 0.052f);
        CreateStatus(maxController, bailandoPlayer);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Bailando comparison scene created",
            $"Saved {ScenePath}.\n\nLeft Elmo uses Max OSC + Mixamo Animator states.\nRight Elmo reads Bailando generated pose JSON from:\n{PoseJsonPath}",
            "OK"
        );
    }

    [MenuItem("Tools/Raqs/Repair Open Bailando Comparison Scene")]
    public static void RepairOpenScene()
    {
        RepairOpenScene(true);
    }

    private static void RepairOpenScene(bool showDialog)
    {
        FixLabels();
        FixStageColors();
        RemoveBailandoSkeletonPreviews();

        DrumDanceController maxController = EnsureMaxTestInput();
        BailandoPosePlayer bailandoPlayer = Object.FindAnyObjectByType<BailandoPosePlayer>();

        if (bailandoPlayer != null)
        {
            bailandoPlayer.RhythmSource = maxController;
        }

        if (GameObject.Find("Comparison Input Status") == null)
        {
            CreateStatus(maxController, bailandoPlayer);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Comparison scene repaired",
                "Fixed labels, added Max test input/status, refreshed colors, and removed the Bailando skeleton preview.",
                "OK"
            );
        }
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

    private static GameObject CreateElmo(string name, Vector3 position)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ElmoFbxPath);

        if (prefab == null)
        {
            Debug.LogError($"Could not load Elmo FBX at {ElmoFbxPath}");
            return null;
        }

        GameObject elmo = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (elmo == null)
        {
            Debug.LogError("Could not instantiate Elmo FBX.");
            return null;
        }

        elmo.name = name;
        elmo.transform.position = position;
        elmo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        elmo.transform.localScale = Vector3.one;

        return elmo;
    }

    private static DrumDanceController ConfigureMaxMixamoSide(GameObject elmo)
    {
        if (elmo == null)
        {
            return null;
        }

        Animator animator = elmo.GetComponentInChildren<Animator>();

        if (animator == null)
        {
            animator = elmo.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorControllerPath);
        animator.applyRootMotion = false;

        GameObject controllerObject = new GameObject("Max Mixamo DrumDanceController");
        controllerObject.transform.position = new Vector3(-1.8f, 0f, -0.55f);

        DrumDanceController controller = controllerObject.AddComponent<DrumDanceController>();
        controller.LolaAnimator = animator;
        controller.Mode = DrumDanceController.DanceMode.BufferedPhrases;
        controller.PhraseBufferSeconds = 0.45f;
        controller.SilenceFlushSeconds = 0.12f;
        controller.PhrasePlaySeconds = 1.35f;
        controller.PhraseCrossfade = 0.14f;
        controller.HoldPhraseUntilNextPhrase = true;
        FillSuggestedPhrasePools(controller);

        DrumDanceTestInput testInput = controllerObject.AddComponent<DrumDanceTestInput>();
        testInput.Target = controller;
        testInput.EnableAutomaticPattern = false;
        testInput.AutoPlayWhenNoLiveInput = false;

        MicrophoneBeatInput microphoneInput = controllerObject.AddComponent<MicrophoneBeatInput>();
        microphoneInput.Target = controller;
        microphoneInput.EnableMicrophoneInput = true;

        return controller;
    }

    private static BailandoPosePlayer ConfigureBailandoSide(GameObject elmo)
    {
        if (elmo == null)
        {
            return null;
        }

        Animator animator = elmo.GetComponentInChildren<Animator>();

        if (animator == null)
        {
            animator = elmo.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = null;
        animator.applyRootMotion = false;

        BailandoPosePlayer player = elmo.AddComponent<BailandoPosePlayer>();
        player.TargetAnimator = animator;
        player.ExternalPoseJsonPath = PoseJsonPath;
        player.EnableRetargeting = true;
        player.DisableAnimatorWhilePlaying = true;
        player.PlayOnStart = true;
        player.DriveFromRhythmInput = true;
        player.PauseWhenNoRecentInput = true;
        player.JumpToPhraseOnHit = true;
        player.ApplyRootMotion = false;
        player.PoseScale = 1f;
        player.RotationSmoothing = 28f;
        player.RetargetWeight = 0.65f;
        player.FlipZ = false;

        return player;
    }

    private static void RemoveBailandoSkeletonPreviews()
    {
        foreach (BailandoSkeletonPreview preview in Object.FindObjectsByType<BailandoSkeletonPreview>(FindObjectsInactive.Include))
        {
            if (preview != null)
            {
                Object.DestroyImmediate(preview.gameObject);
            }
        }

        GameObject namedPreview = GameObject.Find("Bailando Generated Skeleton Preview");

        if (namedPreview != null)
        {
            Object.DestroyImmediate(namedPreview);
        }
    }

    private static void CreateSkeletonPreview(Animator targetAnimator = null)
    {
        GameObject preview = new GameObject("Bailando Generated Skeleton Preview");
        preview.transform.position = new Vector3(1.55f, 0f, 0f);
        preview.transform.rotation = Quaternion.identity;

        BailandoSkeletonPreview skeletonPreview = preview.AddComponent<BailandoSkeletonPreview>();
        skeletonPreview.ExternalPoseJsonPath = PoseJsonPath;
        skeletonPreview.TargetAnimator = targetAnimator;
        skeletonPreview.AutoFitToTarget = false;
        skeletonPreview.Offset = new Vector3(0f, 0.85f, -0.72f);
        skeletonPreview.PoseScale = 1.2f;
    }

    private static void FillSuggestedPhrasePools(DrumDanceController controller)
    {
        controller.PhrasePools = new List<DrumDanceController.DancePhrase>
        {
            Phrase("Grounded belly flow", "Any", 4, "Bellydancing", "Belly Dance", "belly_continuous", "belly_b_continuous", "belly_a_flow", "belly_extended"),
            Phrase("Doum hip drops", "Doum", 3, "hip_drops_double", "belly_a_dropA", "belly_a_dropB", "accent_belly_pop_1", "accent_belly_pop_2"),
            Phrase("Tek rolls", "Tek", 2, "accent_belly_roll_1", "accent_belly_roll_2", "belly_roll_slow", "belly_roll_varied", "belly_a_roll_open", "belly_a_roll_mid"),
            Phrase("Ka sways", "Ka", 2, "accent_belly_sway_1", "accent_belly_sway_2", "belly_a_undulation", "belly_b_open", "belly_b_returnA", "belly_b_returnB"),
            Phrase("Trillo ornaments", "Trillo", 1, "accent_bellydance_1", "accent_bellydance_2", "accent_bellydance_3", "accent_bellydance_4", "accent_bellydance_5", "accent_bellydance_6", "accent_bellydance_7", "accent_belly_final_1", "accent_belly_final_2")
        };
    }

    private static DrumDanceController.DancePhrase Phrase(string name, string preferredStroke, int weight, params string[] states)
    {
        return new DrumDanceController.DancePhrase
        {
            Name = name,
            PreferredStroke = preferredStroke,
            Weight = weight,
            States = new List<string>(states)
        };
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 1.25f, -5.1f);
        cameraObject.transform.rotation = Quaternion.Euler(7f, 0f, 0f);
        camera.fieldOfView = 39f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.09f, 0.075f, 0.075f);
    }

    private static void CreateLighting()
    {
        RenderSettings.ambientLight = new Color(0.34f, 0.29f, 0.28f);

        CreateLight("Warm Key", LightType.Spot, new Vector3(-2.3f, 3.3f, -2.5f), Quaternion.Euler(57f, 27f, 0f), new Color(1f, 0.78f, 0.52f), 5.0f, 52f);
        CreateLight("Cool Fill", LightType.Spot, new Vector3(2.3f, 3.1f, -2.5f), Quaternion.Euler(58f, -28f, 0f), new Color(0.66f, 0.78f, 1f), 2.0f, 58f);
        CreateLight("Back Glow", LightType.Point, new Vector3(0f, 1.45f, 1.45f), Quaternion.identity, new Color(1f, 0.34f, 0.18f), 2.0f, 0f);
    }

    private static void CreateLight(string name, LightType type, Vector3 position, Quaternion rotation, Color color, float intensity, float spotAngle)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.position = position;
        lightObject.transform.rotation = rotation;

        Light light = lightObject.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = 9f;

        if (type == LightType.Spot)
        {
            light.spotAngle = spotAngle;
            light.innerSpotAngle = spotAngle * 0.55f;
        }
    }

    private static void CreateStage()
    {
        Material platformMaterial = CreateRuntimeMaterial("Comparison Warm Wood", new Color(0.36f, 0.23f, 0.16f));
        Material trimMaterial = CreateRuntimeMaterial("Comparison Soft Gold", new Color(0.92f, 0.61f, 0.24f));
        Material leftMaterial = CreateRuntimeMaterial("Max Side Slate", new Color(0.23f, 0.28f, 0.34f));
        Material rightMaterial = CreateRuntimeMaterial("Bailando Side Plum", new Color(0.34f, 0.22f, 0.28f));
        Material wallMaterial = CreateRuntimeMaterial("Comparison Warm Wall", new Color(0.17f, 0.12f, 0.13f));
        Material curtainMaterial = CreateRuntimeMaterial("Comparison Curtain", new Color(0.48f, 0.04f, 0.08f));

        CreateCube("Stage Base", new Vector3(0f, -0.08f, 0f), new Vector3(5.25f, 0.16f, 3.0f), platformMaterial);
        CreateCube("Max Side Floor", new Vector3(-1.31f, 0.012f, 0f), new Vector3(2.50f, 0.025f, 2.85f), leftMaterial);
        CreateCube("Bailando Side Floor", new Vector3(1.31f, 0.014f, 0f), new Vector3(2.50f, 0.025f, 2.85f), rightMaterial);
        CreateCube("Comparison Divider", new Vector3(0f, 0.04f, 0f), new Vector3(0.035f, 0.1f, 2.95f), trimMaterial);
        CreateCube("Front Trim", new Vector3(0f, 0.055f, -1.52f), new Vector3(5.35f, 0.11f, 0.06f), trimMaterial);
        CreateCube("Back Wall", new Vector3(0f, 1.55f, 1.55f), new Vector3(5.7f, 3.1f, 0.12f), wallMaterial);
        CreateCube("Left Curtain", new Vector3(-2.55f, 1.45f, 1.46f), new Vector3(0.42f, 2.75f, 0.18f), curtainMaterial);
        CreateCube("Right Curtain", new Vector3(2.55f, 1.45f, 1.46f), new Vector3(0.42f, 2.75f, 0.18f), curtainMaterial);
        CreateCube("Top Valance", new Vector3(0f, 2.92f, 1.45f), new Vector3(5.7f, 0.32f, 0.22f), curtainMaterial);
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static Material CreateRuntimeMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader)
        {
            name = name,
            color = color
        };

        return material;
    }

    private static void CreateLabel(string text, Vector3 position, float characterSize)
    {
        GameObject labelObject = new GameObject(text);
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.identity;

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = text;
        label.fontSize = 32;
        label.characterSize = characterSize;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = Color.white;
    }

    private static void FixLabels()
    {
        FixLabel("MAX + MIXAMO DATA", "MAX + MIXAMO", new Vector3(-1.55f, 1.9f, 0.25f), 0.052f);
        FixLabel("BAILANDO TRAINED DATA", "BAILANDO TRAINED", new Vector3(1.55f, 1.9f, 0.25f), 0.052f);
        FixLabel("MAX + MIXAMO", "MAX + MIXAMO", new Vector3(-1.55f, 1.9f, 0.25f), 0.052f);
        FixLabel("BAILANDO TRAINED", "BAILANDO TRAINED", new Vector3(1.55f, 1.9f, 0.25f), 0.052f);
    }

    private static void FixLabel(string objectName, string text, Vector3 position, float characterSize)
    {
        GameObject labelObject = GameObject.Find(objectName);

        if (labelObject == null)
        {
            return;
        }

        labelObject.name = text;
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.identity;

        TextMesh label = labelObject.GetComponent<TextMesh>();

        if (label == null)
        {
            label = labelObject.AddComponent<TextMesh>();
        }

        label.text = text;
        label.fontSize = 32;
        label.characterSize = characterSize;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = Color.white;
    }

    private static void FixStageColors()
    {
        SetObjectColor("Stage Base", new Color(0.36f, 0.23f, 0.16f));
        SetObjectColor("Max Side Floor", new Color(0.23f, 0.28f, 0.34f));
        SetObjectColor("Bailando Side Floor", new Color(0.34f, 0.22f, 0.28f));
        SetObjectColor("Comparison Divider", new Color(0.92f, 0.61f, 0.24f));
        SetObjectColor("Back Wall", new Color(0.17f, 0.12f, 0.13f));

        Camera camera = Camera.main;

        if (camera != null)
        {
            camera.backgroundColor = new Color(0.09f, 0.075f, 0.075f);
            camera.fieldOfView = 39f;
            camera.transform.position = new Vector3(0f, 1.25f, -5.1f);
            camera.transform.rotation = Quaternion.Euler(7f, 0f, 0f);
        }
    }

    private static void SetObjectColor(string objectName, Color color)
    {
        GameObject target = GameObject.Find(objectName);

        if (target == null)
        {
            return;
        }

        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = CreateRuntimeMaterial($"{objectName} Material", color);
    }

    private static DrumDanceController EnsureMaxTestInput()
    {
        DrumDanceController controller = Object.FindAnyObjectByType<DrumDanceController>();

        if (controller == null)
        {
            return null;
        }

        controller.PhraseBufferSeconds = 0.45f;
        controller.SilenceFlushSeconds = 0.12f;
        controller.PhrasePlaySeconds = 1.35f;
        controller.PhraseCrossfade = 0.14f;

        DrumDanceTestInput testInput = controller.GetComponent<DrumDanceTestInput>();

        if (testInput == null)
        {
            testInput = controller.gameObject.AddComponent<DrumDanceTestInput>();
        }

        testInput.Target = controller;
        testInput.EnableAutomaticPattern = false;
        testInput.AutoPlayWhenNoLiveInput = false;

        MicrophoneBeatInput microphoneInput = controller.GetComponent<MicrophoneBeatInput>();

        if (microphoneInput == null)
        {
            microphoneInput = controller.gameObject.AddComponent<MicrophoneBeatInput>();
        }

        microphoneInput.Target = controller;
        microphoneInput.EnableMicrophoneInput = true;

        foreach (BailandoPosePlayer player in Object.FindObjectsByType<BailandoPosePlayer>(FindObjectsInactive.Include))
        {
            player.EnableRetargeting = true;
            player.PlayOnStart = true;
            player.RhythmSource = controller;
            player.DriveFromRhythmInput = true;
            player.PauseWhenNoRecentInput = true;
            player.JumpToPhraseOnHit = true;
            player.ApplyRootMotion = false;
            player.RetargetWeight = 0.65f;
            player.RotationSmoothing = 28f;
        }

        RemoveBailandoSkeletonPreviews();

        return controller;
    }

    private static void CreateStatus(DrumDanceController maxController, BailandoPosePlayer bailandoPlayer)
    {
        GameObject statusObject = new GameObject("Comparison Input Status");
        statusObject.transform.position = new Vector3(0f, 0.22f, -1.35f);
        statusObject.transform.rotation = Quaternion.identity;

        TextMesh text = statusObject.AddComponent<TextMesh>();
        text.fontSize = 22;
        text.characterSize = 0.032f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(1f, 0.92f, 0.72f);
        text.text = "Input: mic or Max OSC UDP 7000\nBailando: reacts to the same hits\nTest keys: 1 Doum, 2 Tek, 3 Ka, 4 Trillo";

        RaqsComparisonStatus status = statusObject.AddComponent<RaqsComparisonStatus>();
        status.MaxController = maxController;
        status.BailandoPlayer = bailandoPlayer;
        status.StatusText = text;
    }
}
