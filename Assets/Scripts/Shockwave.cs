using UnityEngine;

public class ShockwaveGrenade : MonoBehaviour
{
    public float fuseDelay = 2.5f;
    public float blastRadius = 8f;
    public float launchPower = 25f;
    public GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private bool hasStuck = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Gravity stays ON so it arcs naturally
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If we hit the Player or another Grenade, don't stick (Physics Layer fix below is better though)
        if (collision.gameObject.CompareTag("Player")) return;

        if (!hasStuck)
        {
            hasStuck = true;

            // Stop all movement and lock it to the ground
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            GetComponent<Renderer>().material.color = Color.red;
            Invoke(nameof(Explode), fuseDelay);
        }
    }

    void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }

        Collider[] victims = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (Collider hit in victims)
        {
            Rigidbody victimRb = hit.GetComponent<Rigidbody>();
            if (victimRb == null) continue;

            Vector3 dir = (hit.transform.position - transform.position).normalized;
            dir.y += 0.5f;

            if (hit.CompareTag("Player"))
            {
                victimRb.linearVelocity = dir * launchPower;
            }
            else
            {
                victimRb.AddExplosionForce(1500f, transform.position, blastRadius, 1.5f);
            }
        }
        Destroy(gameObject);
    }
}