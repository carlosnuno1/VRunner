using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeLauncher : MonoBehaviour
{
    public GameObject grenadePrefab;
    public float throwForce = 15f; // Lower this for more arc, higher for "flatter" throw

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Throw();
        }
    }

    void Throw()
    {
        // 1. Spawn it slightly in front of the camera so it doesn't hit your head
        Vector3 spawnPos = transform.position + (transform.forward * 0.6f);
        GameObject grenade = Instantiate(grenadePrefab, spawnPos, transform.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 2. Launch it exactly where the camera is looking
            // Because gravity is ON, it will naturally start to drop after this initial push
            rb.linearVelocity = transform.forward * throwForce;
        }
    }
}