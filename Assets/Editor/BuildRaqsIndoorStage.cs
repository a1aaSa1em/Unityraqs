using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildRaqsIndoorStage
{
    private const string RootName = "Raqs Indoor Stage";
    private const string MaterialFolder = "Assets/Materials/RaqsIndoorStage";

    [MenuItem("Tools/Raqs/Build Indoor Stage Environment")]
    public static void Build()
    {
        EnsureMaterialFolder();

        GameObject existing = GameObject.Find(RootName);

        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build indoor stage environment");

        Material floorMat = GetOrCreateMaterial("Raqs_Warm_Wood", new Color(0.52f, 0.30f, 0.14f), 0.35f);
        Material wallMat = GetOrCreateMaterial("Raqs_Charcoal_Wall", new Color(0.055f, 0.052f, 0.06f), 0.7f);
        Material curtainMat = GetOrCreateMaterial("Raqs_Deep_Red_Curtain", new Color(0.42f, 0.035f, 0.055f), 0.55f);
        Material goldMat = GetOrCreateMaterial("Raqs_Soft_Gold_Trim", new Color(0.95f, 0.64f, 0.24f), 0.25f);
        Material rugMat = GetOrCreateMaterial("Raqs_Ruby_Rug", new Color(0.55f, 0.03f, 0.09f), 0.5f);

        CreateCube(root.transform, "Stage Platform", new Vector3(0f, -0.08f, 0f), new Vector3(6.8f, 0.16f, 4.4f), floorMat);
        CreateCube(root.transform, "Front Gold Trim", new Vector3(0f, 0.04f, -2.25f), new Vector3(7.05f, 0.16f, 0.08f), goldMat);
        CreateCube(root.transform, "Back Gold Trim", new Vector3(0f, 0.04f, 2.25f), new Vector3(7.05f, 0.16f, 0.08f), goldMat);
        CreateCube(root.transform, "Left Gold Trim", new Vector3(-3.45f, 0.04f, 0f), new Vector3(0.08f, 0.16f, 4.5f), goldMat);
        CreateCube(root.transform, "Right Gold Trim", new Vector3(3.45f, 0.04f, 0f), new Vector3(0.08f, 0.16f, 4.5f), goldMat);

        CreateCube(root.transform, "Back Wall", new Vector3(0f, 1.75f, 2.6f), new Vector3(8.2f, 3.7f, 0.18f), wallMat);
        CreateCube(root.transform, "Left Wall", new Vector3(-4.05f, 1.75f, 0f), new Vector3(0.18f, 3.7f, 5.4f), wallMat);
        CreateCube(root.transform, "Right Wall", new Vector3(4.05f, 1.75f, 0f), new Vector3(0.18f, 3.7f, 5.4f), wallMat);
        CreateCube(root.transform, "Ceiling Valance", new Vector3(0f, 3.35f, 2.35f), new Vector3(8.2f, 0.35f, 0.55f), curtainMat);

        CreateCube(root.transform, "Left Curtain", new Vector3(-3.25f, 1.55f, 2.35f), new Vector3(0.62f, 2.9f, 0.28f), curtainMat);
        CreateCube(root.transform, "Right Curtain", new Vector3(3.25f, 1.55f, 2.35f), new Vector3(0.62f, 2.9f, 0.28f), curtainMat);
        CreateCube(root.transform, "Center Rug", new Vector3(0f, 0.015f, -0.45f), new Vector3(2.25f, 0.025f, 1.65f), rugMat);

        CreateLight(root.transform, "Warm Key Spot", LightType.Spot, new Vector3(-1.7f, 3.0f, -2.7f), Quaternion.Euler(58f, 28f, 0f), new Color(1f, 0.78f, 0.48f), 4.8f, 46f);
        CreateLight(root.transform, "Cool Fill Spot", LightType.Spot, new Vector3(1.9f, 2.7f, -2.3f), Quaternion.Euler(55f, -30f, 0f), new Color(0.54f, 0.68f, 1f), 1.8f, 58f);
        CreateLight(root.transform, "Amber Back Glow", LightType.Point, new Vector3(0f, 1.55f, 2.15f), Quaternion.identity, new Color(1f, 0.34f, 0.18f), 1.4f, 0f);

        DisableOldOutdoorBoard();
        RepositionCharacter();
        ReframeCamera();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Indoor stage built",
            "Built an indoor stage, disabled the old wooden board, and reframed the camera around the performer.",
            "OK"
        );
    }

    private static void EnsureMaterialFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets/Materials", "RaqsIndoorStage");
        }
    }

    private static Material GetOrCreateMaterial(string name, Color color, float smoothness)
    {
        string path = Path.Combine(MaterialFolder, $"{name}.mat");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(cube, $"Create {name}");
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.localPosition = position;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = scale;

        Renderer renderer = cube.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        return cube;
    }

    private static void CreateLight(Transform parent, string name, LightType type, Vector3 position, Quaternion rotation, Color color, float intensity, float spotAngle)
    {
        GameObject lightObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(lightObject, $"Create {name}");
        lightObject.transform.SetParent(parent);
        lightObject.transform.localPosition = position;
        lightObject.transform.localRotation = rotation;

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

    private static void DisableOldOutdoorBoard()
    {
        GameObject board = GameObject.Find("SM_O_Nen_v2_01");

        if (board == null)
        {
            return;
        }

        Undo.RecordObject(board, "Disable old outdoor board");
        board.SetActive(false);
    }

    private static void RepositionCharacter()
    {
        GameObject character = GameObject.Find("Elmo Rigged");

        if (character == null)
        {
            character = GameObject.Find("Lola Bunny");
        }

        if (character == null)
        {
            return;
        }

        Undo.RecordObject(character.transform, "Move performer to indoor stage");
        character.transform.position = new Vector3(0f, 0.05f, -0.3f);
        character.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    private static void ReframeCamera()
    {
        GameObject cameraObject = GameObject.Find("Main Camera");

        if (cameraObject == null)
        {
            return;
        }

        Undo.RecordObject(cameraObject.transform, "Reframe stage camera");
        cameraObject.transform.position = new Vector3(0f, 1.45f, -5.2f);
        cameraObject.transform.rotation = Quaternion.Euler(8f, 0f, 0f);

        Camera camera = cameraObject.GetComponent<Camera>();

        if (camera == null)
        {
            return;
        }

        Undo.RecordObject(camera, "Configure stage camera");
        camera.fieldOfView = 42f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.018f, 0.022f);
    }
}
