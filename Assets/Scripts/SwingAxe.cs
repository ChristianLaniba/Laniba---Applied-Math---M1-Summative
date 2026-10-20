using UnityEngine;

public class SwingAxe : MonoBehaviour
{
    [Header("Swing Settings")]
    [Tooltip("The maximum angle it swings to (e.g., 90).")]
    public float maxAngle = 90f;
    
    [Tooltip("How fast the axe swings back and forth.")]
    public float speed = 1.5f;

    [Tooltip("Check this if the axe starts at 0 degrees but you want it to swing from center.")]
    public bool startAtCenter = true;

    private float startZRotation;

    void Start()
    {
        // Remember the initial Z rotation so it swings relative to how you placed it
        startZRotation = transform.localEulerAngles.z;
    }

    void Update()
    {
        // Mathf.Sin creates a smooth wave between -1 and 1 over time
        float angleOffset = Mathf.Sin(Time.time * speed) * maxAngle;

        // Apply the rotation
        if (startAtCenter)
        {
            // Swings between start+90 and start-90
            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x, 
                transform.localEulerAngles.y, 
                startZRotation + angleOffset
            );
        }
        else
        {
            // Swings exactly between 90 and -90 (ignores the starting Z rotation)
            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x, 
                transform.localEulerAngles.y, 
                angleOffset
            );
        }
    }
}