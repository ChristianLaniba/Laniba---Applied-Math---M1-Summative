using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float baseSpeed = 6f;
    public float acceleration = 14f;
    public float gravity = -20f;

    [Header("Speed modes (K key)")]
    public string[] modeNames = { "NORMAL", "BOOSTED", "REDUCED" }; // <--- CHANGED HERE
    public float[] modeMultipliers = { 1f, 2f, 0.5f };

    [Header("Rotation (X key)")]
    public float rotateSpeedDegPerSec = 150f;

    [Header("Scale (Y key)")]
    public float scaleRate = 0.9f;
    public float minScale = 0.5f;
    public float maxScale = 2.5f;

    public int ModeIndex { get; private set; }
    public float SpeedMultiplier => modeMultipliers[ModeIndex];
    public string ModeName => modeNames[ModeIndex];
    public Vector3 CurrentVelocity => velocity;

    CharacterController controller;
    Vector3 velocity;     
    float verticalVelocity;

    void Awake() => controller = GetComponent<CharacterController>();

    public void ResetMotion()
    {
        velocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    void Update()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float dt = Time.deltaTime;

        // K: cycle speed modes
        if (Input.GetKeyDown(KeyCode.K))
            ModeIndex = (ModeIndex + 1) % modeMultipliers.Length;

        // X: Rotate the Player (and child shield) manually
        if (Input.GetKey(KeyCode.X))
        {
            transform.Rotate(0f, (shift ? -1f : 1f) * rotateSpeedDegPerSec * dt, 0f, Space.World);
        }

        // Y: uniform scale
        if (Input.GetKey(KeyCode.Y))
        {
            float s = transform.localScale.x * (1f + (shift ? -1f : 1f) * scaleRate * dt);
            s = Mathf.Clamp(s, minScale, maxScale);
            transform.localScale = Vector3.one * s;
        }

        // --- WASD Input relative to Camera ---
        float h = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float v = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        Vector3 inputDir = new Vector3(h, 0f, v);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0f; 
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            inputDir = (camForward * v) + (camRight * h);
            if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();
        }

        Vector3 target = inputDir * baseSpeed * SpeedMultiplier;
        velocity = Vector3.Lerp(velocity, target, 1f - Mathf.Exp(-acceleration * dt));

        // Gravity
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * dt;

        controller.Move((velocity + Vector3.up * verticalVelocity) * dt);
    }

    // --- Simple Transformation HUD (Bottom Right) ---
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 32;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.LowerRight;

        Vector3 s = transform.localScale;
        Vector3 r = transform.eulerAngles;

        string text = 
            $"SPEED: {ModeName} (x{SpeedMultiplier})\n" +
            $"VELOCITY: {velocity.magnitude:F1} u/s\n" +
            $"SCALE: {s.x:F2} | {s.y:F2} | {s.z:F2}\n" +
            $"ROT: {r.x:F0}° | {r.y:F0}° | {r.z:F0}°";

        Rect rect = new Rect(0, 0, Screen.width - 20, Screen.height - 20);
        GUI.Label(rect, text, style);
    }
}