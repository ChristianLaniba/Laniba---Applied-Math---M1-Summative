using System.Collections;
using UnityEngine;

public class IceSpikeSpawner : MonoBehaviour
{
    [Header("Prefabs & Materials")]
    public GameObject spikePrefab;
    public Material telegraphMaterial;

    [Header("Spawn Settings")]
    public float spawnRadius = 5f;
    public float spawnInterval = 1.5f;
    
    [Tooltip("How far down the telegraph hangs from this spawner's position")]
    public float telegraphLength = 15f; 

    [Tooltip("How long the warning shows before the spike falls")]
    public float telegraphDuration = 1.5f;

    [Header("Telegraph Size")]
    public float telegraphRadius = 0.5f;

    void Start()
    {
        if (spikePrefab == null)
        {
            Debug.LogError("Spike Prefab is not assigned on " + gameObject.name);
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            StartCoroutine(SingleSpikeSequence(spawnPos));

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator SingleSpikeSequence(Vector3 anchorPos)
    {
        // 1. Create the telegraph hanging DOWN from the anchor
        GameObject telegraph = CreateTelegraph(anchorPos);

        // 2. Wait for the warning duration
        yield return new WaitForSeconds(telegraphDuration);

        // 3. Remove the telegraph
        if (telegraph != null) Destroy(telegraph);

        // 4. Spawn the actual spike AT THE TOP (anchorPos)
        if (spikePrefab != null)
        {
            // FIXED: Spawn at the anchor position (the top), not the bottom
            Instantiate(spikePrefab, anchorPos, Quaternion.identity);
        }
    }

    GameObject CreateTelegraph(Vector3 anchorPos)
    {
        GameObject telegraph = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        telegraph.name = "IceSpike_Telegraph";
        Destroy(telegraph.GetComponent<Collider>());

        float yScale = telegraphLength / 2f;
        telegraph.transform.localScale = new Vector3(telegraphRadius * 2f, yScale, telegraphRadius * 2f);

        // Position so the TOP of the cylinder is at the anchor
        telegraph.transform.position = anchorPos - new Vector3(0f, yScale, 0f);

        if (telegraphMaterial != null)
        {
            telegraph.GetComponent<Renderer>().material = telegraphMaterial;
        }

        return telegraph;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(0f, telegraphLength, 0f));
    }
}