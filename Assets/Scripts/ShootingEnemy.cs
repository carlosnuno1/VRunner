using UnityEngine;
using UnityEngine.AI;

public class ShootingEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, MaintainDistance, Retreat }
    public EnemyState currentState = EnemyState.Idle;

    public NavMeshAgent agent;
    public Transform player;

    [Header("Distance Settings")]
    public float detectionRange = 20f;
    public float idealDistance = 10f;
    public float retreatThreshold = 6f;
    public float shootingBuffer = 2f;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float attackCooldown = 1.5f;
    public float shootingAngleThreshold = 0f;
    public AudioSource EnemyAudioSource;
    public AudioClip EnemyShootSound;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distance <= detectionRange) currentState = EnemyState.MaintainDistance;
                break;

            case EnemyState.MaintainDistance:
                HandleMaintenance(distance);
                break;

            case EnemyState.Retreat:
                HandleRetreat(distance);
                break;
        }
    }

    void HandleMaintenance(float distance)
    {
        if (distance < retreatThreshold)
        {
            currentState = EnemyState.Retreat;
            return;
        }

        if (distance > idealDistance + shootingBuffer)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            LookAtPlayer();

            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToPlayer);

            if (dot >= shootingAngleThreshold)
            {
                TryAttack();
            }
        }
    }

    void HandleRetreat(float distance)
    {
        Vector3 dirAwayFromPlayer = transform.position - player.position;
        Vector3 retreatDestination = transform.position + dirAwayFromPlayer.normalized * 5f;

        agent.isStopped = false;
        agent.SetDestination(retreatDestination);

        if (distance > idealDistance + shootingBuffer)
        {
            currentState = EnemyState.MaintainDistance;
        }
    }

    void TryAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Shoot();
        }
    }

    void Shoot()
    {
        EnemyAudioSource.PlayOneShot(EnemyShootSound);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        Vector3 direction = (player.position - firePoint.position).normalized;
        rb.linearVelocity = direction * bulletSpeed;
        Destroy(bullet, 3f);
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
    }
}