using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;

    [Header("Framing")]
    public Vector3 lookAtOffset = new Vector3(0f, 0f, 1f); 
    
    [Header("Vertical Lock")]
    [Tooltip("How high above the player's center the camera must stay.")]
    public float minVerticalOffset = 1.5f; 
    
    [Header("Mouse Orbit")]
    public float mouseSensitivity = 3f;
    public float minPitch = -20f; 
    public float maxPitch = 60f;  
    public bool lockCursor = true;

    [Header("Zoom")]
    public float defaultDistance = 12f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float zoomSpeed = 5f;
    private float currentDistance;

    [Header("Smoothing")]
    public float followSharpness = 10f; 

    [Header("Scale-aware zoom")]
    [Range(0f, 1f)] public float scaleZoom = 0.5f;

    private float yaw = 0f;
    private float pitch = 20f; 

    void Start()
    {
        currentDistance = defaultDistance;
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Handle Mouse Input
        if (!PauseMenu.IsPaused)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                currentDistance -= scroll * zoomSpeed;
                currentDistance = Mathf.Clamp(currentDistance, minZoom, maxZoom);
            }
        }

        // Calculate target position
        float s = Mathf.Lerp(1f, target.localScale.x, scaleZoom);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + (rotation * new Vector3(0f, 0f, -currentDistance * s));

        // --- VERTICAL LOCK ---
        // Ensure the camera never drops below the player's center + the vertical offset
        float minCameraY = target.position.y + (minVerticalOffset * s); 
        desiredPosition.y = Mathf.Max(desiredPosition.y, minCameraY);

        // Apply Smoothing
        float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, t);

        // Look at the player
        transform.LookAt(target.position + lookAtOffset);
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        float s = Mathf.Lerp(1f, target.localScale.x, scaleZoom);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + (rotation * new Vector3(0f, 0f, -currentDistance * s));

        // Apply lock even when snapping
        float minCameraY = target.position.y + (minVerticalOffset * s);
        desiredPosition.y = Mathf.Max(desiredPosition.y, minCameraY);

        transform.position = desiredPosition;
        transform.LookAt(target.position + lookAtOffset);
    }
}