using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollingBoulder : MonoBehaviour
{
    [Header("Boulder Settings")]
    public float rollSpeed = 15f;
    public float lifeTime = 6f;

    [Header("Ground Stick")]
    [Tooltip("Keeps the boulder glued to the floor. Increase if it still bounces.")]
    public float downforce = 15f; 

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        // Optional: Makes the boulder look smooth while rolling
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Optional: Prevents the boulder from teleporting through the floor
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // 1. Push forward down the slope
        Vector3 forwardDir = transform.forward;
        rb.AddForce(forwardDir * rollSpeed, ForceMode.Acceleration);

        // 2. Push down to stick to the floor (counteracts bouncing)
        rb.AddForce(Vector3.down * downforce, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponentInParent<PlayerRespawn>();
        if (player != null)
        {
            player.Kill();
        }
    }
}