using UnityEngine;

/// <summary>Steers a tank toward the moving tracker while maintaining a trailing distance.</summary>
public sealed class FollowTracker : MonoBehaviour
{
    [SerializeField] private CircuitTracker tracker;
    [SerializeField, Min(0.1f)] private float speed = 9f;
    [SerializeField, Min(0f)] private float followDistance = 2.5f;
    [SerializeField, Min(1f)] private float turnSpeed = 120f;
    private Rigidbody body;

    public void Configure(CircuitTracker target, float followSpeed = 9f, float distance = 2.5f)
    {
        tracker = target;
        speed = Mathf.Max(0.1f, followSpeed);
        followDistance = Mathf.Max(0f, distance);
    }

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        if (tracker == null) tracker = FindFirstObjectByType<CircuitTracker>();
        if (tracker == null) Debug.LogError($"{name}: falta asignar el CircuitTracker.", this);
    }

    private void Update()
    {
        if (Application.isPlaying && body == null) body = GetComponent<Rigidbody>();
        if (body == null) MoveTowardsTracker(Time.deltaTime, false);
    }

    private void FixedUpdate()
    {
        if (body != null) MoveTowardsTracker(Time.fixedDeltaTime, true);
    }

    private void MoveTowardsTracker(float deltaTime, bool usePhysics)
    {
        if (tracker == null) return;
        Vector3 offset = tracker.TargetPosition - transform.position;
        offset.y = 0f;
        if (offset.magnitude <= followDistance) return;
        Quaternion targetRotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
        Quaternion nextRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * deltaTime);
        if (usePhysics) body.MoveRotation(nextRotation);
        else transform.rotation = nextRotation;
        float alignment = Mathf.Clamp01(Vector3.Dot(transform.forward, offset.normalized));
        Vector3 nextPosition = transform.position + transform.forward * speed * alignment * deltaTime;
        if (usePhysics) body.MovePosition(nextPosition);
        else transform.position = nextPosition;
        if (usePhysics)
        {
            Vector3 position = body.position;
            body.position = new Vector3(position.x, 0.05f, position.z);
        }
    }
}
