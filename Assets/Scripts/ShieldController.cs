using System;
using UnityEngine;

/// <summary>
/// Front-facing arc shield. Put this on a child object ("Shield") of the player root,
/// at local position (0,0,0), so it inherits the player's rotation (X key) and scale (Y key).
/// Builds its own arc mesh at runtime and destroys any Projectile or FireProjectile that enters the arc.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShieldController : MonoBehaviour
{
    [Header("Shape (local units, before player scale)")]
    public float radius = 1.15f;
    public float height = 1.7f;
    public float yOffset = 0.3f;                    // bottom of the arc above the feet
    [Range(10f, 180f)] public float halfAngleDeg = 50f;   // 50 => 100 degree arc
    public int segments = 24;

    [Header("Look")]
    public Material shieldMaterial;                 // transparent cyan material
    public Color baseColor = new Color(0.4f, 1f, 0.94f, 0.4f);
    public Color flashColor = new Color(1f, 1f, 1f, 0.9f);
    public float flashTime = 0.25f;

    [Header("Detection")]
    public float projectileMargin = 0.3f;

    public int BlockedCount { get; private set; }
    public event Action<int> OnBlocked;             // HUD can subscribe

    MeshRenderer rend;
    float flashTimer;

    void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        GetComponent<MeshFilter>().mesh = BuildArcMesh();
        if (shieldMaterial != null) rend.material = shieldMaterial;   // creates an instance
        SetColor(baseColor);
    }

    void Update()
    {
        FlashUpdate();
        BlockProjectiles();
    }

    void BlockProjectiles()
    {
        float s = transform.lossyScale.x;                   // player's uniform scale
        float shieldR = radius * s;
        float h = (yOffset + height) * s;
        Vector3 origin = transform.position;

        // Direction the shield faces, flattened to the XZ plane
        Vector3 facing = transform.forward; facing.y = 0f; facing.Normalize();

        float searchR = Mathf.Sqrt(shieldR * shieldR + h * h) + projectileMargin;
        Collider[] hits = Physics.OverlapSphere(origin, searchR, ~0, QueryTriggerInteraction.Collide);

        foreach (Collider c in hits)
        {
            // --- CHECK FOR STANDARD PROJECTILE ---
            Projectile p = c.GetComponent<Projectile>();
            
            // --- CHECK FOR DRAGON FIRE PROJECTILE ---
            FireProjectile fp = c.GetComponent<FireProjectile>();

            // If it's neither, skip it
            if (p == null && fp == null) continue;

            // Check if the projectile is within the vertical and horizontal arc
            Vector3 to = c.transform.position - origin;
            if (to.y < -0.5f || to.y > h + projectileMargin) continue;   // vertical range
            to.y = 0f;
            float dist = to.magnitude;
            if (dist < 0.0001f || dist > shieldR + projectileMargin) continue;

            // Inside the arc if the angle to the facing direction is within the half-angle
            if (Vector3.Angle(facing, to) <= halfAngleDeg)
            {
                // Block it!
                if (p != null) p.Block();
                if (fp != null) fp.Block();

                BlockedCount++;
                flashTimer = flashTime;
                OnBlocked?.Invoke(BlockedCount);
            }
        }
    }

    void FlashUpdate()
    {
        if (flashTimer <= 0f) return;
        flashTimer -= Time.deltaTime;
        SetColor(Color.Lerp(baseColor, flashColor, Mathf.Clamp01(flashTimer / flashTime)));
    }

    void SetColor(Color c) { if (rend != null && rend.material != null) rend.material.color = c; }

    // Arc centred on local +Z. x = r*sin(a), z = r*cos(a). Two vertex sets so each side is lit correctly.
    Mesh BuildArcMesh()
    {
        int cols = segments + 1;
        var v = new Vector3[cols * 4];
        var n = new Vector3[cols * 4];
        var uv = new Vector2[cols * 4];
        var tris = new int[segments * 12];
        float half = halfAngleDeg * Mathf.Deg2Rad;
        float y0 = yOffset, y1 = yOffset + height;

        for (int i = 0; i < cols; i++)
        {
            float t = (float)i / segments;
            float a = Mathf.Lerp(-half, half, t);
            Vector3 outDir = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
            Vector3 p = outDir * radius;
            for (int side = 0; side < 2; side++)            // 0 = outward face, 1 = inward face
            {
                int b = side * cols * 2 + i * 2;
                v[b] = new Vector3(p.x, y0, p.z);
                v[b + 1] = new Vector3(p.x, y1, p.z);
                n[b] = n[b + 1] = side == 0 ? outDir : -outDir;
                uv[b] = new Vector2(t, 0f);
                uv[b + 1] = new Vector2(t, 1f);
            }
        }

        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int b0 = i * 2, t0 = b0 + 1, b1 = b0 + 2, t1 = b0 + 3;
            // outward face
            tris[ti++] = b0; tris[ti++] = b1; tris[ti++] = t0;
            tris[ti++] = b1; tris[ti++] = t1; tris[ti++] = t0;
            // inward face (reversed winding, second vertex set)
            int o = cols * 2;
            tris[ti++] = o + b0; tris[ti++] = o + t0; tris[ti++] = o + b1;
            tris[ti++] = o + b1; tris[ti++] = o + t0; tris[ti++] = o + t1;
        }

        var m = new Mesh { name = "ShieldArc", vertices = v, normals = n, uv = uv, triangles = tris };
        m.RecalculateBounds();
        return m;
    }

    // Visualise the blocking arc in the Scene view
    void OnDrawGizmosSelected()
    {
        float s = Application.isPlaying ? transform.lossyScale.x : 1f;
        Gizmos.color = Color.cyan;
        Vector3 f = Quaternion.Euler(0, -halfAngleDeg, 0) * transform.forward;
        Vector3 g = Quaternion.Euler(0, halfAngleDeg, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + f * radius * s);
        Gizmos.DrawLine(transform.position, transform.position + g * radius * s);
    }
}