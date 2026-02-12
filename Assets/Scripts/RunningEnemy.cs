using UnityEngine;
using UnityEngine.AI;

public class RunningEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, Chase, Charging, Lunging, Recovery, Stunned }
    public EnemyState currentState = EnemyState.Idle;

    public NavMeshAgent agent;
    public Transform player;
    private Rigidbody rb;
    private Collider enemyCollider;

    [Header("Distance Settings")]
    public float detectionRange = 20f;
    public float lungeRange = 3.5f;

    [Header("Lunge Settings")]
    public float chargeTime = 0.6f;
    public float lungeSpeed = 50f;
    public float lungeDuration = 0.2f;
    public float recoveryTime = 1.0f;
    public float stunTime = 2.0f;

    private Vector3 lungeDirection;
    private float stateTimer;
    private bool hasDealtDamage;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();
        rb.isKinematic = true;

        agent.speed = 12f;
        agent.acceleration = 30f;
        agent.angularSpeed = 120f;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distance <= detectionRange) currentState = EnemyState.Chase;
                break;
            case EnemyState.Chase:
                HandleChase(distance);
                break;
            case EnemyState.Charging:
                HandleCharging(distance);
                break;
            case EnemyState.Lunging:
                HandleLunge();
                break;
            case EnemyState.Recovery:
                HandleRecovery();
                break;
            case EnemyState.Stunned:
                HandleStun();
                break;
        }
    }

    void HandleChase(float distance)
    {
        if (!agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (distance <= lungeRange && HasLineOfSight())
        {
            currentState = EnemyState.Charging;
            stateTimer = chargeTime;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    void HandleCharging(float distance)
    {
        if (distance > lungeRange + 1.5f || !HasLineOfSight())
        {
            currentState = EnemyState.Chase;
            return;
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0) PrepareLunge();
    }

    void PrepareLunge()
    {
        currentState = EnemyState.Lunging;
        stateTimer = lungeDuration;
        hasDealtDamage = false;

        agent.enabled = false;
        rb.isKinematic = false;

        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider != null) Physics.IgnoreCollision(enemyCollider, playerCollider, true);

        lungeDirection = (player.position - transform.position).normalized;
        lungeDirection.y = 0.02f;
    }

    void HandleLunge()
    {
        stateTimer -= Time.deltaTime;
        rb.linearVelocity = lungeDirection * lungeSpeed;

        if (stateTimer <= 0) ExitLunge(recoveryTime, EnemyState.Recovery);
    }

    void ExitLunge(float time, EnemyState nextState)
    {
        stateTimer = time;
        currentState = nextState;

        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider != null) Physics.IgnoreCollision(enemyCollider, playerCollider, false);
    }

    void HandleRecovery()
    {
        stateTimer -= Time.deltaTime;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.deltaTime * 5f);
        if (stateTimer <= 0) ReenableAgent();
    }

    void HandleStun()
    {
        stateTimer -= Time.deltaTime;
        rb.linearVelocity = Vector3.zero;
        if (stateTimer <= 0) ReenableAgent();
    }

    void ReenableAgent()
    {
        rb.isKinematic = true;
        agent.enabled = true;
        NavMeshHit nHit;
        if (NavMesh.SamplePosition(transform.position, out nHit, 3.0f, NavMesh.AllAreas))
        {
            agent.Warp(nHit.position);
            currentState = EnemyState.Chase;
        }
    }

    bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 direction = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position + Vector3.up, direction, out hit, lungeRange + 2f))
        {
            if (hit.collider.CompareTag("Player")) return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState == EnemyState.Lunging && other.CompareTag("Player") && !hasDealtDamage)
        {
            hasDealtDamage = true;
            Debug.Log("PLAYER HIT BY RUNNING ENEMY");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentState == EnemyState.Lunging && !collision.gameObject.CompareTag("Player"))
        {
            ExitLunge(stunTime, EnemyState.Stunned);
        }
    }
}