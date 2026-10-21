#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CarCircuitScenePreview
{
    private const string ScenePath = "Assets/Scenes/CarCircuit.unity";
    static CarCircuitScenePreview()
    {
        EditorApplication.delayCall += EnsurePreview;
    }

    [MenuItem("LAB06/Rebuild Car Circuit Preview")]
    private static void RebuildFromMenu()
    {
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (sceneAsset == null) return;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = GameObject.Find("CostaAzulRaceBootstrap");
        CarCircuitDemo.CreateEditModePreview(root);
        SavePreview(root);
        root.GetComponent<CarCircuitDemo>().EditorSavePreview();
    }

    private static void EnsurePreview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
            EditorApplication.isUpdating) return;
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (sceneAsset == null) return;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetSceneAt(i);
            if (scene.path == ScenePath)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                    if (root.name == "CostaAzulRaceBootstrap" && root.transform.Find("CAR CIRCUIT PREVIEW") != null)
                        return;
            }
        }
        if (EditorSceneManager.GetActiveScene().isDirty) return;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject bootstrap = GameObject.Find("CostaAzulRaceBootstrap");
        CarCircuitDemo.CreateEditModePreview(bootstrap);
        SavePreview(bootstrap);
    }

    private static void SavePreview(GameObject root)
    {
        if (root == null) return;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
    }
}
#endif



