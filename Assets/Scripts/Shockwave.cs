using UnityEngine;

public class ShockwaveBehavior : MonoBehaviour
{
    public float delay = 2f;
    public float radius = 8f;
    public float explosionForce = 1500f; // High force for enemies
    public float playerLaunchPower = 20f; // Snappy jump for player
    public float upwardsModifier = 1.5f; // Makes things go "Up and Out"

    void Start()
    {
        // Start the fuse immediately
        Invoke("Explode", delay);
    }

    void Explode()
    {
        // Find everything in the blast zone
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null) continue;

            if (hit.CompareTag("Player"))
            {
                // Fortnite style: Calculate direction from grenade to player
                Vector3 launchDir = (hit.transform.position - transform.position).normalized;

                // Add an upward boost so it's a "jump" and not just a slide
                launchDir.y += 0.5f;

                // Override velocity for that instant "pop" feel
                rb.linearVelocity = launchDir * playerLaunchPower;
            }
            else
            {
                // Normal explosion physics for enemies/props
                rb.AddExplosionForce(explosionForce, transform.position, radius, upwardsModifier);
            }
        }

        // TODO: Instantiate an explosion particle effect here later
        Destroy(gameObject);
    }
}