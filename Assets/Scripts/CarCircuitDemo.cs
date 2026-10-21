using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a complete coastal racing scene from Unity primitives and drives four cars
/// around a closed waypoint circuit. Press T (or use the on-screen button) to switch
/// between direct waypoint following and the moving-tracker method.
/// </summary>
public sealed class CarCircuitDemo : MonoBehaviour
{
    [SerializeField] private int carCount = 4;
    [SerializeField, Min(0.1f)] private float trackWidth = 10f;
    [SerializeField] private bool startInTrackerMode;

    private readonly List<FollowWP> waypointDrivers = new List<FollowWP>();
    private readonly List<FollowTracker> trackerDrivers = new List<FollowTracker>();
    private CircuitTracker tracker;
    private bool trackerMode;

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

    private void Awake()
    {
        BuildCircuit();
        SetTrackerMode(startInTrackerMode);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) SetTrackerMode(!trackerMode);
        if (Input.GetKeyDown(KeyCode.R)) ResetRace();
    }

    private void BuildCircuit()
    {
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
        CreateRibbon("Inside white line", centerline, 0.18f, white, 0.075f, true, -(trackWidth * 0.5f - 0.35f));
        CreateRibbon("Outside white line", centerline, 0.18f, white, 0.075f, true, trackWidth * 0.5f - 0.35f);
        AddKerbs(centerline, trackWidth, red, white);
        AddBarriers(centerline, trackWidth, barrier);
        AddPalmGroves(grass);
        AddStartFinish(centerline, white, asphalt);

        GameObject routeObject = new GameObject("Race Waypoint Circuit");
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
        trackerObject.SetActive(false);

        SpawnCars(points);
        CreateLightingAndCamera();
    }

    private void SpawnCars(Transform[] points)
    {
        int count = Mathf.Clamp(carCount, 2, 4);
        float[] laneOffsets = { -3f, -1f, 1f, 3f };
        float[] speeds = { 12f, 11f, 12.8f, 11.7f };
        for (int i = 0; i < count; i++)
        {
            int startAt = (i * points.Length) / count;
            Vector3 tangent = points[(startAt + 1) % points.Length].position - points[startAt].position;
            tangent.y = 0f;
            Vector3 side = new Vector3(-tangent.normalized.z, 0f, tangent.normalized.x);
            Vector3 start = points[startAt].position + side * laneOffsets[i] + Vector3.up * 0.05f;

            GameObject car = new GameObject(new[] { "Azure", "Coral", "Sunset", "Palm" }[i] + " Racer");
            car.transform.position = start;
            car.transform.rotation = Quaternion.LookRotation(tangent.normalized, Vector3.up);
            BuildCarModel(car.transform, TeamColors[i]);

            FollowWP waypoint = car.AddComponent<FollowWP>();
            waypoint.Configure(points, speeds[i], (startAt + 1) % points.Length, laneOffsets[i]);
            waypointDrivers.Add(waypoint);

            FollowTracker follower = car.AddComponent<FollowTracker>();
            follower.Configure(tracker, speeds[i] + 1.2f, 2.2f + i * 2.4f);
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
                new Vector3(side * 1.05f, 0.38f, axle * 1.08f), new Vector3(0.32f, 0.34f, 0.34f), rubber);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
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
                    new Vector3(1.15f, 0.22f, 1.05f), (i / stride) % 2 == 0 ? red : white).transform.rotation = rotation;
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
                    new Vector3(0.55f, 1.15f, 5.4f), material).transform.rotation = rotation;
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
            CreatePrimitive("Palm crown", PrimitiveType.Sphere,
                basePoint + Vector3.up * 4.1f, new Vector3(1.65f, 0.6f, 1.65f), grass);
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
        Light sunlight = lightObject.AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.intensity = 1.25f;
        sunlight.color = new Color(1f, 0.91f, 0.75f);
        lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

        GameObject cameraObject = new GameObject("Race Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.fieldOfView = 47f;
        camera.transform.position = new Vector3(49f, 56f, 62f);
        camera.transform.LookAt(new Vector3(0f, 0f, 0f));
    }

    private void SetTrackerMode(bool enabled)
    {
        trackerMode = enabled;
        if (tracker != null) tracker.gameObject.SetActive(enabled);
        foreach (FollowWP driver in waypointDrivers) if (driver != null) driver.enabled = !enabled;
        foreach (FollowTracker driver in trackerDrivers) if (driver != null) driver.enabled = enabled;
    }

    private void ResetRace()
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
        GUI.Box(new Rect(16, 16, 335, 88), "");
        GUI.Label(new Rect(30, 26, 310, 24), "COSTA AZUL  |  CIRCUITO DE CARRERAS");
        GUI.Label(new Rect(30, 50, 310, 20), trackerMode ? "Modo: tracker móvil" : "Modo: waypoints directos");
        if (GUI.Button(new Rect(30, 75, 145, 24), "Cambiar modo (T)")) SetTrackerMode(!trackerMode);
        if (GUI.Button(new Rect(185, 75, 120, 24), "Reiniciar (R)")) ResetRace();
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
        Mesh mesh = new Mesh { name = name };
        mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles;
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        GameObject road = new GameObject(name);
        MeshFilter filter = road.AddComponent<MeshFilter>(); filter.sharedMesh = mesh;
        MeshRenderer renderer = road.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
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
        Material material = new Material(shader) { name = name, color = color };
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.35f);
        }
        return material;
    }

    private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = name; item.transform.position = position; item.transform.localScale = scale;
        Renderer renderer = item.GetComponent<Renderer>(); if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = item.GetComponent<Collider>(); if (collider != null) Destroy(collider);
        return item;
    }

    private static GameObject CreateChild(PrimitiveType type, string name, Transform parent,
        Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject child = GameObject.CreatePrimitive(type);
        child.name = name; child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition; child.transform.localScale = localScale;
        Renderer renderer = child.GetComponent<Renderer>(); if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = child.GetComponent<Collider>(); if (collider != null) Destroy(collider);
        return child;
    }
}
