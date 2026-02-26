using UnityEngine;
using UnityEngine.AI;

public class EnemyAIBehavior : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Shoot,
        Reposition
    }

    [Header("Waypoints")]
    public Transform[] waypoints;
    public Transform player;

    [Header("Detection")]
    public float detectionRadius = 10f;
    public float playerReachingPoint = 1f;

    [Header("Movement")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 5.5f;
    public float waypointReachingPoint = 0.5f;

    [Header("Shooting")]
    public GameObject pewpewPrefab;
    public Transform firePoint;
    public float timeBetweenShots = 1f;
    public int shotCount = 3;
    public float shootingDistance = 8f;
    public float bulletForce = 40f;

    [Header("Rotation")]
    public float bodyRotationSpeed = 5f;

    [Header("Reposition (Flying Rigidbody)")]
    public float repositionRadius = 5f;
    public float minDistanceFromPlayer = 2f;
    public float flyForce = 50f;
    public float maxFlySpeed = 20f;
    public float hoverHeight = 2f;
    public float arrivalDistance = 0.5f;

    private EnemyState currentState = EnemyState.Patrol;

    private NavMeshAgent agent;
    private Rigidbody rb;

    private int currentWaypoint = 0;
    private int shotsFired = 0;
    private float shotTimer = 0f;
    private float buffer = 2f;

    private bool isFlying = false;
    private Vector3 flyTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        agent.speed = patrolSpeed;
        agent.stoppingDistance = waypointReachingPoint;

        if (waypoints.Length > 0)
        {
            MoveToNextWaypoint();
        }
    }

    void Update()
    {
        if (isFlying)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol(distance);
                break;

            case EnemyState.Chase:
                UpdateChase(distance);
                break;

            case EnemyState.Shoot:
                UpdateShoot(distance);
                break;

            case EnemyState.Reposition:
                BeginReposition();
                break;
        }
    }

    void FixedUpdate()
    {
        if (isFlying)
        {
            HandleFlyingPhysics();
        }
    }

    void UpdatePatrol(float distance)
    {
        if (distance <= detectionRadius)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        PatrolWaypoints();
    }

    void UpdateChase(float distance)
    {
        if (distance > detectionRadius * buffer)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        if (distance <= shootingDistance)
        {
            ChangeState(EnemyState.Shoot);
            return;
        }

        agent.SetDestination(player.position);
    }

    void UpdateShoot(float distance)
    {
        if (distance > shootingDistance + buffer)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        agent.isStopped = true;
        RotateTowardsPlayer();

        shotTimer -= Time.deltaTime;

        if (shotTimer <= 0f)
        {
            Shoot();
            shotTimer = timeBetweenShots;
            shotsFired++;

            if (shotsFired >= shotCount)
            {
                ChangeState(EnemyState.Reposition);
            }
        }
    }

    void BeginReposition()
    {
        flyTarget = FindRepositionPosition();
        flyTarget.y = hoverHeight;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        rb.linearVelocity = Vector3.zero;
        isFlying = true;
    }

    void HandleFlyingPhysics()
    {
        Vector3 direction = flyTarget - transform.position;
        float distance = direction.magnitude;

        if (distance < arrivalDistance)
        {
            isFlying = false;

            rb.linearVelocity = Vector3.zero;

            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.Warp(transform.position);

            shotsFired = 0;
            ChangeState(EnemyState.Shoot);
            return;
        }

        direction.Normalize();

        if (rb.linearVelocity.magnitude < maxFlySpeed)
        {
            rb.AddForce(direction * flyForce, ForceMode.Acceleration);
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            Time.fixedDeltaTime * bodyRotationSpeed
        ));
    }

    void ChangeState(EnemyState newState)
    {
        if (newState == EnemyState.Patrol)
        {
            agent.speed = patrolSpeed;
            agent.stoppingDistance = waypointReachingPoint;
            agent.isStopped = false;
            MoveToNextWaypoint();
        }

        if (newState == EnemyState.Chase)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = playerReachingPoint;
            agent.isStopped = false;
        }

        if (newState == EnemyState.Shoot)
        {
            agent.isStopped = true;
            shotTimer = 0f;
        }

        currentState = newState;
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(pewpewPrefab, firePoint.position, firePoint.rotation);

        Vector3 dir = (player.position - firePoint.position).normalized;
        bullet.transform.forward = dir;

        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
        if (rbBullet != null)
        {
            rbBullet.AddForce(dir * bulletForce, ForceMode.Impulse);
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * bodyRotationSpeed
            );
        }
    }

    void PatrolWaypoints()
    {
        if (waypoints.Length == 0)
            return;

        if (agent.remainingDistance <= waypointReachingPoint && !agent.pathPending)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            MoveToNextWaypoint();
        }
    }

    void MoveToNextWaypoint()
    {
        agent.SetDestination(waypoints[currentWaypoint].position);
    }

    Vector3 FindRepositionPosition()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 random = Random.insideUnitCircle.normalized * repositionRadius;
            Vector3 candidate = player.position + new Vector3(random.x, 0f, random.y);

            if (Vector3.Distance(candidate, player.position) < minDistanceFromPlayer)
                continue;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }

        return player.position - player.forward * minDistanceFromPlayer;
    }
}