#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class MainMenuScenePreview
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";

    static MainMenuScenePreview() { EditorApplication.delayCall += EnsurePreview; }

    private static void EnsurePreview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) return;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (scene.path == ScenePath) return;
        }
        if (EditorSceneManager.GetActiveScene().isDirty) return;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = GameObject.Find("Costa Azul Main Menu");
        if (root == null) return;
        if (root.GetComponent<MainMenuController>() == null) root.AddComponent<MainMenuController>();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
    }
}
#endif
