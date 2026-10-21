using UnityEngine;

/// <summary>Drives a tank around an ordered, closed list of waypoint transforms.</summary>
public sealed class FollowWP : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Min(0.1f)] private float moveSpeed = 8f;
    [SerializeField, Min(1f)] private float turnSpeed = 120f;
    [SerializeField, Min(0.05f)] private float arrivalDistance = 1.25f;
    [SerializeField] private bool drawRoute = true;

    private int currentWaypoint;

    public int CurrentWaypoint => currentWaypoint;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = Mathf.Max(0.1f, value); }

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
            Debug.LogError($"{name}: asigna al menos un waypoint en FollowWP.", this);
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (waypoints[currentWaypoint] == null)
        {
            Debug.LogError($"{name}: el waypoint {currentWaypoint} no está asignado.", this);
            enabled = false;
            return;
        }

        Vector3 target = waypoints[currentWaypoint].position;
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        float alignment = Mathf.Clamp01(Vector3.Dot(transform.forward, direction.normalized));
        transform.position += transform.forward * (moveSpeed * alignment * Time.deltaTime);
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
