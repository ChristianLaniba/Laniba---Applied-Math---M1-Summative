using System;
using UnityEngine;

// Remove PlayerController from the RequireComponent so it works with both
[RequireComponent(typeof(CharacterController))]
public class PlayerRespawn : MonoBehaviour
{
    [Tooltip("Leave empty to use the position the player starts at")]
    public Transform spawnPoint;
    public float invulnerableSeconds = 1f;

    public int HitCount { get; private set; }
    public event Action<int> OnHit;

    CharacterController controller;
    MonoBehaviour motion; // Changed from PlayerController to MonoBehaviour
    Vector3 spawnPos;
    float invulnerableUntil;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Try to find either the normal or slippery controller
        motion = GetComponent<PlayerController>() as MonoBehaviour;
        if (motion == null) motion = GetComponent<SlipperyPlayerController>() as MonoBehaviour;

        spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
    }

    public void Kill()
    {
        if (Time.time < invulnerableUntil) return;
        HitCount++;
        Respawn();
        OnHit?.Invoke(HitCount);
    }

    public void Respawn()
    {
        controller.enabled = false;
        transform.position = spawnPos;
        controller.enabled = true;
        
        // Call ResetMotion safely
        if (motion is PlayerController normal) normal.ResetMotion();
        else if (motion is SlipperyPlayerController slippery) slippery.ResetMotion();

        invulnerableUntil = Time.time + invulnerableSeconds;
    }
}