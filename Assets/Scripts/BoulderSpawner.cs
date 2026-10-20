using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns boulders at regular intervals. Attach to an empty GameObject at the top of the cliff.
/// </summary>
public class BoulderSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject boulderPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    
    [Tooltip("Initial push force to get the boulder rolling off the ledge")]
    public float initialPushForce = 5f;

    void Start()
    {
        if (boulderPrefab == null)
        {
            Debug.LogError("Boulder Prefab is not assigned on " + gameObject.name);
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Wait for the next interval
            yield return new WaitForSeconds(spawnInterval);

            // Spawn the boulder slightly above the spawner to prevent clipping
            Vector3 spawnPos = transform.position + Vector3.up;
            
            // Spawn facing the same direction as the spawner (make sure the blue arrow points down the cliff)
            GameObject newBoulder = Instantiate(boulderPrefab, spawnPos, transform.rotation);

            // Give it a tiny nudge to ensure it starts rolling
            Rigidbody rb = newBoulder.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * initialPushForce, ForceMode.Impulse);
            }
        }
    }
}