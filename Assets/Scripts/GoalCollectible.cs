using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class GoalCollectible : MonoBehaviour
{
    [Header("Level flow")]
    public string nextSceneName = "Level2";
    public float loadDelay = 1f;
    public UnityEvent onCollected; 

    [Header("Motion")]
    public float spinDegPerSec = 90f;
    public float bobHeight = 0.2f;
    public float bobSpeed = 3f;

    [Header("Generated ring (used when no mesh is set)")]
    public float ringRadius = 1.2f;
    public float tubeRadius = 0.2f;
    public float colliderDepth = 2.6f;
    public bool autoSizeCollider = true;
    public Material ringMaterial;           

    [Header("Feedback")]
    public AudioClip collectSound;

    bool collected;
    Vector3 basePos;
    Vector3 baseScale;

    void Awake()
    {
        var mf = GetComponent<MeshFilter>();
        bool generated = mf.sharedMesh == null;
        if (generated) mf.mesh = BuildTorus(ringRadius, tubeRadius, 32, 12);
        if (ringMaterial != null) GetComponent<MeshRenderer>().material = ringMaterial;

        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        if (autoSizeCollider)
        {
            if (generated)
            {
                float outer = 2f * (ringRadius + tubeRadius);
                box.center = Vector3.zero;
                box.size = new Vector3(outer, outer, colliderDepth);
            }
            else
            {
                Bounds b = mf.sharedMesh.bounds;
                box.center = b.center;
                box.size = new Vector3(b.size.x, b.size.y, Mathf.Max(b.size.z, colliderDepth * 0.5f));
            }
        }

        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start()
    {
        basePos = transform.position;
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (collected) return;
        transform.Rotate(0f, spinDegPerSec * Time.deltaTime, 0f, Space.World);
        transform.position = basePos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        
        // --- FIXED: Look for either player controller ---
        bool isPlayer = other.GetComponentInParent<PlayerController>() != null || 
                        other.GetComponentInParent<SlipperyPlayerController>() != null;
        
        if (!isPlayer) return;
        
        StartCoroutine(Collect());
    }

    IEnumerator Collect()
    {
        collected = true;
        GetComponent<Collider>().enabled = false;
        if (collectSound != null) AudioSource.PlayClipAtPoint(collectSound, transform.position);
        onCollected?.Invoke();

        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / dur);
            transform.localScale = baseScale * k;
            transform.Rotate(0f, spinDegPerSec * 6f * Time.deltaTime, 0f, Space.World);
            yield return null;
        }
        transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(Mathf.Max(0f, loadDelay - dur));

        if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene(nextSceneName);
    }

    static Mesh BuildTorus(float R, float r, int ringSegs, int tubeSegs)
    {
        var v = new Vector3[(ringSegs + 1) * (tubeSegs + 1)];
        var n = new Vector3[v.Length];
        var uv = new Vector2[v.Length];
        var tris = new int[ringSegs * tubeSegs * 6];

        for (int i = 0; i <= ringSegs; i++)
        {
            float u = (float)i / ringSegs * Mathf.PI * 2f;
            float cu = Mathf.Cos(u), su = Mathf.Sin(u);
            for (int j = 0; j <= tubeSegs; j++)
            {
                float w = (float)j / tubeSegs * Mathf.PI * 2f;
                float cw = Mathf.Cos(w), sw = Mathf.Sin(w);
                int idx = i * (tubeSegs + 1) + j;
                v[idx] = new Vector3((R + r * cw) * cu, (R + r * cw) * su, r * sw);
                n[idx] = new Vector3(cw * cu, cw * su, sw);
                uv[idx] = new Vector2((float)i / ringSegs, (float)j / tubeSegs);
            }
        }

        int ti = 0;
        for (int i = 0; i < ringSegs; i++)
        {
            for (int j = 0; j < tubeSegs; j++)
            {
                int a = i * (tubeSegs + 1) + j;
                int b = (i + 1) * (tubeSegs + 1) + j;
                int c = a + 1;
                int d = b + 1;
                tris[ti++] = a; tris[ti++] = b; tris[ti++] = c;
                tris[ti++] = b; tris[ti++] = d; tris[ti++] = c;
            }
        }

        var m = new Mesh { name = "GoalRing", vertices = v, normals = n, uv = uv, triangles = tris };
        m.RecalculateBounds();
        return m;
    }
}