using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Full-screen main menu with separate Tank and Car game modes.</summary>
[ExecuteAlways]
public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private string tankScene = "TankRacing";
    [SerializeField] private string carScene = "CarCircuit";
    private Canvas canvas;
    private int lastWidth;
    private int lastHeight;

    private void OnEnable()
    {
        if (!Application.isPlaying) BuildMenu();
    }

    private void Start()
    {
        if (!Application.isPlaying) return;
        BuildMenu();
        ApplyResponsiveLayout();
        BindButton("Tank Mode Card", tankScene);
        BindButton("Tank Mode Card/Enter Mode", tankScene);
        BindButton("Car Mode Card", carScene);
        BindButton("Car Mode Card/Enter Mode", carScene);
        EnsureEventSystem();
    }

    private void Update()
    {
        if (!Application.isPlaying || (lastWidth == Screen.width && lastHeight == Screen.height)) return;
        ApplyResponsiveLayout();
    }

    private void BindButton(string path, string scene)
    {
        Transform item = transform.Find("Main Menu Canvas/" + path);
        Button button = item == null ? null : item.GetComponent<Button>();
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => Load(scene));
    }

    private void EnsureEventSystem()
    {
        EventSystem system = FindFirstObjectByType<EventSystem>();
        GameObject target = system != null ? system.gameObject : new GameObject("Menu Event System", typeof(EventSystem));
        if (system == null) target.transform.SetParent(transform, false);
        Type inputModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem", false);
        if (inputModule == null)
        {
            Debug.LogError("El paquete Input System no tiene su modulo de UI cargado; los botones no recibiran clics.", this);
            return;
        }

        // A scene or another bootstrapper may have left the legacy StandaloneInputModule
        // on this EventSystem. It is incompatible with projects using Input System only.
        BaseInputModule[] modules = target.GetComponents<BaseInputModule>();
        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i] != null && modules[i].GetType() != inputModule)
            {
                if (Application.isPlaying) Destroy(modules[i]);
                else DestroyImmediate(modules[i]);
            }
        }

        Component module = target.GetComponent(inputModule);
        if (module == null) module = target.AddComponent(inputModule);
        if (module is Behaviour behaviour) behaviour.enabled = true;

        // Explicitly initialize the module's mouse, touch, navigation and submit actions.
        // OnEnable does this for a new module; this also repairs a reused module with empty actions.
        var actionsAsset = inputModule.GetProperty("actionsAsset");
        if (actionsAsset != null && actionsAsset.GetValue(module) == null)
        {
            var assignDefaultActions = inputModule.GetMethod("AssignDefaultActions");
            if (assignDefaultActions != null) assignDefaultActions.Invoke(module, null);
        }
    }

    private void BuildMenu()
    {
        EnsureMenuCamera();
        Transform existing = transform.Find("Main Menu Canvas");
        if (existing != null)
        {
            if (existing.Find("Tank Mode Card") != null && existing.Find("Car Mode Card") != null)
            {
                canvas = existing.GetComponent<Canvas>();
                return;
            }
            if (Application.isPlaying) Destroy(existing.gameObject);
            else DestroyImmediate(existing.gameObject);
        }
        GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;

        RectTransform full = new GameObject("Menu Background", typeof(RectTransform), typeof(Image))
            .GetComponent<RectTransform>();
        full.SetParent(canvasObject.transform, false);
        full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one;
        full.offsetMin = Vector2.zero; full.offsetMax = Vector2.zero;
        full.GetComponent<Image>().color = new Color(0.018f, 0.04f, 0.06f, 1f);
        CreateImage(canvasObject.transform, "Top Accent", new Vector2(0, 0), new Vector2(1280, 5), new Color(0.12f, 0.86f, 0.78f));
        CreateImage(canvasObject.transform, "Header Bar", new Vector2(0, 5), new Vector2(1280, 62), new Color(0.035f, 0.075f, 0.092f));
        CreateText(canvasObject.transform, "Brand Mark", "CA", 42, 15, 42, 39, 24,
            FontStyle.Bold, new Color(0.15f, 0.93f, 0.82f), TextAnchor.MiddleCenter);
        CreateText(canvasObject.transform, "Brand", "COSTA AZUL RACING", 98, 13, 370, 40, 22,
            FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
        CreateText(canvasObject.transform, "Game Category", "WAYPOINT CHAMPIONSHIP     /     LAB 06", 700, 16, 500, 36, 12,
            FontStyle.Bold, new Color(0.56f, 0.69f, 0.75f), TextAnchor.MiddleRight);
        CreateText(canvasObject.transform, "Edition Tag", "SELECCION DE CIRCUITO     /     01 - 02", 82, 99, 570, 25, 12,
            FontStyle.Bold, new Color(0.18f, 0.83f, 0.76f), TextAnchor.MiddleLeft);
        CreateText(canvasObject.transform, "Hero Title", "ELIGE TU\nCARRERA", 77, 125, 510, 125, 48,
            FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
        CreateText(canvasObject.transform, "Hero Copy", "Dos circuitos. Una ruta. Entra y juega.", 590, 166, 610, 52, 19,
            FontStyle.Normal, new Color(0.73f, 0.82f, 0.86f), TextAnchor.MiddleRight);

        CreateCard(canvasObject.transform, "Tank Mode Card", "01   MODO TANQUES",
            "Nueve puntos de ruta · cuatro tanques\nIA en circuito cerrado · cambia velocidad y giro",
            new Vector2(80, -275), new Color(0.045f, 0.095f, 0.112f), "ENTRAR AL CIRCUITO",
            () => Load(tankScene));
        CreateCard(canvasObject.transform, "Car Mode Card", "02   CARRERA DE AUTOS",
            "Quince puntos de carrera · cuatro autos\nWASD o flechas para conducir y completar vueltas",
            new Vector2(660, -275), new Color(0.045f, 0.095f, 0.112f), "CONDUCIR",
            () => Load(carScene));

        CreateImage(canvasObject.transform, "Footer Divider", new Vector2(80, 612), new Vector2(1120, 1), new Color(0.16f, 0.28f, 0.32f));
        CreateText(canvasObject.transform, "Lab Footer", "UNSA   /   DESARROLLO DE SOFTWARE PARA JUEGOS",
            80, 630, 570, 27, 11, FontStyle.Bold, new Color(0.54f, 0.66f, 0.71f), TextAnchor.MiddleLeft);
        CreateText(canvasObject.transform, "Version Footer", "UNITY 6000.3.17f1      /      WASD PARA CONDUCIR",
            650, 630, 550, 27, 11, FontStyle.Bold, new Color(0.54f, 0.66f, 0.71f), TextAnchor.MiddleRight);
        ApplyResponsiveLayout();
    }

    private void EnsureMenuCamera()
    {
        Camera menuCamera = GetComponentInChildren<Camera>(true);
        if (menuCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Menu Camera");
            cameraObject.transform.SetParent(transform, false);
            menuCamera = cameraObject.AddComponent<Camera>();
        }
        menuCamera.tag = "MainCamera";
        menuCamera.enabled = true;
        menuCamera.targetDisplay = 0;
        menuCamera.rect = new Rect(0f, 0f, 1f, 1f);
        menuCamera.clearFlags = CameraClearFlags.SolidColor;
        menuCamera.backgroundColor = new Color(0.018f, 0.04f, 0.06f, 1f);
        menuCamera.cullingMask = ~0;
        menuCamera.depth = -10f;
    }

    /// <summary>Keeps the complete selector visible and fills the Game view, including panoramic layouts.</summary>
    private void ApplyResponsiveLayout()
    {
        if (canvas == null) return;
        Canvas.ForceUpdateCanvases();
        RectTransform root = canvas.GetComponent<RectTransform>();
        float width = root.rect.width;
        float height = root.rect.height;
        if (width < 1f || height < 1f) return;
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        SetTopLeft(canvas.transform, "Top Accent", 0f, 0f, width, 5f);
        SetTopLeft(canvas.transform, "Header Bar", 0f, 5f, width, 62f);
        SetTopLeft(canvas.transform, "Game Category", width - 580f, 16f, 500f, 36f);
        SetTopLeft(canvas.transform, "Edition Tag", 80f, 74f, width - 160f, 24f);
        bool compact = height < 560f;
        float titleY = compact ? 96f : 125f;
        float titleHeight = compact ? 58f : 125f;
        SetTopLeft(canvas.transform, "Hero Title", 77f, titleY, Mathf.Min(510f, width * 0.46f), titleHeight);
        SetTopLeft(canvas.transform, "Hero Copy", width * 0.47f, compact ? titleY + 6f : 166f,
            width * 0.47f - 70f, compact ? 44f : 52f);
        Text title = FindText(transform, "Main Menu Canvas/Hero Title");
        if (title != null) title.fontSize = compact ? 34 : 48;
        Text copy = FindText(transform, "Main Menu Canvas/Hero Copy");
        if (copy != null) copy.fontSize = compact ? 15 : 19;

        float margin = Mathf.Clamp(width * 0.06f, 26f, 82f);
        float gap = Mathf.Clamp(width * 0.035f, 24f, 48f);
        float cardWidth = (width - margin * 2f - gap) * 0.5f;
        float cardTop = Mathf.Clamp(height * 0.42f, compact ? 158f : 245f, 290f);
        float cardHeight = Mathf.Clamp(height - cardTop - 55f, 155f, 285f);
        ResizeCard("Tank Mode Card", margin, cardTop, cardWidth, cardHeight, compact);
        ResizeCard("Car Mode Card", margin + cardWidth + gap, cardTop, cardWidth, cardHeight, compact);

        SetTopLeft(canvas.transform, "Footer Divider", margin, height - 52f, width - margin * 2f, 1f);
        SetTopLeft(canvas.transform, "Lab Footer", margin, height - 38f, width * 0.46f, 26f);
        SetTopLeft(canvas.transform, "Version Footer", width * 0.53f, height - 38f, width * 0.41f, 26f);
    }

    private void ResizeCard(string name, float x, float y, float width, float height, bool compact)
    {
        Transform card = transform.Find("Main Menu Canvas/" + name);
        if (card == null) return;
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0f, 1f);
        cardRect.pivot = new Vector2(0f, 1f);
        cardRect.anchoredPosition = new Vector2(x, -y);
        cardRect.sizeDelta = new Vector2(width, height);

        SetTopLeft(card, "Mode Title", 27f, 17f, width - 54f, 38f);
        SetTopLeft(card, "Title Divider", 27f, 62f, width - 54f, 1f);
        Text description = FindText(card, "Mode Description");
        if (description != null && compact && height < 200f)
        {
            description.text = name.Contains("Tank") ? "4 TANQUES  ·  9 WAYPOINTS" : "WASD  ·  3 RIVALES";
            description.fontSize = 12;
            SetTopLeft(card, "Mode Description", 27f, 72f, width - 54f, 24f);
        }
        else
        {
            if (description != null)
            {
                description.text = name.Contains("Tank")
                    ? "Nueve puntos de ruta · cuatro tanques\nIA en circuito cerrado · cambia velocidad y giro"
                    : "Quince puntos de carrera · cuatro autos\nWASD o flechas para conducir y completar vueltas";
                description.fontSize = compact ? 13 : 15;
            }
            SetTopLeft(card, "Mode Description", compact ? 27f : 165f, compact ? 72f : 84f,
                width - (compact ? 54f : 190f), Mathf.Max(36f, height - (compact ? 145f : 172f)));
        }
        string[] vehicleElements = { "Vehicle Shadow", "Vehicle Chassis", "Vehicle Cabin", "Vehicle Turret",
            "Vehicle Barrel", "Vehicle Track", "Vehicle Wheel Left", "Vehicle Wheel Right", "Vehicle Wing", "Vehicle Number" };
        bool showVehicle = !compact && height >= 225f;
        for (int i = 0; i < vehicleElements.Length; i++)
        {
            Transform element = card.Find(vehicleElements[i]);
            if (element != null) element.gameObject.SetActive(showVehicle);
        }
        Transform button = card.Find("Enter Mode");
        if (button != null)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(27f, -(height - (compact && height < 200f ? 55f : 61f)));
            buttonRect.sizeDelta = new Vector2(width - 54f, 46f);
            RectTransform label = button.Find("Button Label").GetComponent<RectTransform>();
            label.offsetMin = Vector2.zero;
            label.offsetMax = Vector2.zero;
        }
    }

    private static void SetTopLeft(Transform parent, string name, float x, float y, float width, float height)
    {
        Transform item = parent.Find(name);
        if (item == null) return;
        RectTransform rect = item.GetComponent<RectTransform>();
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static Text FindText(Transform parent, string path)
    {
        Transform item = parent.Find(path);
        return item == null ? null : item.GetComponent<Text>();
    }

    private void Load(string scene)
    {
        if (Application.isPlaying) SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    private static void CreateCard(Transform parent, string name, string title, string description,
        Vector2 position, Color color, string action, UnityEngine.Events.UnityAction callback)
    {
        GameObject card = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        card.transform.SetParent(parent, false);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(540, 285);
        Image cardImage = card.GetComponent<Image>();
        cardImage.color = color;
        Button cardButton = card.GetComponent<Button>();
        cardButton.targetGraphic = cardImage;
        ColorBlock cardColors = cardButton.colors;
        cardColors.normalColor = color;
        cardColors.highlightedColor = new Color(color.r + 0.025f, color.g + 0.045f, color.b + 0.05f, color.a);
        cardColors.pressedColor = new Color(color.r + 0.04f, color.g + 0.07f, color.b + 0.075f, color.a);
        cardButton.colors = cardColors;
        cardButton.onClick.AddListener(callback);
        CreateImage(card.transform, "Card Accent", Vector2.zero, new Vector2(5, 285),
            title.Contains("TANQUES") ? new Color(0.91f, 0.57f, 0.2f) : new Color(0.15f, 0.87f, 0.81f));
        CreateText(card.transform, "Mode Title", title, 27, 19, 485, 38, 19,
            FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
        CreateImage(card.transform, "Title Divider", new Vector2(27, 63), new Vector2(480, 1), new Color(0.17f, 0.31f, 0.35f));
        CreateVehicleMark(card.transform, title.Contains("TANQUES"));
        CreateText(card.transform, "Mode Description", description, 165, 84, 340, 92, 15,
            FontStyle.Normal, new Color(0.81f, 0.88f, 0.9f), TextAnchor.UpperLeft);
        CreateButton(card.transform, "Enter Mode", action, new Vector2(27, -224), callback);
    }

    private static void CreateVehicleMark(Transform parent, bool tank)
    {
        Color body = tank ? new Color(0.77f, 0.52f, 0.22f) : new Color(0.12f, 0.75f, 0.74f);
        CreateImage(parent, "Vehicle Shadow", new Vector2(33, 191), new Vector2(106, 18), new Color(0.015f, 0.025f, 0.03f));
        CreateImage(parent, "Vehicle Chassis", new Vector2(42, 143), new Vector2(88, 38), body);
        CreateImage(parent, "Vehicle Cabin", new Vector2(61, 166), new Vector2(48, 28),
            tank ? new Color(0.62f, 0.41f, 0.18f) : new Color(0.08f, 0.27f, 0.34f));
        if (tank)
        {
            CreateImage(parent, "Vehicle Turret", new Vector2(73, 179), new Vector2(40, 14), body);
            CreateImage(parent, "Vehicle Barrel", new Vector2(108, 183), new Vector2(34, 6), new Color(0.82f, 0.67f, 0.4f));
            CreateImage(parent, "Vehicle Track", new Vector2(35, 129), new Vector2(101, 9), new Color(0.19f, 0.22f, 0.19f));
        }
        else
        {
            CreateImage(parent, "Vehicle Wheel Left", new Vector2(48, 132), new Vector2(20, 14), new Color(0.018f, 0.03f, 0.035f));
            CreateImage(parent, "Vehicle Wheel Right", new Vector2(108, 132), new Vector2(20, 14), new Color(0.018f, 0.03f, 0.035f));
            CreateImage(parent, "Vehicle Wing", new Vector2(39, 177), new Vector2(94, 5), new Color(0.92f, 0.43f, 0.2f));
        }
        CreateText(parent, "Vehicle Number", tank ? "T-01" : "GT-01", 43, 203, 92, 18, 10,
            FontStyle.Bold, new Color(0.8f, 0.86f, 0.86f), TextAnchor.MiddleCenter);
    }

    private static void CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(position.x, -position.y);
        rect.sizeDelta = size;
        Image image = obj.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
    }

    private static void CreateText(Transform parent, string name, string value, float x, float y,
        float width, float height, int fontSize, FontStyle style, Color color, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        Text text = obj.GetComponent<Text>(); text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize;
        text.fontStyle = style; text.color = color; text.alignment = alignment; text.raycastTarget = false;
        if (alignment == TextAnchor.UpperLeft) text.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private static void CreateButton(Transform parent, string name, string caption,
        Vector2 position, UnityEngine.Events.UnityAction callback)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(486, 54);
        Image image = obj.GetComponent<Image>(); image.color = new Color(0.04f, 0.39f, 0.42f);
        Button button = obj.GetComponent<Button>();
        ColorBlock colors = button.colors; colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.08f, 0.56f, 0.55f);
        colors.pressedColor = new Color(0.03f, 0.25f, 0.3f); button.colors = colors;
        button.onClick.AddListener(callback);
        CreateText(obj.transform, "Button Label", caption, 0, 0, 486, 54, 15,
            FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        RectTransform labelRect = obj.transform.Find("Button Label").GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
    }
}
