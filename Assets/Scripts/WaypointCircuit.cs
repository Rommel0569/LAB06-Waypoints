using UnityEngine;

/// <summary>Optional circuit component: its child transforms are ordered waypoints.</summary>
public sealed class WaypointCircuit : MonoBehaviour
{
    [SerializeField] private bool closeCircuit = true;
    [SerializeField] private Transform[] explicitWaypoints;
    public Transform[] Waypoints
    {
        get
        {
            if (explicitWaypoints != null && explicitWaypoints.Length > 0) return explicitWaypoints;
            Transform[] points = new Transform[transform.childCount];
            for (int i = 0; i < points.Length; i++) points[i] = transform.GetChild(i);
            return points;
        }
    }

    private void OnDrawGizmos()
    {
        Transform[] points = Waypoints;
        if (points.Length < 2) return;
        Gizmos.color = new Color(1f, 0.72f, 0.12f, 1f);
        int segments = closeCircuit ? points.Length : points.Length - 1;
        for (int i = 0; i < segments; i++)
            Gizmos.DrawLine(points[i].position,
                points[(i + 1) % points.Length].position);
    }

    public void Configure(Transform[] points, bool closed = true)
    {
        explicitWaypoints = points;
        closeCircuit = closed;
    }
}
