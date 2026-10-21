using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>Compact race HUD with a clear return-to-mode-selection control.</summary>
[ExecuteAlways]
public sealed class LabNavigationPanel : MonoBehaviour
{
    [SerializeField] private string tankScene = "TankRacing";
    [SerializeField] private string carScene = "CarCircuit";
    [SerializeField] private string menuScene = "MainMenu";

    private Canvas canvas;
    private Text sceneLabel;
    private Text controlLabel;
    private Text speedLabel;
    private PlayerCarController player;
    private CarCircuitDemo carDemo;
    private VehicleCameraNavigator cameraNavigator;
    private bool overviewView;
    private GameObject pauseOverlay;
    private bool isPaused;
    private readonly Transform[] tankTargets = new Transform[4];
    private int cameraTargetIndex = -1;

    private void OnEnable()
    {
        carDemo = GetComponent<CarCircuitDemo>();
        if (carDemo != null) carDemo.SetNavigationPanel(this);
        if (!Application.isPlaying) EnsureCanvas();
    }

    public void ConfigureCarDemo(CarCircuitDemo demo)
    {
        carDemo = demo;
        if (demo != null) demo.SetNavigationPanel(this);
    }

    private void Start()
    {
        if (!Application.isPlaying) return;
        EnsureCanvas();
        player = FindFirstObjectByType<PlayerCarController>();
        tankTargets[0] = FindTank("BlueTank");
        tankTargets[1] = FindTank("RedTank");
        tankTargets[2] = FindTank("YellowTank");
        tankTargets[3] = FindTank("GreenTank");
        Camera camera = Camera.main;
        if (camera != null) cameraNavigator = camera.GetComponent<VehicleCameraNavigator>();
        BindButtons();
        EnsureEventSystem();
        EnsurePauseOverlay();
        RefreshLabels();
    }

    private void Update()
    {
        if (canvas == null) EnsureCanvas();
        if (Application.isPlaying)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePause();
            RefreshLabels();
        }
    }

    private void EnsureCanvas()
    {
        Transform existing = transform.Find("LAB06 Race HUD");
        Transform legacy = transform.Find("LAB06 Navigation Canvas");
        if (legacy != null)
        {
            if (Application.isPlaying) Destroy(legacy.gameObject);
            else DestroyImmediate(legacy.gameObject);
        }
        if (existing != null)
        {
            Transform panel = existing.Find("HUD Panel");
            if (panel == null || FindText(panel, "Controls") == null || FindText(panel, "Speed") == null ||
                panel.Find("Pause") == null || panel.Find("Exit") == null || panel.Find("Main Menu") == null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
                existing = null;
            }
        }
        if (existing != null)
        {
            canvas = existing.GetComponent<Canvas>();
            Transform panel = existing.Find("HUD Panel");
            if (panel != null)
            {
                sceneLabel = FindText(panel, "Scene Label");
                controlLabel = FindText(panel, "Controls");
                speedLabel = FindText(panel, "Speed");
            }
            return;
        }

        GameObject canvasObject = new GameObject("LAB06 Race HUD", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("HUD Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1); rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1); rect.anchoredPosition = new Vector2(-22, -22);
        rect.sizeDelta = new Vector2(410, 236);
        panelObject.GetComponent<Image>().color = new Color(0.035f, 0.075f, 0.095f, 0.88f);

        sceneLabel = CreateText(panelObject.transform, "Scene Label", "COSTA AZUL  |  RACE CONTROL",
            new Vector2(18, -14), new Vector2(350, 26), 16, FontStyle.Bold, new Color(0.4f, 0.91f, 0.86f));
        controlLabel = CreateText(panelObject.transform, "Controls", "WASD / FLECHAS  conducir\nESPACIO  freno de mano",
            new Vector2(18, -46), new Vector2(180, 46), 12, FontStyle.Normal, Color.white);
        speedLabel = CreateText(panelObject.transform, "Speed", "VELOCIDAD  0 km/h",
            new Vector2(210, -48), new Vector2(164, 42), 13, FontStyle.Bold, new Color(0.85f, 0.92f, 0.95f));
        CreateButton(panelObject.transform, "Movement Mode", "MODO WAYPOINTS",
            new Vector2(18, -94), new Vector2(164, 34), ToggleMovementMode);
        CreateButton(panelObject.transform, "Tank Mode", "TANQUES",
            new Vector2(18, -140), new Vector2(82, 34), () => Load(tankScene));
        CreateButton(panelObject.transform, "Car Mode", "CARRERA",
            new Vector2(108, -140), new Vector2(82, 34), () => Load(carScene));
        CreateButton(panelObject.transform, "Main Menu", "CAMBIAR MODO",
            new Vector2(198, -140), new Vector2(176, 34), () => Load(menuScene));
        CreateButton(panelObject.transform, "Pause", "PAUSAR  [ESC]",
            new Vector2(18, -184), new Vector2(174, 34), TogglePause);
        CreateButton(panelObject.transform, "Exit", "SALIR",
            new Vector2(198, -184), new Vector2(176, 34), ExitGame);
        CreateButton(panelObject.transform, "Camera View", "CAMARA",
            new Vector2(210, -94), new Vector2(164, 34), ToggleCameraView);
    }

    private void BindButtons()
    {
        Bind("Main Menu", () => Load(menuScene));
        Bind("Tank Mode", () => Load(tankScene));
        Bind("Car Mode", () => Load(carScene));
        Bind("Camera View", ToggleCameraView);
        Bind("Movement Mode", ToggleMovementMode);
        Bind("Pause", TogglePause);
        Bind("Exit", ExitGame);
    }

    private void Bind(string name, UnityEngine.Events.UnityAction action)
    {
        Transform buttonObject = transform.Find("LAB06 Race HUD/HUD Panel/" + name);
        Button button = buttonObject == null ? null : buttonObject.GetComponent<Button>();
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void EnsureEventSystem()
    {
        EventSystem system = FindFirstObjectByType<EventSystem>();
        GameObject target = system != null ? system.gameObject : new GameObject("LAB06 UI Event System", typeof(EventSystem));
        if (system == null) target.transform.SetParent(transform, false);
        if (target.GetComponent<BaseInputModule>() != null) return;
        System.Type inputModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem", false);
        if (inputModule != null) target.AddComponent(inputModule);
        else Debug.LogError("El paquete Input System no tiene su modulo de UI cargado; los botones no recibiran clics.", this);
    }

    private void ToggleCameraView()
    {
        if (cameraNavigator == null)
        {
            Camera camera = Camera.main;
            if (camera != null) cameraNavigator = camera.GetComponent<VehicleCameraNavigator>();
        }
        if (cameraNavigator == null) return;
        if (SceneManager.GetActiveScene().name == tankScene)
        {
            cameraTargetIndex++;
            while (cameraTargetIndex < tankTargets.Length && tankTargets[cameraTargetIndex] == null)
                cameraTargetIndex++;
            if (cameraTargetIndex >= tankTargets.Length)
            {
                cameraTargetIndex = -1;
                cameraNavigator.ShowOverview();
            }
            else cameraNavigator.Focus(tankTargets[cameraTargetIndex]);
            return;
        }
        overviewView = !overviewView;
        if (overviewView) cameraNavigator.ShowOverview();
        else if (player != null) cameraNavigator.Focus(player.transform);
    }

    private static Transform FindTank(string tankName)
    {
        GameObject tank = GameObject.Find(tankName);
        return tank == null ? null : tank.transform;
    }

    private int CountTankTargets()
    {
        int count = 0;
        for (int i = 0; i < tankTargets.Length; i++) if (tankTargets[i] != null) count++;
        return count;
    }

    private void RefreshLabels()
    {
        if (sceneLabel == null) return;
        string active = SceneManager.GetActiveScene().name;
        sceneLabel.text = active == carScene ? "COSTA AZUL  |  CARRERA" : "LAB 06  |  WAYPOINTS - TANQUES";
        if (speedLabel == null || controlLabel == null) return;
        player = player == null ? FindFirstObjectByType<PlayerCarController>() : player;
        if (carDemo == null) carDemo = FindFirstObjectByType<CarCircuitDemo>();
        bool driving = player != null && active == carScene;
        bool tankMode = active == tankScene;
        controlLabel.text = driving
            ? "WASD / FLECHAS  conducir\nESPACIO  freno de mano"
            : tankMode ? "4 TANQUES IA\n9 WAYPOINTS  ·  circuito activo"
                       : "SELECCION DE CIRCUITO\nTanques o carrera de autos";
        speedLabel.text = driving ? "VELOCIDAD  " + Mathf.RoundToInt(player.SpeedKph) + " km/h"
            : tankMode ? "TANQUES  " + CountTankTargets() + " / 4" : "WAYPOINTS  circuito activo";
        Transform movementButton = transform.Find("LAB06 Race HUD/HUD Panel/Movement Mode");
        if (movementButton != null)
        {
            Button button = movementButton.GetComponent<Button>();
            if (button != null) button.interactable = carDemo != null && driving;
            Text caption = FindText(movementButton, "Label");
            if (caption != null)
                caption.text = carDemo != null && carDemo.TrackerMode ? "MODO TRACKER" : "MODO WAYPOINTS";
        }
        Transform cameraButton = transform.Find("LAB06 Race HUD/HUD Panel/Camera View");
        Text cameraCaption = cameraButton == null ? null : FindText(cameraButton, "Label");
        if (cameraCaption != null) cameraCaption.text = tankMode ? "SIGUIENTE TANQUE" : "CAMARA";
    }

    private void ToggleMovementMode()
    {
        if (carDemo == null) carDemo = FindFirstObjectByType<CarCircuitDemo>();
        if (carDemo != null) carDemo.SetTrackerMode(!carDemo.TrackerMode);
        RefreshLabels();
    }

    private void EnsurePauseOverlay()
    {
        if (canvas == null) return;
        Transform existing = canvas.transform.Find("Pause Overlay");
        if (existing != null)
        {
            pauseOverlay = existing.gameObject;
            pauseOverlay.SetActive(isPaused);
            return;
        }

        pauseOverlay = new GameObject("Pause Overlay", typeof(RectTransform), typeof(Image));
        pauseOverlay.transform.SetParent(canvas.transform, false);
        RectTransform backgroundRect = pauseOverlay.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image background = pauseOverlay.GetComponent<Image>();
        background.color = new Color(0.008f, 0.02f, 0.03f, 0.86f);

        GameObject card = new GameObject("Pause Card", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(pauseOverlay.transform, false);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(440, 330);
        card.GetComponent<Image>().color = new Color(0.035f, 0.075f, 0.095f, 1f);
        CreateText(card.transform, "Pause Title", "JUEGO EN PAUSA", new Vector2(28, -26),
            new Vector2(384, 44), 24, FontStyle.Bold, new Color(0.4f, 0.91f, 0.86f));
        CreateText(card.transform, "Pause Hint", "Tu progreso de la carrera se conserva.", new Vector2(28, -78),
            new Vector2(384, 32), 14, FontStyle.Normal, Color.white);
        CreateButton(card.transform, "Resume", "CONTINUAR", new Vector2(28, -130),
            new Vector2(384, 46), TogglePause);
        CreateButton(card.transform, "Pause Menu", "CAMBIAR MODO", new Vector2(28, -190),
            new Vector2(384, 46), () => Load(menuScene));
        CreateButton(card.transform, "Pause Exit", "SALIR DEL JUEGO", new Vector2(28, -250),
            new Vector2(384, 46), ExitGame);
        pauseOverlay.SetActive(isPaused);
    }

    private void TogglePause()
    {
        if (!Application.isPlaying) return;
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pauseOverlay == null) EnsurePauseOverlay();
        if (pauseOverlay != null) pauseOverlay.SetActive(isPaused);
    }

    private void ExitGame()
    {
        if (!Application.isPlaying) return;
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Load(string scene)
    {
        if (Application.isPlaying && SceneManager.GetActiveScene().name != scene)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }
    }

    private void OnDestroy()
    {
        if (isPaused) Time.timeScale = 1f;
    }

    private static Text FindText(Transform parent, string name)
    {
        Transform found = parent.Find(name);
        return found == null ? null : found.GetComponent<Text>();
    }

    private static Text CreateText(Transform parent, string name, string value, Vector2 position,
        Vector2 size, int fontSize, FontStyle style, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
        Text text = obj.GetComponent<Text>();
        text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize; text.fontStyle = style; text.color = color;
        text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string caption,
        Vector2 position, Vector2 size, UnityEngine.Events.UnityAction callback)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
        Image image = obj.GetComponent<Image>(); image.color = new Color(0.06f, 0.3f, 0.35f, 1);
        Button button = obj.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.1f, 0.49f, 0.52f, 1);
        colors.pressedColor = new Color(0.04f, 0.2f, 0.24f, 1);
        button.colors = colors; button.onClick.AddListener(callback);
        CreateText(obj.transform, "Label", caption, Vector2.zero, size, 12, FontStyle.Bold, Color.white)
            .GetComponent<RectTransform>().anchorMax = Vector2.one;
        RectTransform label = obj.transform.Find("Label").GetComponent<RectTransform>();
        label.anchorMin = Vector2.zero; label.anchorMax = Vector2.one;
        label.pivot = new Vector2(0.5f, 0.5f); label.anchoredPosition = Vector2.zero;
        label.offsetMin = Vector2.zero; label.offsetMax = Vector2.zero;
        Text labelText = label.GetComponent<Text>(); labelText.alignment = TextAnchor.MiddleCenter;
        labelText.fontSize = 11;
        return button;
    }
}
