using UnityEngine;

/// <summary>
/// Minimal projectile: flies straight and expires. Needs a Collider (Is Trigger ON) so the shield can find it.
/// The turret script (next step) will call Launch(). Player damage is added with the hazard scripts.
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    void Awake()
    {
        // Trigger events against static walls need a Rigidbody on one side
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        GetComponent<Collider>().isTrigger = true;
    }

    public float speed = 7f;
    public float lifetime = 6f;

    Vector3 direction = Vector3.forward;

    public void Launch(Vector3 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
    }

    void Start() => Destroy(gameObject, lifetime);

    void Update() => transform.position += direction * speed * Time.deltaTime;

    /// <summary>Called by the shield when it stops this projectile.</summary>
    public void Block()
    {
        // TODO: spawn a small spark effect here if you like
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Walls stop projectiles (tag your wall objects "Wall")
        if (other.CompareTag("Wall")) Destroy(gameObject);
    }
}
