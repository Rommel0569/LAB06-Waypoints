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
        FrameCircuit();
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
                    {
                        EditorSceneManager.SetActiveScene(scene);
                        CarCircuitDemo.CreateEditModePreview(root);
                        if (scene.isDirty) EditorSceneManager.SaveScene(scene);
                        AssetDatabase.SaveAssets();
                        FrameCircuit();
                        return;
                    }
            }
        }
        if (EditorSceneManager.GetActiveScene().isDirty) return;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject bootstrap = GameObject.Find("CostaAzulRaceBootstrap");
        CarCircuitDemo.CreateEditModePreview(bootstrap);
        SavePreview(bootstrap);
        FrameCircuit();
    }

    private static void SavePreview(GameObject root)
    {
        if (root == null) return;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
    }

    private static void FrameCircuit()
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
        {
            view.Frame(new Bounds(Vector3.zero, new Vector3(115f, 16f, 105f)), false);
            view.Repaint();
        }
    }
}
#endif



