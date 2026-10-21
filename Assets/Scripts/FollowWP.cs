using UnityEngine;

/// <summary>Drives a tank around an ordered, closed list of waypoint transforms.</summary>
public sealed class FollowWP : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Min(0.1f)] private float moveSpeed = 8f;
    [SerializeField, Min(1f)] private float turnSpeed = 120f;
    [SerializeField, Min(0.05f)] private float arrivalDistance = 1.25f;
    [SerializeField, Min(0)] private int startingWaypoint = 0;
    [SerializeField] private float laneOffset;
    [SerializeField, Min(0.2f)] private float cornerSlowdown = 0.72f;
    [SerializeField] private bool drawRoute = true;

    private int currentWaypoint;
    private bool configuredAtRuntime;
    private Rigidbody body;

    public int CurrentWaypoint => currentWaypoint;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = Mathf.Max(0.1f, value); }

    public void Configure(Transform[] route, float speed, int startAt = 0, float offset = 0f)
    {
        waypoints = route;
        moveSpeed = Mathf.Max(0.1f, speed);
        laneOffset = offset;
        currentWaypoint = route == null || route.Length == 0 ? 0 : (startAt % route.Length + route.Length) % route.Length;
        configuredAtRuntime = true;
    }

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        if (waypoints == null || waypoints.Length == 0)
            Debug.LogError($"{name}: asigna al menos un waypoint en FollowWP.", this);
        else if (!configuredAtRuntime)
            currentWaypoint = startingWaypoint % waypoints.Length;
    }

    private void Update()
    {
        if (Application.isPlaying && body == null) body = GetComponent<Rigidbody>();
        if (body == null) MoveTowardsWaypoint(Time.deltaTime, false);
    }

    private void FixedUpdate()
    {
        if (body != null) MoveTowardsWaypoint(Time.fixedDeltaTime, true);
    }

    private void MoveTowardsWaypoint(float deltaTime, bool usePhysics)
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (waypoints[currentWaypoint] == null)
        {
            Debug.LogError($"{name}: el waypoint {currentWaypoint} no está asignado.", this);
            enabled = false;
            return;
        }

        Vector3 target = GetWaypointPosition(currentWaypoint);
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion nextRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * deltaTime);
        if (usePhysics) body.MoveRotation(nextRotation);
        else transform.rotation = nextRotation;
        float alignment = Mathf.Clamp01(Vector3.Dot(transform.forward, direction.normalized));
        // Brake before sharp bends instead of taking a straight chord across neighbouring lanes.
        Vector3 nextDirection = GetWaypointPosition((currentWaypoint + 1) % waypoints.Length) - target;
        nextDirection.y = 0f;
        float bend = nextDirection.sqrMagnitude > 0.001f
            ? Vector3.Angle(direction, nextDirection) / 180f : 0f;
        float speedFactor = Mathf.Lerp(1f, cornerSlowdown, Mathf.SmoothStep(0f, 0.72f, bend));
        float steeringFactor = Mathf.InverseLerp(-0.08f, 0.55f, alignment);
        Vector3 nextPosition = transform.position + transform.forward * (moveSpeed * speedFactor * steeringFactor * deltaTime);
        if (usePhysics) body.MovePosition(nextPosition);
        else transform.position = nextPosition;
        if (usePhysics)
        {
            Vector3 position = body.position;
            body.position = new Vector3(position.x, 0.05f, position.z);
        }
    }

    private Vector3 GetWaypointPosition(int index)
    {
        Vector3 point = waypoints[index].position;
        if (Mathf.Abs(laneOffset) < 0.001f || waypoints.Length < 3) return point;

        Vector3 before = waypoints[(index - 1 + waypoints.Length) % waypoints.Length].position;
        Vector3 after = waypoints[(index + 1) % waypoints.Length].position;
        Vector3 tangent = after - before;
        tangent.y = 0f;
        if (tangent.sqrMagnitude < 0.001f) return point;
        tangent.Normalize();
        Vector3 side = new Vector3(-tangent.z, 0f, tangent.x);
        return point + side * laneOffset;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawRoute || waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform a = waypoints[i];
            Transform b = waypoints[(i + 1) % waypoints.Length];
            if (a == null || b == null) continue;
            Gizmos.DrawSphere(a.position, 0.45f);
            Gizmos.DrawLine(a.position, b.position);
        }
    }
}
