using UnityEngine;

/// <summary>
/// Unblockable hazard: anything with this component sends the player back to spawn on contact.
/// Use it on spikes (static) and on patrol drones (together with PatrolHazard).
/// The shield only looks for Projectile components, so hazards ignore it completely.
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Hazard : MonoBehaviour
{
    void Awake()
    {
        // Kinematic Rigidbody so trigger events fire reliably, even for moving hazards
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other) => TryHit(other);
    void OnTriggerStay(Collider other) => TryHit(other);   // catches hazards that move onto a standing player

    void TryHit(Collider other)
    {
        var player = other.GetComponentInParent<PlayerRespawn>();
        if (player != null) player.Kill();
    }
}
