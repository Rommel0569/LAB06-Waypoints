using UnityEngine;

/// <summary>Moves the scene camera between vehicles selected in the laboratory panel.</summary>
public sealed class VehicleCameraNavigator : MonoBehaviour
{
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 6f, -11f);
    [SerializeField, Min(0.1f)] private float smoothTime = 0.28f;
    private Transform target;
    private Vector3 velocity;

    public void Focus(Transform newTarget)
    {
        target = newTarget;
        velocity = Vector3.zero;
    }

    public void ShowOverview()
    {
        target = null;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + target.rotation * followOffset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        Vector3 lookPoint = target.position + Vector3.up * 1.2f;
        Vector3 lookDirection = lookPoint - transform.position;
        if (lookDirection.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }
}
