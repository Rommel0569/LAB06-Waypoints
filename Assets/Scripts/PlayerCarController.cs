using System;
using UnityEngine;

/// <summary>Keyboard-controlled rear-wheel-drive car for the Costa Azul race.</summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerCarController : MonoBehaviour
{
    [SerializeField, Min(1f)] private float acceleration = 17f;
    [SerializeField, Min(1f)] private float reverseAcceleration = 8f;
    [SerializeField, Min(1f)] private float maxSpeed = 22f;
    [SerializeField, Min(1f)] private float brakingStrength = 28f;
    [SerializeField, Min(1f)] private float steerDegreesPerSecond = 95f;
    [SerializeField, Range(0f, 1f)] private float grip = 0.92f;

    private Rigidbody body;
    private WheelCollider[] wheels = Array.Empty<WheelCollider>();
    private Transform[] wheelVisuals = Array.Empty<Transform>();
    private float throttleInput;
    private float steeringInput;
    private bool handbrake;
    private Vector3 flatVelocity;

    public float SpeedKph => flatVelocity.magnitude * 3.6f;

    public void Configure(Rigidbody vehicleBody, WheelCollider[] wheelColliders)
    {
        body = vehicleBody;
        wheels = wheelColliders ?? Array.Empty<WheelCollider>();
        Transform[] all = GetComponentsInChildren<Transform>(true);
        System.Collections.Generic.List<Transform> visuals = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < all.Length; i++)
            if (all[i] != transform && all[i].name == "Tire") visuals.Add(all[i]);
        visuals.Sort((a, b) =>
        {
            int frontFirst = b.localPosition.z.CompareTo(a.localPosition.z);
            return frontFirst != 0 ? frontFirst : a.localPosition.x.CompareTo(b.localPosition.x);
        });
        wheelVisuals = visuals.ToArray();
        Array.Sort(wheels, (a, b) =>
        {
            int frontFirst = b.transform.localPosition.z.CompareTo(a.transform.localPosition.z);
            return frontFirst != 0 ? frontFirst : a.transform.localPosition.x.CompareTo(b.transform.localPosition.x);
        });
    }

    private void Awake()
    {
        if (body == null) body = GetComponent<Rigidbody>();
        if (wheels.Length == 0)
        {
            wheels = GetComponentsInChildren<WheelCollider>();
            Configure(body, wheels);
        }
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            float forward = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) -
                            (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            float turn = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) -
                         (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            throttleInput = forward;
            steeringInput = turn;
            handbrake = keyboard.spaceKey.isPressed;
            return;
        }
#endif
        throttleInput = Input.GetAxisRaw("Vertical");
        steeringInput = Input.GetAxisRaw("Horizontal");
        handbrake = Input.GetKey(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        if (body == null) return;
        flatVelocity = body.linearVelocity;
        flatVelocity.y = 0f;

        bool braking = handbrake || (flatVelocity.magnitude > 0.6f &&
            Mathf.Sign(throttleInput) != Mathf.Sign(Vector3.Dot(flatVelocity, transform.forward)) &&
            Mathf.Abs(throttleInput) > 0.1f);
        float forwardSpeed = Vector3.Dot(flatVelocity, transform.forward);
        if (handbrake || Mathf.Abs(throttleInput) < 0.08f)
        {
            Vector3 lateral = transform.right * Vector3.Dot(flatVelocity, transform.right);
            body.AddForce(-lateral * (handbrake ? 5f : 1.6f), ForceMode.Acceleration);
        }
        else if (!braking && flatVelocity.magnitude < maxSpeed)
        {
            float engine = throttleInput >= 0f ? acceleration : reverseAcceleration;
            body.AddForce(transform.forward * (throttleInput * engine), ForceMode.Acceleration);
        }

        if (braking) body.AddForce(-flatVelocity.normalized * brakingStrength, ForceMode.Acceleration);
        float speedInfluence = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 2.2f);
        float direction = forwardSpeed < -0.25f ? -1f : 1f;
        float turn = steeringInput * steerDegreesPerSecond * speedInfluence * direction * Time.fixedDeltaTime;
        if (speedInfluence > 0.01f)
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, turn, 0f));

        Vector3 sidewaysVelocity = transform.right * Vector3.Dot(body.linearVelocity, transform.right);
        body.AddForce(-sidewaysVelocity * grip, ForceMode.Acceleration);
        Vector3 velocity = body.linearVelocity;
        velocity.y = 0f;
        if (velocity.magnitude > maxSpeed) velocity = velocity.normalized * maxSpeed;
        body.linearVelocity = velocity;
        Vector3 position = body.position;
        body.position = new Vector3(position.x, 0.05f, position.z);
        ApplyWheelSteering();
        UpdateWheelVisuals();
    }

    private void ApplyWheelSteering()
    {
        if (wheels == null || wheels.Length < 4) return;
        float steer = steeringInput * 28f;
        wheels[0].steerAngle = steer;
        wheels[1].steerAngle = steer;
        for (int i = 0; i < wheels.Length; i++)
        {
            bool rearWheel = wheels[i].transform.localPosition.z < 0f;
            wheels[i].motorTorque = rearWheel && !handbrake ? throttleInput * 620f : 0f;
            wheels[i].brakeTorque = handbrake ? 1800f : 0f;
        }
    }

    private void UpdateWheelVisuals()
    {
        int count = Mathf.Min(wheels.Length, wheelVisuals.Length);
        for (int i = 0; i < count; i++)
        {
            wheels[i].GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheelVisuals[i].SetPositionAndRotation(position, rotation * Quaternion.Euler(0f, 0f, 90f));
        }
    }
}
