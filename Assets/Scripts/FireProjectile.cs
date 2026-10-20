using UnityEngine;

/// <summary>
/// Attach this to the invisible trigger object that the Dragon breathes out.
/// It moves forward, checks for hits, and works with the ShieldController.
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class FireProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 3f;

    void Start()
    {
        // Ensure physics is set up correctly for a trigger projectile
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        GetComponent<Collider>().isTrigger = true;

        // Destroy after a few seconds so it doesn't travel forever
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Move forward constantly
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Did we hit the player?
        var player = other.GetComponentInParent<PlayerRespawn>();
        if (player != null)
        {
            player.Kill();
            Destroy(gameObject);
            return;
        }

        // 2. Did we hit the environment (a wall/floor)?
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    // --- THE SHIELD CALLS THIS METHOD ---
    public void Block()
    {
        // You can add a particle effect here later if you want
        Destroy(gameObject);
    }
}