using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Builds a visible in-scene HUD and lets the user navigate both lab scenes.</summary>
[ExecuteAlways]
public sealed class LabNavigationPanel : MonoBehaviour
{
    [SerializeField] private string tankScene = "TankRacing";
    [SerializeField] private string carScene = "CarCircuit";
    private FollowWP[] vehicles = Array.Empty<FollowWP>();
    private VehicleCameraNavigator cameraNavigator;
    private CarCircuitDemo carDemo;
    private RaceLapCounter lapCounter;
    private int selectedIndex = -1;
    private bool carSceneMode;
    private Canvas canvas;
    private Text sceneLabel;
    private Text vehicleLabel;
    private Text modeLabel;
    private Text lapLabel;

    private void OnEnable()
    {
        CarCircuitDemo demo = GetComponent<CarCircuitDemo>();
        if (demo != null) ConfigureCarDemo(demo);
        if (!Application.isPlaying) EnsureVisibleCanvas();
    }

    public void ConfigureCarDemo(CarCircuitDemo demo)
    {
        carSceneMode = demo != null;
        if (demo != null) demo.SetNavigationPanel(this);
    }

    private void Start()
    {
        if (!Application.isPlaying) return;
        EnsureVisibleCanvas();
        RefreshSceneReferences();
        BindButtons();
        UpdateLabels();
    }

    private void Update()
    {
        if (Application.isPlaying) UpdateLabels();
        else if (canvas != null && sceneLabel != null) UpdateLabels();
    }

    private void EnsureVisibleCanvas()
    {
        Transform existing = transform.Find("LAB06 Navigation Canvas");
        if (existing != null)
        {
            canvas = existing.GetComponent<Canvas>();
            Transform panel = existing.Find("Navigation Panel");
            if (panel != null)
            {
                sceneLabel = FindLabel(panel, "Scene Status");
                vehicleLabel = FindLabel(panel, "Selected Vehicle");
                modeLabel = FindLabel(panel, "Mode Status");
                lapLabel = FindLabel(panel, "Lap Status");
            }
            EnsureEventSystem();
            if (!Application.isPlaying) UpdateLabels();
            return;
        }

        GameObject canvasObject = new GameObject("LAB06 Navigation Canvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("Navigation Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        AnchorTopLeft(panelRect, 14f, 14f, 468f, 252f);
        panelObject.GetComponent<Image>().color = new Color(0.035f, 0.09f, 0.13f, 0.94f);

        CreateLabel(panelObject.transform, "Panel Title", "LAB 06  |  NAVEGACION", 16f, 14f, 434f, 28f,
            18, FontStyle.Bold, new Color(0.43f, 0.91f, 0.86f), TextAnchor.MiddleLeft);
        sceneLabel = CreateLabel(panelObject.transform, "Scene Status", "Escena actual", 16f, 43f, 434f, 24f,
            13, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
        CreateButton(panelObject.transform, "Open Tank Scene", "TANQUES", 16f, 73f, 210f, 34f,
            () => LoadScene(tankScene));
        CreateButton(panelObject.transform, "Open Car Scene", "CIRCUITO DE AUTOS", 242f, 73f, 210f, 34f,
            () => LoadScene(carScene));

        CreateLabel(panelObject.transform, "Vehicle Caption", "VEHICULO", 16f, 116f, 86f, 30f,
            11, FontStyle.Bold, new Color(0.62f, 0.74f, 0.79f), TextAnchor.MiddleLeft);
        CreateButton(panelObject.transform, "Previous Vehicle", "<", 106f, 116f, 38f, 30f,
            () => StepVehicle(-1));
        vehicleLabel = CreateLabel(panelObject.transform, "Selected Vehicle", "Vista general", 150f, 116f, 256f, 30f,
            13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        CreateButton(panelObject.transform, "Next Vehicle", ">", 412f, 116f, 40f, 30f,
            () => StepVehicle(1));

        CreateButton(panelObject.transform, "Focus Vehicle", "ENFOCAR VEHICULO", 16f, 155f, 210f, 34f,
            () => FocusVehicle(selectedIndex < 0 ? 0 : selectedIndex));
        CreateButton(panelObject.transform, "Overview", "VISTA GENERAL", 242f, 155f, 210f, 34f,
            ShowOverview);
        CreateButton(panelObject.transform, "Movement Mode", "MODO DE MOVIMIENTO", 16f, 198f, 250f, 36f,
            ToggleMovementMode);
        modeLabel = CreateLabel(panelObject.transform, "Mode Status", "", 274f, 198f, 178f, 18f,
            10, FontStyle.Normal, new Color(0.74f, 0.83f, 0.85f), TextAnchor.MiddleLeft);
        lapLabel = CreateLabel(panelObject.transform, "Lap Status", "Vueltas: 0", 274f, 215f, 178f, 17f,
            10, FontStyle.Normal, new Color(0.74f, 0.83f, 0.85f), TextAnchor.MiddleLeft);

        EnsureEventSystem();
        RefreshSceneReferences();
        UpdateLabels();
    }

    private void EnsureEventSystem()
    {
        EventSystem system = GetComponentInChildren<EventSystem>(true);
        GameObject eventSystemObject;
        if (system == null)
        {
            eventSystemObject = new GameObject("LAB06 UI Event System", typeof(EventSystem));
            eventSystemObject.transform.SetParent(transform, false);
        }
        else
        {
            eventSystemObject = system.gameObject;
            if (system.GetComponent<BaseInputModule>() != null) return;
        }
        Type uiModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (uiModuleType == null)
        {
            Debug.LogWarning("Input System aun no se ha cargado; el panel instalara su modulo de UI al resolver el paquete.", this);
            return;
        }
        eventSystemObject.AddComponent(uiModuleType);
    }

    private void BindButtons()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
            button.onClick.RemoveAllListeners();
        Button[] buttons = GetComponentsInChildren<Button>(true);
        Bind(buttons, "Open Tank Scene", () => LoadScene(tankScene));
        Bind(buttons, "Open Car Scene", () => LoadScene(carScene));
        Bind(buttons, "Previous Vehicle", () => StepVehicle(-1));
        Bind(buttons, "Next Vehicle", () => StepVehicle(1));
        Bind(buttons, "Focus Vehicle", () => FocusVehicle(selectedIndex < 0 ? 0 : selectedIndex));
        Bind(buttons, "Overview", ShowOverview);
        Bind(buttons, "Movement Mode", ToggleMovementMode);
    }

    private static void Bind(Button[] buttons, string name, UnityEngine.Events.UnityAction action)
    {
        Button button = null;
        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i] != null && buttons[i].name == name) { button = buttons[i]; break; }
        if (button != null) button.onClick.AddListener(action);
    }

    private void RefreshSceneReferences()
    {
        vehicles = FindObjectsByType<FollowWP>(FindObjectsSortMode.None);
        System.Array.Sort(vehicles, (left, right) => string.CompareOrdinal(left.name, right.name));
        if (carSceneMode && carDemo == null) carDemo = GetComponent<CarCircuitDemo>();
        lapCounter = FindFirstObjectByType<RaceLapCounter>();
        Camera camera = Camera.main;
        if (camera == null) camera = FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            cameraNavigator = camera.GetComponent<VehicleCameraNavigator>();
            if (cameraNavigator == null) cameraNavigator = camera.gameObject.AddComponent<VehicleCameraNavigator>();
        }
    }

    private void StepVehicle(int direction)
    {
        if (vehicles.Length == 0) RefreshSceneReferences();
        if (vehicles.Length == 0) return;
        int next = selectedIndex < 0 ? (direction > 0 ? 0 : vehicles.Length - 1) : selectedIndex + direction;
        FocusVehicle((next % vehicles.Length + vehicles.Length) % vehicles.Length);
    }

    private void FocusVehicle(int index)
    {
        if (vehicles.Length == 0) RefreshSceneReferences();
        if (vehicles.Length == 0) return;
        selectedIndex = (index % vehicles.Length + vehicles.Length) % vehicles.Length;
        if (cameraNavigator != null) cameraNavigator.Focus(vehicles[selectedIndex].transform);
        UpdateLabels();
    }

    private void ShowOverview()
    {
        selectedIndex = -1;
        if (cameraNavigator != null) cameraNavigator.ShowOverview();
        UpdateLabels();
    }

    private void ToggleMovementMode()
    {
        if (carDemo == null && carSceneMode) carDemo = GetComponent<CarCircuitDemo>();
        if (carDemo != null) carDemo.SetTrackerMode(!carDemo.TrackerMode);
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (sceneLabel == null) return;
        if (carDemo == null && carSceneMode) carDemo = GetComponent<CarCircuitDemo>();
        if (vehicles.Length == 0 && !Application.isPlaying)
            vehicles = GetComponentsInChildren<FollowWP>(true);
        sceneLabel.text = "Escena actual: " + SceneManager.GetActiveScene().name;
        vehicleLabel.text = selectedIndex >= 0 && selectedIndex < vehicles.Length
            ? vehicles[selectedIndex].name : "Vista general  |  " + vehicles.Length + " vehiculos";
        bool carSceneLoaded = carDemo != null && carDemo.HasScenePreview;
        modeLabel.text = carSceneLoaded
            ? (carDemo.TrackerMode ? "Tracker movil activo" : "Waypoints directos activos")
            : "Ruta cerrada de waypoints";
        int laps = lapCounter != null && selectedIndex >= 0 && selectedIndex < vehicles.Length
            ? lapCounter.GetLaps(vehicles[selectedIndex]) : 0;
        lapLabel.text = carSceneLoaded ? "Vueltas completadas: " + laps : "Selecciona un tanque o auto";
    }

    private void LoadScene(string sceneName)
    {
        if (!Application.isPlaying) return;
        if (SceneManager.GetActiveScene().name != sceneName)
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private static Text CreateLabel(Transform parent, string name, string text, float x, float y,
        float width, float height, int size, FontStyle fontStyle, Color color, TextAnchor anchor)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        AnchorTopLeft(obj.GetComponent<RectTransform>(), x, y, width, height);
        Text label = obj.GetComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = size;
        label.fontStyle = fontStyle;
        label.color = color;
        label.alignment = anchor;
        label.raycastTarget = false;
        return label;
    }

    private static Button CreateButton(Transform parent, string name, string caption, float x, float y,
        float width, float height, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        AnchorTopLeft(obj.GetComponent<RectTransform>(), x, y, width, height);
        Image image = obj.GetComponent<Image>();
        image.color = new Color(0.07f, 0.31f, 0.37f, 1f);
        Button button = obj.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.07f, 0.31f, 0.37f, 1f);
        colors.highlightedColor = new Color(0.1f, 0.48f, 0.52f, 1f);
        colors.pressedColor = new Color(0.04f, 0.22f, 0.27f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObj.transform.SetParent(obj.transform, false);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
        Text label = labelObj.GetComponent<Text>();
        label.text = caption;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 12; label.fontStyle = FontStyle.Bold;
        label.color = Color.white; label.alignment = TextAnchor.MiddleCenter;
        button.onClick.AddListener(action);
        return button;
    }

    private static void AnchorTopLeft(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static Text FindLabel(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<Text>() : null;
    }
}
