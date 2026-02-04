using UnityEngine;

public class ShockwaveGrenade : MonoBehaviour
{
    public float fuseDelay = 2.5f;
    public float blastRadius = 8f;
    public float launchPower = 25f;
    public GameObject explosionEffectPrefab;
    private bool thrown;

    private Rigidbody rb;
    private bool hasStuck = false;
    private Vector3 upAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Gravity stays ON so it arcs naturally
        rb.useGravity = true;
        upAngle = new Vector3(0, 8, 0);
    }

    public void Thrown()
    {
        thrown = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If we hit the Player or another Grenade, don't stick (Physics Layer fix below is better though)
        if (collision.gameObject.CompareTag("Player")) return;

        if (!hasStuck && thrown)
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
                victimRb.linearVelocity = (dir + upAngle) * launchPower;
                Debug.Log("dir = " + dir);
                Debug.Log("launchpower = " + launchPower);
                Debug.Log("dir * launchpower = " + dir * launchPower);
            }

        }
        Destroy(gameObject);
    }
}