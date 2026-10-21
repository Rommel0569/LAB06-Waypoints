#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MainMenuScenePreview
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";

    static MainMenuScenePreview()
    {
        EditorApplication.delayCall += EnsurePreview;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += EnsurePreview;
    }

    private static void EnsurePreview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;
        SceneAsset menuAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (menuAsset == null) return;
        // Pressing Play from CarCircuit or TankRacing should still start at the mode selector.
        EditorSceneManager.playModeStartScene = menuAsset;
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (EditorSceneManager.GetActiveScene().isDirty) return;
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject root = null;
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            if (sceneRoot.name == "Costa Azul Main Menu") { root = sceneRoot; break; }
        if (root == null) return;
        bool changed = false;
        MainMenuController controller = root.GetComponent<MainMenuController>();
        if (controller == null) { controller = Undo.AddComponent<MainMenuController>(root); changed = true; }

        Transform canvas = root.transform.Find("Main Menu Canvas");
        if (canvas == null) return;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null && canvasRect.localScale != Vector3.one)
        {
            canvasRect.localScale = Vector3.one;
            changed = true;
        }
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null) { Undo.AddComponent<GraphicRaycaster>(canvas.gameObject); changed = true; }
        else if (!raycaster.enabled) { raycaster.enabled = true; changed = true; }
        changed |= EnsureButton(canvas.Find("Tank Mode Card"), controller, controller.LoadTankMode);
        changed |= EnsureButton(canvas.Find("Tank Mode Card/Enter Mode"), controller, controller.LoadTankMode);
        changed |= EnsureFrontOverlay(canvas.Find("Tank Mode Card"), controller, controller.LoadTankMode);
        changed |= EnsureButton(canvas.Find("Car Mode Card"), controller, controller.LoadCarMode);
        changed |= EnsureButton(canvas.Find("Car Mode Card/Enter Mode"), controller, controller.LoadCarMode);
        changed |= EnsureFrontOverlay(canvas.Find("Car Mode Card"), controller, controller.LoadCarMode);
        changed |= EnsureEventSystem(scene);

        if (!changed) return;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static bool EnsureButton(Transform item, MainMenuController controller, UnityEngine.Events.UnityAction action)
    {
        if (item == null) return false;
        bool changed = false;
        Image image = item.GetComponent<Image>();
        if (image == null) { image = Undo.AddComponent<Image>(item.gameObject); changed = true; }
        image.raycastTarget = true;

        Button button = item.GetComponent<Button>();
        if (button == null) { button = Undo.AddComponent<Button>(item.gameObject); changed = true; }
        button.targetGraphic = image;
        button.interactable = true;
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        bool hasExpectedListener = false;
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (button.onClick.GetPersistentTarget(i) == controller &&
                button.onClick.GetPersistentMethodName(i) == action.Method.Name && !hasExpectedListener)
            {
                hasExpectedListener = true;
                continue;
            }
            UnityEventTools.RemovePersistentListener(button.onClick, i);
            changed = true;
        }
        if (!hasExpectedListener)
        {
            UnityEventTools.AddPersistentListener(button.onClick, action);
            changed = true;
        }

        EditorUtility.SetDirty(image);
        EditorUtility.SetDirty(button);
        return changed;
    }

    private static bool EnsureEventSystem(Scene scene)
    {
        EventSystem system = null;
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
        {
            system = sceneRoot.GetComponentInChildren<EventSystem>(true);
            if (system != null) break;
        }
        bool changed = false;
        if (system == null)
        {
            GameObject eventObject = new GameObject("Menu Event System", typeof(EventSystem));
            SceneManager.MoveGameObjectToScene(eventObject, scene);
            Undo.RegisterCreatedObjectUndo(eventObject, "Create menu EventSystem");
            system = eventObject.GetComponent<EventSystem>();
            changed = true;
        }

        BaseInputModule[] modules = system.GetComponents<BaseInputModule>();
        foreach (BaseInputModule module in modules)
        {
            if (module is InputSystemUIInputModule) continue;
            Undo.DestroyObjectImmediate(module);
            changed = true;
        }
        if (system.GetComponent<InputSystemUIInputModule>() == null)
        {
            Undo.AddComponent<InputSystemUIInputModule>(system.gameObject);
            changed = true;
        }
        return changed;
    }

    private static bool EnsureFrontOverlay(Transform card, MainMenuController controller, UnityEngine.Events.UnityAction action)
    {
        if (card == null) return false;
        Transform overlay = card.Find("Mode Click Overlay");
        bool changed = false;
        if (overlay == null)
        {
            GameObject overlayObject = new GameObject("Mode Click Overlay", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(overlayObject, "Create foreground mode button");
            overlayObject.transform.SetParent(card, false);
            RectTransform rect = overlayObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            overlay = overlayObject.transform;
            changed = true;
        }

        Image image = overlay.GetComponent<Image>();
        if (image == null) { image = Undo.AddComponent<Image>(overlay.gameObject); changed = true; }
        image.color = new Color(0.04f, 0.55f, 0.58f, 0f);
        image.raycastTarget = true;
        Button button = overlay.GetComponent<Button>();
        if (button == null) { button = Undo.AddComponent<Button>(overlay.gameObject); changed = true; }
        button.targetGraphic = image;
        button.interactable = true;
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0f);
        colors.highlightedColor = new Color(0.24f, 1f, 0.93f, 0.10f);
        colors.pressedColor = new Color(0.24f, 1f, 0.93f, 0.22f);
        colors.selectedColor = new Color(0.24f, 1f, 0.93f, 0.10f);
        button.colors = colors;

        bool hasAction = false;
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (button.onClick.GetPersistentTarget(i) == controller &&
                button.onClick.GetPersistentMethodName(i) == action.Method.Name && !hasAction)
            {
                hasAction = true;
                continue;
            }
            UnityEventTools.RemovePersistentListener(button.onClick, i);
            changed = true;
        }
        if (!hasAction)
        {
            UnityEventTools.AddPersistentListener(button.onClick, action);
            changed = true;
        }
        overlay.SetAsLastSibling();
        EditorUtility.SetDirty(image);
        EditorUtility.SetDirty(button);
        return changed;
    }
}
#endif
