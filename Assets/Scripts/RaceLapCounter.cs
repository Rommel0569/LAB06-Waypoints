using System.Collections.Generic;
using UnityEngine;

/// <summary>Counts complete circuits when each car crosses the start/finish trigger.</summary>
public sealed class RaceLapCounter : MonoBehaviour
{
    private readonly HashSet<int> hasDepartedGate = new HashSet<int>();
    private readonly Dictionary<int, int> lapsByVehicle = new Dictionary<int, int>();

    private void Awake()
    {
        Collider gate = GetComponent<Collider>();
        if (gate != null) gate.isTrigger = true;
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    public int GetLaps(FollowWP vehicle)
    {
        if (vehicle == null) return 0;
        return lapsByVehicle.TryGetValue(vehicle.GetInstanceID(), out int laps) ? laps : 0;
    }

    private void OnTriggerExit(Collider other)
    {
        FollowWP vehicle = other.GetComponentInParent<FollowWP>();
        if (vehicle != null) hasDepartedGate.Add(vehicle.GetInstanceID());
    }

    private void OnTriggerEnter(Collider other)
    {
        FollowWP vehicle = other.GetComponentInParent<FollowWP>();
        if (vehicle == null) return;
        int id = vehicle.GetInstanceID();
        if (!hasDepartedGate.Contains(id)) return;
        lapsByVehicle.TryGetValue(id, out int laps);
        lapsByVehicle[id] = laps + 1;
    }
}
