using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SlipperyPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float baseSpeed = 6f;
    public float gravity = -20f;
    
    [Tooltip("How quickly momentum fades when no keys are pressed.")]
    public float drag = 1.2f; 

    [Header("Grip & Slipperiness")]
    [Tooltip("Grip when at Max Size (2.5) and Reduced Speed (0.5x). Should feel like normal levels.")]
    public float maxGripAcceleration = 14f; 
    
    [Tooltip("Grip when at Min Size (0.5) and Boosted Speed (2x). Should feel like pure ice.")]
    public float minGripAcceleration = 0.2f; 

    [Tooltip("Shapes the grip curve. Pull the right side up to make normal size slippery. (X = Size/Speed, Y = Grip)")]
    public AnimationCurve gripCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Threshold where movement switches from slippery (RotateTowards) to snappy (Lerp). Higher = only big sizes get snappy.")]
    public float snappyThreshold = 0.75f; 

    [Header("Steering (Turning)")]
    public float baseTurnSpeed = 180f;
    public float sizeTurnPenalty = 1.5f;
    public float speedTurnPenalty = 0.5f;

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
    
    public float CurrentAcceleration { get; private set; }
    public float CurrentGripRatio { get; private set; }

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

        // K: Cycle speed modes
        if (Input.GetKeyDown(KeyCode.K))
            ModeIndex = (ModeIndex + 1) % modeMultipliers.Length;

        // X: Rotate manually
        if (Input.GetKey(KeyCode.X))
        {
            transform.Rotate(0f, (shift ? -1f : 1f) * rotateSpeedDegPerSec * dt, 0f, Space.World);
        }

        // Y: Scale
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

        // --- CALCULATE GRIP ---
        float sizeRatio = Mathf.InverseLerp(minScale, maxScale, transform.localScale.x);
        float speedModeRatio = Mathf.InverseLerp(2f, 0.5f, SpeedMultiplier);

        float rawGripRatio = (sizeRatio + speedModeRatio) * 0.5f;
        CurrentGripRatio = gripCurve.Evaluate(rawGripRatio);
        CurrentAcceleration = Mathf.Lerp(minGripAcceleration, maxGripAcceleration, CurrentGripRatio);

        // --- APPLY MOVEMENT ---
        if (inputDir.sqrMagnitude > 0.01f)
        {
            Vector3 targetVelocity = inputDir * baseSpeed * SpeedMultiplier;

            if (CurrentGripRatio > snappyThreshold)
            {
                velocity = Vector3.Lerp(velocity, targetVelocity, 1f - Mathf.Exp(-CurrentAcceleration * dt));
            }
            else
            {
                float sizeTurnFactor = Mathf.Clamp01(transform.localScale.x / (1f + sizeTurnPenalty)); 
                float maxPossibleSpeed = baseSpeed * 2f; 
                float speedRatio = Mathf.Clamp01(velocity.magnitude / maxPossibleSpeed);
                float speedTurnFactor = 1f / (1f + (speedRatio * speedTurnPenalty));
                float dynamicTurnSpeed = baseTurnSpeed * sizeTurnFactor * speedTurnFactor;

                velocity = Vector3.RotateTowards(velocity, targetVelocity, dynamicTurnSpeed * Mathf.Deg2Rad * dt, CurrentAcceleration * dt);
            }
        }
        else
        {
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, drag * dt);
        }

        // --- GRAVITY & MOVE ---
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