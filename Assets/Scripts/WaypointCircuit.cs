using UnityEngine;

/// <summary>Optional circuit component: its child transforms are ordered waypoints.</summary>
public sealed class WaypointCircuit : MonoBehaviour
{
    [SerializeField] private bool closeCircuit = true;
    public Transform[] Waypoints
    {
        get
        {
            Transform[] points = new Transform[transform.childCount];
            for (int i = 0; i < points.Length; i++) points[i] = transform.GetChild(i);
            return points;
        }
    }

    private void OnDrawGizmos()
    {
        if (transform.childCount < 2) return;
        Gizmos.color = new Color(1f, 0.72f, 0.12f, 1f);
        int segments = closeCircuit ? transform.childCount : transform.childCount - 1;
        for (int i = 0; i < segments; i++)
            Gizmos.DrawLine(transform.GetChild(i).position,
                transform.GetChild((i + 1) % transform.childCount).position);
    }
}
