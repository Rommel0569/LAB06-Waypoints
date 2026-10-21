using UnityEngine;

/// <summary>Steers a tank toward the moving tracker while maintaining a trailing distance.</summary>
public sealed class FollowTracker : MonoBehaviour
{
    [SerializeField] private CircuitTracker tracker;
    [SerializeField, Min(0.1f)] private float speed = 9f;
    [SerializeField, Min(0f)] private float followDistance = 2.5f;
    [SerializeField, Min(1f)] private float turnSpeed = 120f;

    private void Start()
    {
        if (tracker == null) tracker = FindObjectOfType<CircuitTracker>();
        if (tracker == null) Debug.LogError($"{name}: falta asignar el CircuitTracker.", this);
    }

    private void Update()
    {
        if (tracker == null) return;
        Vector3 offset = tracker.TargetPosition - transform.position;
        offset.y = 0f;
        if (offset.magnitude <= followDistance) return;
        Quaternion targetRotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        float alignment = Mathf.Clamp01(Vector3.Dot(transform.forward, offset.normalized));
        transform.position += transform.forward * speed * alignment * Time.deltaTime;
    }
}
