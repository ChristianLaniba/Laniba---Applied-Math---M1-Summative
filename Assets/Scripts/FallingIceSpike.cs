using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    public float fallSpeed = 20f;
    public float destroyAfterSeconds = 5f;

    void Start()
    {
        // Destroy the spike after a few seconds to save memory
        Destroy(gameObject, destroyAfterSeconds);
    }

    void Update()
    {
        // Move the spike straight down
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
    }

    // Optional: If you have a hazard script, call it here
    // void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         collision.gameObject.GetComponent<PlayerRespawn>().Kill();
    //     }
    // }
}