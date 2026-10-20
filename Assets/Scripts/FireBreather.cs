using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Dragon. Handles breathing fire and spawning the projectiles.
/// </summary>
public class FireBreather : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your invisible fire projectile prefab here")]
    public GameObject fireProjectilePrefab;
    
    [Tooltip("The transform where the fire comes out (the dragon's mouth)")]
    public Transform firePoint;

    [Tooltip("Optional: The Particle System showing the fire visuals")]
    public ParticleSystem fireParticles;

    [Header("Breathing Settings")]
    public float timeBetweenBreaths = 4f;
    public float breathDuration = 2f;
    
    [Tooltip("How fast projectiles spawn during a breath")]
    public float spawnRate = 0.1f; // 10 projectiles per second

    [Header("Spread")]
    [Tooltip("The maximum angle (in degrees) the fire can deviate from the center. 15 = 15 degrees left/right/up/down.")]
    public float spreadAngle = 15f;

    void Start()
    {
        if (firePoint == null) firePoint = transform; // Default to dragon's center
        if (fireParticles != null) fireParticles.Stop(); // Make sure particles are off at start

        StartCoroutine(BreathLoop());
    }

    IEnumerator BreathLoop()
    {
        while (true)
        {
            // Wait between breaths
            yield return new WaitForSeconds(timeBetweenBreaths);

            // Start breathing
            if (fireParticles != null) fireParticles.Play();

            float timer = 0f;
            while (timer < breathDuration)
            {
                if (fireProjectilePrefab != null)
                {
                    // Calculate a random rotation for the spread
                    float randomYaw = Random.Range(-spreadAngle, spreadAngle);
                    float randomPitch = Random.Range(-spreadAngle, spreadAngle);
                    
                    // Create the new rotation based on the firePoint's rotation, plus the random offsets
                    Quaternion spreadRotation = firePoint.rotation * Quaternion.Euler(randomPitch, randomYaw, 0f);

                    // Spawn the projectile with the new spread rotation
                    Instantiate(fireProjectilePrefab, firePoint.position, spreadRotation);
                }

                yield return new WaitForSeconds(spawnRate);
                timer += spawnRate;
            }

            // Stop breathing
            if (fireParticles != null) fireParticles.Stop();
        }
    }
}