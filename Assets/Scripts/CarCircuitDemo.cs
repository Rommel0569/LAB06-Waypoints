using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a complete coastal racing scene from Unity primitives and drives four cars
/// around a closed waypoint circuit. Press T (or use the on-screen button) to switch
/// between direct waypoint following and the moving-tracker method.
/// </summary>
[ExecuteAlways]
public sealed class CarCircuitDemo : MonoBehaviour
{
    [SerializeField] private int carCount = 4;
    [SerializeField, Min(0.1f)] private float trackWidth = 10f;
    [SerializeField]
    private List<FollowWP> waypointDrivers = new List<FollowWP>();
    private List<FollowTracker> trackerDrivers = new List<FollowTracker>();
    private CircuitTracker tracker;
    private bool trackerMode;
    private Transform generatedRoot;
    private bool buildingPreview;
    private LabNavigationPanel navigationPanel;
    private PlayerCarController playerCar;

    public bool TrackerMode => trackerMode;

    public bool HasScenePreview => generatedRoot != null;
    public void SetNavigationPanel(LabNavigationPanel panel) { navigationPanel = panel; }
    public static void CreateEditModePreview(GameObject root)
    {
        if (root == null) return;
        CarCircuitDemo demo = root.GetComponent<CarCircuitDemo>();
        if (demo == null) demo = root.AddComponent<CarCircuitDemo>();
        demo.EnsureScenePreview();
    }
    public void EditorSavePreview()
    {
#if UNITY_EDITOR
        EnsureScenePreview();
        CacheGeneratedObjects();
#endif
    }

    private static readonly Vector3[] CircuitPoints =
    {
        new Vector3(-31f, 0f, -14f), new Vector3(-28f, 0f, -22f),
        new Vector3(-19f, 0f, -26f), new Vector3(-5f, 0f, -27f),
        new Vector3(11f, 0f, -25f), new Vector3(25f, 0f, -20f),
        new Vector3(31f, 0f, -11f), new Vector3(32f, 0f, 2f),
        new Vector3(28f, 0f, 15f), new Vector3(19f, 0f, 22f),
        new Vector3(5f, 0f, 25f), new Vector3(-10f, 0f, 24f),
        new Vector3(-24f, 0f, 20f), new Vector3(-31f, 0f, 12f),
        new Vector3(-33f, 0f, 1f)
    };

    private static readonly Color[] TeamColors =
    {
        new Color(0.08f, 0.34f, 0.96f), new Color(0.92f, 0.12f, 0.08f),
        new Color(1f, 0.72f, 0.04f), new Color(0.08f, 0.62f, 0.2f)
    };

    private static Transform activeBuildRoot;

    private void OnEnable()
    {
        if (!Application.isPlaying) EnsureScenePreview();
    }

    private void Awake()
    {
        if (!Application.isPlaying) return;
        EnsureScenePreview();
        CacheGeneratedObjects();
        SetTrackerMode(false);
    }

    private void EnsureScenePreview()
    {
        if (buildingPreview) return;
        Transform existing = transform.Find("CAR CIRCUIT PREVIEW");
        if (existing != null)
        {
            generatedRoot = existing;
            CacheGeneratedObjects();
            return;
        }
        buildingPreview = true;
        BuildCircuit();
        buildingPreview = false;
        CacheGeneratedObjects();
    }

    private void CacheGeneratedObjects()
    {
        if (generatedRoot == null) generatedRoot = transform.Find("CAR CIRCUIT PREVIEW");
        if (generatedRoot == null) return;
        WaypointCircuit route = generatedRoot.GetComponentInChildren<WaypointCircuit>(true);
        tracker = generatedRoot.GetComponentInChildren<CircuitTracker>(true);
        waypointDrivers = new List<FollowWP>(generatedRoot.GetComponentsInChildren<FollowWP>(true));
        trackerDrivers = new List<FollowTracker>(generatedRoot.GetComponentsInChildren<FollowTracker>(true));
        playerCar = generatedRoot.GetComponentInChildren<PlayerCarController>(true);
        if (route != null && tracker != null) tracker.Configure(route, 10f, 3.5f);
        if (route != null) ConfigureExistingRacers(route.Waypoints);
        if (navigationPanel == null) navigationPanel = GetComponent<LabNavigationPanel>();
        if (navigationPanel == null) navigationPanel = gameObject.AddComponent<LabNavigationPanel>();
        if (navigationPanel != null) navigationPanel.ConfigureCarDemo(this);
        if (Application.isPlaying && playerCar != null)
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                VehicleCameraNavigator navigator = camera.GetComponent<VehicleCameraNavigator>();
                if (navigator != null) navigator.Focus(playerCar.transform);
            }
        }
    }

    private void ConfigureExistingRacers(Transform[] points)
    {
        if (points == null || points.Length < 3) return;
        string[] names = { "Azure Racer", "Coral Racer", "Sunset Racer", "Palm Racer" };
        float[] offsets = { -3.45f, -1.15f, 1.15f, 3.45f };
        float[] speeds = { 7f, 7.4f, 7f, 6.7f };
        for (int i = 0; i < names.Length; i++)
        {
            Transform car = generatedRoot.Find(names[i]);
            if (car == null) continue;
            FollowWP waypoint = car.GetComponent<FollowWP>();
            Rigidbody body = car.GetComponent<Rigidbody>();
            if (waypoint == null || body == null) continue;

            int startAt = i == 0 ? 0 : (i * points.Length) / 4;
            int next = (startAt + 1) % points.Length;
            Vector3 tangent = points[next].position - points[startAt].position;
            tangent.y = 0f;
            if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.forward;
            tangent.Normalize();
            Vector3 side = new Vector3(-tangent.z, 0f, tangent.x);
            car.position = points[startAt].position + side * offsets[i] + Vector3.up * 0.05f;
            car.rotation = Quaternion.LookRotation(tangent, Vector3.up);
            waypoint.Configure(points, speeds[i], next, offsets[i]);

            FollowTracker follower = car.GetComponent<FollowTracker>();
            if (follower != null)
            {
                follower.Configure(tracker, speeds[i] + 1.2f, 2.2f + i * 2.4f);
                follower.enabled = false;
            }

            if (i == 0)
            {
                waypoint.enabled = false;
                if (playerCar == null) playerCar = car.GetComponent<PlayerCarController>();
                if (playerCar == null) playerCar = car.gameObject.AddComponent<PlayerCarController>();
                playerCar.Configure(body, car.GetComponentsInChildren<WheelCollider>(true));
                playerCar.enabled = true;
            }
            else waypoint.enabled = true;
        }
    }

    [ContextMenu("Rebuild visible circuit preview")]
    private void RebuildVisibleCircuitPreview()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;
        Transform existing = transform.Find("CAR CIRCUIT PREVIEW");
        if (existing != null) DestroyImmediate(existing.gameObject);
        generatedRoot = null;
        EnsureScenePreview();
#endif
    }

    private void BuildCircuit()
    {
        GameObject preview = new GameObject("CAR CIRCUIT PREVIEW");
        preview.transform.SetParent(transform, false);
        generatedRoot = preview.transform;
        activeBuildRoot = generatedRoot;

        Material grass = MakeMaterial("Tropical grass", new Color(0.19f, 0.39f, 0.2f));
        Material sand = MakeMaterial("Warm beach sand", new Color(0.83f, 0.69f, 0.43f));
        Material water = MakeMaterial("Coastal water", new Color(0.05f, 0.48f, 0.63f));
        Material asphalt = MakeMaterial("Race asphalt", new Color(0.15f, 0.17f, 0.19f));
        Material white = MakeMaterial("Road markings", new Color(0.94f, 0.94f, 0.88f));
        Material red = MakeMaterial("Kerb red", new Color(0.85f, 0.08f, 0.06f));
        Material barrier = MakeMaterial("Safety barriers", new Color(0.88f, 0.9f, 0.88f));

        CreatePrimitive("Open Pacific", PrimitiveType.Plane, new Vector3(0f, -1.3f, 0f),
            new Vector3(24f, 1f, 18f), water);
        CreatePrimitive("Sandy island", PrimitiveType.Plane, new Vector3(0f, -0.65f, 0f),
            new Vector3(11f, 1f, 8f), sand);
        CreatePrimitive("Tropical infield", PrimitiveType.Plane, new Vector3(0f, -0.2f, 0f),
            new Vector3(8.5f, 1f, 6.2f), grass);

        List<Vector3> centerline = SampleCircuit(12);
        CreateRibbon("Asphalt circuit", centerline, trackWidth, asphalt, 0.04f);
        CreateRibbon("Grass safety runoff", centerline, trackWidth + 5.6f, grass, -0.08f);
        CreateRibbon("Inside white line", centerline, 0.18f, white, 0.075f, true, -(trackWidth * 0.5f - 0.35f));
        CreateRibbon("Outside white line", centerline, 0.18f, white, 0.075f, true, trackWidth * 0.5f - 0.35f);
        AddKerbs(centerline, trackWidth, red, white);
        AddBarriers(centerline, trackWidth, barrier);
        AddPalmGroves(grass);
        AddStartFinish(centerline, white, asphalt);

        GameObject routeObject = new GameObject("Race Waypoint Circuit (15 ordered points)");
        routeObject.transform.SetParent(generatedRoot, false);
        WaypointCircuit route = routeObject.AddComponent<WaypointCircuit>();
        Transform[] points = new Transform[CircuitPoints.Length];
        for (int i = 0; i < CircuitPoints.Length; i++)
        {
            GameObject point = new GameObject("Waypoint " + (i + 1).ToString("00"));
            point.transform.SetParent(routeObject.transform, false);
            point.transform.position = CircuitPoints[i] + Vector3.up * 0.12f;
            points[i] = point.transform;
            CreatePrimitive("WP beacon " + (i + 1), PrimitiveType.Sphere,
                CircuitPoints[i] + Vector3.up * 0.65f, Vector3.one * 0.38f,
                MakeMaterial("Waypoint beacon " + i, new Color(1f, 0.72f, 0.05f), true));
        }
        route.Configure(points, true);

        GameObject trackerObject = CreatePrimitive("Circuit tracker", PrimitiveType.Sphere,
            CircuitPoints[0] + Vector3.up * 0.8f, Vector3.one * 0.85f,
            MakeMaterial("Tracker cyan", new Color(0f, 0.9f, 1f), true));
        tracker = trackerObject.AddComponent<CircuitTracker>();
        tracker.Configure(route, 10f, 3.5f);
        tracker.enabled = false;

        SpawnCars(points);
        CreateLightingAndCamera();
        CreatePaddock();
        CreateStartGate(centerline, white);
        navigationPanel = GetComponent<LabNavigationPanel>();
        if (navigationPanel != null) navigationPanel.ConfigureCarDemo(this);
        activeBuildRoot = null;
    }

    private void SpawnCars(Transform[] points)
    {
        int count = Mathf.Clamp(carCount, 2, 4);
        // Player starts alone at the start line; AI cars keep physical separation at launch.
        float[] laneOffsets = { -3.45f, -1.15f, 1.15f, 3.45f };
        float[] speeds = { 0f, 7.4f, 7.0f, 6.7f };
        for (int i = 0; i < count; i++)
        {
            int startAt = i == 0 ? 0 : (i * points.Length) / count;
            Vector3 tangent = points[(startAt + 1) % points.Length].position - points[startAt].position;
            tangent.y = 0f;
            Vector3 side = new Vector3(-tangent.normalized.z, 0f, tangent.normalized.x);
            Vector3 start = points[startAt].position + side * laneOffsets[i] + Vector3.up * 0.05f;

            GameObject car = new GameObject(new[] { "Azure", "Coral", "Sunset", "Palm" }[i] + " Racer");
            car.transform.SetParent(generatedRoot, true);
            car.transform.position = start;
            car.transform.rotation = Quaternion.LookRotation(tangent.normalized, Vector3.up);
            BuildCarModel(car.transform, TeamColors[i]);

            BoxCollider chassis = car.AddComponent<BoxCollider>();
            chassis.center = new Vector3(0f, 0.58f, 0f);
            chassis.size = new Vector3(2.25f, 0.72f, 3.65f);
            Rigidbody body = car.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.mass = 1100f;
            body.constraints = RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            FollowWP waypoint = car.AddComponent<FollowWP>();
            waypoint.Configure(points, i == 0 ? 7f : speeds[i], (startAt + 1) % points.Length, laneOffsets[i]);
            if (i == 0)
            {
                waypoint.enabled = false;
                playerCar = car.AddComponent<PlayerCarController>();
                playerCar.Configure(body, wheelColliders: car.GetComponentsInChildren<WheelCollider>());
            }
            waypointDrivers.Add(waypoint);

            FollowTracker follower = car.AddComponent<FollowTracker>();
            follower.Configure(tracker, i == 0 ? 0.1f : speeds[i] + 1.2f, 2.2f + i * 2.4f);
            follower.enabled = false;
            trackerDrivers.Add(follower);
        }
    }

    private void BuildCarModel(Transform parent, Color bodyColor)
    {
        Material body = MakeMaterial("Racer paint", bodyColor);
        Material glass = MakeMaterial("Dark cockpit glass", new Color(0.08f, 0.16f, 0.2f), true);
        Material rubber = MakeMaterial("Tire rubber", new Color(0.035f, 0.04f, 0.045f));
        Material trim = MakeMaterial("Car trim", new Color(0.86f, 0.9f, 0.91f));

        CreateChild(PrimitiveType.Cube, "Low racing chassis", parent, new Vector3(0f, 0.54f, 0f),
            new Vector3(2.15f, 0.5f, 3.55f), body);
        CreateChild(PrimitiveType.Cube, "Front hood", parent, new Vector3(0f, 0.83f, 0.95f),
            new Vector3(1.7f, 0.22f, 1.15f), body);
        CreateChild(PrimitiveType.Cube, "Cockpit", parent, new Vector3(0f, 1.06f, -0.2f),
            new Vector3(1.25f, 0.58f, 1.42f), glass);
        CreateChild(PrimitiveType.Cube, "Front bumper", parent, new Vector3(0f, 0.43f, 1.84f),
            new Vector3(2.25f, 0.18f, 0.22f), trim);
        CreateChild(PrimitiveType.Cube, "Rear spoiler", parent, new Vector3(0f, 1.05f, -1.62f),
            new Vector3(2.1f, 0.12f, 0.28f), trim);
        for (int side = -1; side <= 1; side += 2)
        for (int axle = -1; axle <= 1; axle += 2)
        {
            GameObject wheel = CreateChild(PrimitiveType.Cylinder, "Tire", parent,
                new Vector3(side * 1.05f, 0.38f, axle * 1.08f), new Vector3(0.32f, 0.34f, 0.34f), rubber, true);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Collider visualCollider = wheel.GetComponent<Collider>();
            if (visualCollider != null) visualCollider.enabled = false;
            GameObject wheelColliderObject = new GameObject("Wheel Collider");
            wheelColliderObject.transform.SetParent(parent, false);
            wheelColliderObject.transform.localPosition = new Vector3(side * 0.92f, 0.37f, axle * 1.12f);
            WheelCollider wheelCollider = wheelColliderObject.AddComponent<WheelCollider>();
            wheelCollider.radius = 0.36f;
            wheelCollider.suspensionDistance = 0.12f;
        }
    }

    private void AddKerbs(List<Vector3> path, float width, Material red, Material white)
    {
        int stride = 3;
        for (int i = 0; i < path.Count; i += stride)
        {
            Vector3 tangent = TangentAt(path, i);
            Vector3 side = new Vector3(-tangent.z, 0f, tangent.x);
            Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);
            for (int edge = -1; edge <= 1; edge += 2)
            {
                Vector3 position = path[i] + side * (edge * (width * 0.5f + 0.45f));
                position.y = 0.12f;
                CreatePrimitive("Alternating red-white kerb", PrimitiveType.Cube, position,
                    new Vector3(1.15f, 0.22f, 1.05f), (i / stride) % 2 == 0 ? red : white, true).transform.rotation = rotation;
            }
        }
    }

    private void AddBarriers(List<Vector3> path, float width, Material material)
    {
        for (int i = 0; i < path.Count; i += 8)
        {
            Vector3 tangent = TangentAt(path, i);
            Vector3 side = new Vector3(-tangent.z, 0f, tangent.x);
            Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);
            for (int edge = -1; edge <= 1; edge += 2)
            {
                Vector3 position = path[i] + side * (edge * (width * 0.5f + 2.1f));
                position.y = 0.62f;
                CreatePrimitive("Track safety barrier", PrimitiveType.Cube, position,
                    new Vector3(0.55f, 1.15f, 5.4f), material, true).transform.rotation = rotation;
            }
        }
    }

    private void AddPalmGroves(Material grass)
    {
        for (int i = 0; i < 14; i++)
        {
            float angle = i * Mathf.PI * 2f / 14f;
            Vector3 basePoint = new Vector3(Mathf.Cos(angle) * 42f, 0f, Mathf.Sin(angle) * 32f);
            GameObject trunk = CreatePrimitive("Palm trunk", PrimitiveType.Cylinder,
                basePoint + Vector3.up * 2f, new Vector3(0.48f, 2f, 0.48f),
                MakeMaterial("Palm bark", new Color(0.35f, 0.22f, 0.1f)));
            trunk.transform.rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
            Vector3 crown = basePoint + Vector3.up * 4.15f;
            CreatePrimitive("Palm heart", PrimitiveType.Sphere, crown, new Vector3(0.65f, 0.5f, 0.65f),
                MakeMaterial("Palm heart green", new Color(0.16f, 0.42f, 0.12f)));
            for (int frond = 0; frond < 8; frond++)
            {
                float yaw = frond * 45f + angle * Mathf.Rad2Deg;
                Vector3 radial = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                GameObject leaf = CreatePrimitive("Palm frond", PrimitiveType.Sphere,
                    crown + radial * 1.45f + Vector3.down * 0.22f,
                    new Vector3(0.38f, 0.22f, 2.8f), grass);
                leaf.transform.rotation = Quaternion.LookRotation(radial + Vector3.down * 0.35f, Vector3.up);
            }
        }
    }

    private void AddStartFinish(List<Vector3> path, Material white, Material asphalt)
    {
        Vector3 tangent = TangentAt(path, 0);
        Vector3 side = new Vector3(-tangent.z, 0f, tangent.x);
        Vector3 start = path[0];
        for (int i = -4; i < 4; i++)
        {
            Vector3 position = start + side * (i * 1.25f);
            position.y = 0.12f;
            Material tile = i % 2 == 0 ? white : asphalt;
            CreatePrimitive("Start finish checker", PrimitiveType.Cube, position,
                new Vector3(1.22f, 0.04f, 1.1f), tile).transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
        }
    }

    private void CreateLightingAndCamera()
    {
        GameObject lightObject = new GameObject("Tropical afternoon sun");
        lightObject.transform.SetParent(generatedRoot, true);
        Light sunlight = lightObject.AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.intensity = 1.25f;
        sunlight.color = new Color(1f, 0.91f, 0.75f);
        lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

        GameObject cameraObject = new GameObject("Race Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(generatedRoot, true);
        cameraObject.AddComponent<AudioListener>();
        camera.orthographic = true;
        camera.orthographicSize = 48f;
        camera.fieldOfView = 47f;
        camera.transform.position = new Vector3(49f, 56f, 62f);
        camera.transform.LookAt(new Vector3(0f, 0f, 0f));
        cameraObject.AddComponent<VehicleCameraNavigator>();
    }

    public void SetTrackerMode(bool enabled)
    {
        trackerMode = enabled;
        if (tracker != null)
        {
            tracker.gameObject.SetActive(enabled);
            tracker.enabled = enabled;
        }
        for (int i = 0; i < waypointDrivers.Count; i++)
        {
            FollowWP driver = waypointDrivers[i];
            if (driver != null) driver.enabled = !enabled && driver.GetComponent<PlayerCarController>() == null;
        }
        for (int i = 0; i < trackerDrivers.Count; i++)
        {
            FollowTracker driver = trackerDrivers[i];
            if (driver != null) driver.enabled = enabled && driver.GetComponent<PlayerCarController>() == null;
        }
        if (playerCar != null) playerCar.enabled = true;
    }

    public void ResetRace()
    {
        SceneManagerReload();
    }

    private void SceneManagerReload()
    {
        // Restarting the active scene resets both the tracker and all cars deterministically.
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void OnGUI()
    {
        // Controls are centralized in LabNavigationPanel in both laboratory scenes.
    }

    private void OnDrawGizmos()
    {
        if (CircuitPoints == null || CircuitPoints.Length < 2) return;
        Gizmos.color = new Color(1f, 0.62f, 0.06f, 1f);
        for (int i = 0; i < CircuitPoints.Length; i++)
            Gizmos.DrawLine(CircuitPoints[i], CircuitPoints[(i + 1) % CircuitPoints.Length]);
    }

    private List<Vector3> SampleCircuit(int stepsPerSegment)
    {
        List<Vector3> points = new List<Vector3>(CircuitPoints.Length * stepsPerSegment);
        for (int segment = 0; segment < CircuitPoints.Length; segment++)
        {
            Vector3 p0 = CircuitPoints[(segment - 1 + CircuitPoints.Length) % CircuitPoints.Length];
            Vector3 p1 = CircuitPoints[segment];
            Vector3 p2 = CircuitPoints[(segment + 1) % CircuitPoints.Length];
            Vector3 p3 = CircuitPoints[(segment + 2) % CircuitPoints.Length];
            for (int step = 0; step < stepsPerSegment; step++)
                points.Add(CatmullRom(p0, p1, p2, p3, step / (float)stepsPerSegment));
        }
        return points;
    }

    private void CreateRibbon(string name, List<Vector3> path, float width, Material material,
        float height, bool line = false, float offset = 0f)
    {
        int count = path.Count;
        Vector3[] vertices = new Vector3[count * 2];
        Vector2[] uv = new Vector2[count * 2];
        int[] triangles = new int[count * 6];
        for (int i = 0; i < count; i++)
        {
            Vector3 tangent = TangentAt(path, i);
            Vector3 side = new Vector3(-tangent.z, 0f, tangent.x);
            float halfWidth = width * 0.5f;
            Vector3 center = path[i] + Vector3.up * height;
            if (line) center += side * offset;
            vertices[i * 2] = center - side * halfWidth;
            vertices[i * 2 + 1] = center + side * halfWidth;
            uv[i * 2] = new Vector2(0f, i / 5f);
            uv[i * 2 + 1] = new Vector2(1f, i / 5f);
            int next = (i + 1) % count;
            int ti = i * 6;
            triangles[ti] = i * 2; triangles[ti + 1] = i * 2 + 1; triangles[ti + 2] = next * 2;
            triangles[ti + 3] = i * 2 + 1; triangles[ti + 4] = next * 2 + 1; triangles[ti + 5] = next * 2;
        }
        Mesh mesh = CreatePersistentRibbonMesh(name, vertices, uv, triangles);
        GameObject road = new GameObject(name);
        if (activeBuildRoot != null) road.transform.SetParent(activeBuildRoot, true);
        MeshFilter filter = road.AddComponent<MeshFilter>(); filter.sharedMesh = mesh;
        MeshRenderer renderer = road.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
        if (name == "Asphalt circuit" || name == "Grass safety runoff")
        {
            MeshCollider collision = road.AddComponent<MeshCollider>();
            collision.sharedMesh = mesh;
        }
    }

    private static Vector3 TangentAt(List<Vector3> points, int index)
    {
        Vector3 tangent = points[(index + 1) % points.Count] - points[(index - 1 + points.Count) % points.Count];
        tangent.y = 0f;
        return tangent.sqrMagnitude > 0.0001f ? tangent.normalized : Vector3.forward;
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * (2f * p1 + (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    private static Material MakeMaterial(string name, Color color, bool emission = false)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EnsureGeneratedAssetFolder();
            string colorKey = ColorUtility.ToHtmlStringRGBA(color);
            string fileName = "LAB06_" + SafeAssetName(name) + "_" + colorKey + ".mat";
            string path = "Assets/LAB06Generated/" + fileName;
            Material persistent = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (persistent == null)
            {
                persistent = new Material(shader) { name = name, color = color };
                UnityEditor.AssetDatabase.CreateAsset(persistent, path);
            }
            persistent.shader = shader;
            persistent.color = color;
            if (emission)
            {
                persistent.EnableKeyword("_EMISSION");
                persistent.SetColor("_EmissionColor", color * 0.35f);
            }
            else
            {
                persistent.DisableKeyword("_EMISSION");
            }
            UnityEditor.EditorUtility.SetDirty(persistent);
            return persistent;
        }
#endif
        Material material = new Material(shader) { name = name, color = color };
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.35f);
        }
        return material;
    }

    private static Mesh CreatePersistentRibbonMesh(string name, Vector3[] vertices, Vector2[] uv, int[] triangles)
    {
        Mesh mesh;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EnsureGeneratedAssetFolder();
            string path = "Assets/LAB06Generated/LAB06_" + SafeAssetName(name) + ".asset";
            mesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = name };
                UnityEditor.AssetDatabase.CreateAsset(mesh, path);
            }
            else
            {
                mesh.Clear();
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            UnityEditor.EditorUtility.SetDirty(mesh);
            return mesh;
        }
#endif
        mesh = new Mesh { name = name };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

#if UNITY_EDITOR
    private static void EnsureGeneratedAssetFolder()
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/LAB06Generated"))
            UnityEditor.AssetDatabase.CreateFolder("Assets", "LAB06Generated");
    }
#endif

    private static string SafeAssetName(string value)
    {
        return System.Text.RegularExpressions.Regex.Replace(value, "[^a-zA-Z0-9]", string.Empty);
    }

    private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material, bool collision = false)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = name; item.transform.position = position; item.transform.localScale = scale;
        if (activeBuildRoot != null) item.transform.SetParent(activeBuildRoot, true);
        Renderer renderer = item.GetComponent<Renderer>(); if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = item.GetComponent<Collider>(); if (collider != null) collider.enabled = collision;
        return item;
    }

    private static GameObject CreateChild(PrimitiveType type, string name, Transform parent,
        Vector3 localPosition, Vector3 localScale, Material material, bool collision = false)
    {
        GameObject child = GameObject.CreatePrimitive(type);
        child.name = name; child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition; child.transform.localScale = localScale;
        Renderer renderer = child.GetComponent<Renderer>(); if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = child.GetComponent<Collider>(); if (collider != null) collider.enabled = collision;
        return child;
    }

    private void CreatePaddock()
    {
        Material wall = MakeMaterial("Pit garage white", new Color(0.78f, 0.82f, 0.8f));
        Material roof = MakeMaterial("Pit garage teal", new Color(0.04f, 0.28f, 0.34f));
        Material stand = MakeMaterial("Grandstand seats", new Color(0.82f, 0.27f, 0.08f));
        for (int bay = 0; bay < 4; bay++)
        {
            Vector3 p = new Vector3(-8f + bay * 4f, 1.1f, -34f);
            CreatePrimitive("Pit garage bay " + (bay + 1), PrimitiveType.Cube, p,
                new Vector3(3.8f, 2.2f, 5f), wall);
            CreatePrimitive("Pit garage roof " + (bay + 1), PrimitiveType.Cube,
                p + Vector3.up * 1.3f, new Vector3(4.1f, 0.35f, 5.3f), roof);
        }
        for (int row = 0; row < 4; row++)
        {
            Vector3 p = new Vector3(27f, 0.6f + row * 0.72f, 0f);
            CreatePrimitive("Spectator grandstand row " + (row + 1), PrimitiveType.Cube,
                p, new Vector3(5f, 0.48f, 17f - row * 1.5f), stand);
        }
        CreatePrimitive("Tropical circuit sign", PrimitiveType.Cube,
            new Vector3(0f, 3.5f, -34f), new Vector3(13f, 2.1f, 0.45f), roof);
    }

    private void CreateStartGate(List<Vector3> path, Material material)
    {
        Vector3 tangent = TangentAt(path, 0);
        GameObject gate = CreatePrimitive("Start Finish Lap Trigger", PrimitiveType.Cube,
            path[0] + Vector3.up, new Vector3(trackWidth - 0.6f, 2f, 0.35f), material, true);
        gate.transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
        BoxCollider trigger = gate.GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        Rigidbody triggerBody = gate.AddComponent<Rigidbody>();
        triggerBody.isKinematic = true;
        triggerBody.useGravity = false;
        gate.AddComponent<RaceLapCounter>();
        CreatePrimitive("Start finish arch", PrimitiveType.Cube,
            path[0] + Vector3.up * 3.5f, new Vector3(trackWidth + 0.8f, 0.22f, 0.3f),
            MakeMaterial("Finish arch teal", new Color(0.02f, 0.35f, 0.42f)), true)
            .transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
    }
}
