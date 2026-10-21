using UnityEngine;

/// <summary>Moves a visible tracker smoothly along the children of a WaypointCircuit.</summary>
public sealed class CircuitTracker : MonoBehaviour
{
    [SerializeField] private WaypointCircuit circuit;
    [SerializeField, Min(0.1f)] private float speed = 10f;
    [SerializeField, Min(0.1f)] private float lookAhead = 3f;
    private float distance;
    private float circuitLength;
    private Vector3[] samples;

    public Vector3 TargetPosition { get; private set; }

    public void Configure(WaypointCircuit route, float trackerSpeed = 10f, float ahead = 3f)
    {
        circuit = route;
        speed = Mathf.Max(0.1f, trackerSpeed);
        lookAhead = Mathf.Max(0.1f, ahead);
        RebuildSamples();
    }

    private void Awake() => RebuildSamples();

    private void Update()
    {
        if (samples == null || samples.Length < 2 || circuitLength <= 0f) return;
        distance = Mathf.Repeat(distance + speed * Time.deltaTime, circuitLength);
        TargetPosition = PositionAt(distance);
        transform.position = TargetPosition;
        Vector3 tangent = PositionAt(Mathf.Repeat(distance + lookAhead, circuitLength)) - TargetPosition;
        if (tangent.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
    }

    private void RebuildSamples()
    {
        if (circuit == null) circuit = FindFirstObjectByType<WaypointCircuit>();
        if (circuit == null) return;
        Transform[] points = circuit.Waypoints;
        if (points.Length < 2) return;
        const int stepsPerSegment = 24;
        int segmentCount = points.Length;
        samples = new Vector3[segmentCount * stepsPerSegment + 1];
        circuitLength = 0f;
        for (int i = 0; i <= segmentCount * stepsPerSegment; i++)
        {
            float segmentT = (float)i / stepsPerSegment;
            int segment = Mathf.Min(Mathf.FloorToInt(segmentT), segmentCount - 1);
            float t = segmentT - segment;
            Vector3 p0 = points[(segment - 1 + segmentCount) % segmentCount].position;
            Vector3 p1 = points[segment % segmentCount].position;
            Vector3 p2 = points[(segment + 1) % segmentCount].position;
            Vector3 p3 = points[(segment + 2) % segmentCount].position;
            samples[i] = CatmullRom(p0, p1, p2, p3, t);
            if (i > 0) circuitLength += Vector3.Distance(samples[i - 1], samples[i]);
        }
        TargetPosition = samples[0];
    }

    private Vector3 PositionAt(float d)
    {
        float walked = 0f;
        for (int i = 1; i < samples.Length; i++)
        {
            float length = Vector3.Distance(samples[i - 1], samples[i]);
            if (walked + length >= d && length > 0f)
                return Vector3.Lerp(samples[i - 1], samples[i], (d - walked) / length);
            walked += length;
        }
        return samples[0];
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f*p0 - 5f*p1 + 4f*p2 - p3) * t2 + (-p0 + 3f*p1 - 3f*p2 + p3) * t3);
    }
}
