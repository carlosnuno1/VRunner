using UnityEngine;
using UnityEngine.AI;

public class EnemyAIBehavior : MonoBehaviour
{

    // Goal:
    /*
    Enemy patrols between waypoints, shoots at player 3 times, teleport, loop then boom
    */

    // Overall Plan:
    /*
    ----> Shoot three shots
        ----> Then it teleports within the radius of the player. 
            ----> Then it shoots again (loop)
                ----> When health is low, it explodes and if player is within radius, player kaboom too.
                    -----> EL FIN.

    --> Maybe no kaboom??
    */

    // Notes:
    /*
    Need improvements. enemy stops chasing so I gotta fix that. Aiming to finish by sat or sun
    wire up shooting/teleporting
    */

    public enum EnemyState
    {
        Patrol,
        Chase,
        Shoot,
        Teleport,
    }

    [Header("Waypoints")]
    public Transform[] waypoints;
    public Transform player;

    [Header("Detection")]
    public float detectionRadius = 10f;
    public float fieldOfView = 90f;
    public float playerReachingPoint = 1f;

    [Header("Movement")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 5.5f;
    public float waypointReachingPoint = 0.5f;

    [Header("Shooting")]
    public GameObject pewpewPrefab;
    public Transform firePoint;
    public float shootCooldown = 1f;
    public float timeBetweenShots = 1f;
    public int shotCount = 3;
    public float shootingDistance = 8f;
    public float bulletForce = 400f;

    [Header("Teleport")]
    public GameObject teleportVFX;
    public float teleportWithinPlayerRadius = 5f;
    public float minTeleportDistanceFromPlayer = 2f;
    public float teleportCooldown = 5f;
    public float teleportDuration = 1f;

    private EnemyState currentState = EnemyState.Patrol;
    private int shotsFired = 0;
    private float shotTimer = 0f;
    private float teleportTimer = 0f;
    private NavMeshAgent agent;
    private bool isChasing = false;
    private int whatWaypointIsBroTravelingTo = 0;
    private float buffer = 2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent is not found");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player is not found");
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player != null)
            {
                Debug.Log("Player found with tag.");
            }
            else
            {
                Debug.LogError("Player still not found, the tags were checked");
            }
            return;
        }

        if (firePoint == null)
        {
            firePoint = transform.Find("Firepoint");
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePoint = firePointObj.transform;
                firePoint.SetParent(transform);
                firePoint.localPosition = new Vector3(0, 1, 0);
                Debug.Log("Dale Fireball created");
            }
        }

        agent.speed = patrolSpeed;
        agent.stoppingDistance = waypointReachingPoint;
        if (waypoints.Length > 0)
        {
            moveTowardsNextWaypoint();
            return;
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (agent == null)
        {
            Debug.Log("Agent is null");
            return;

        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                updatePatrolState(distanceToPlayer);
                break;
            case EnemyState.Chase:
                updateChaseState(distanceToPlayer);
                break;
            case EnemyState.Shoot:
                updateShootingState(distanceToPlayer);
                break;
            case EnemyState.Teleport:
                updateTeleportState();
                break;
        }
    }

    void moveTowardsNextWaypoint()
    {
        if (waypoints.Length > 0 && agent != null && agent.isActiveAndEnabled)
        {
            agent.SetDestination(waypoints[whatWaypointIsBroTravelingTo].position);
        }
        return;

    }

    bool canSeeThePlayer()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < detectionRadius)
        {
            Debug.Log("Player's cheeks are about to get clapped");
            return true;
        }

        return false;

    }

    void startChasing()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = playerReachingPoint;

        chasingPlayer();

    }

    void chasingPlayer()
    {
        if (agent.isActiveAndEnabled && player != null)
        {
            agent.SetDestination(player.position);
            agent.stoppingDistance = playerReachingPoint;
            Debug.Log($"Player position: {player.position}");
        }
    }
    void stopChasing()
    {
        isChasing = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = waypointReachingPoint;

        if (waypoints.Length > 0)
        {
            moveTowardsNextWaypoint();
        }
    }

    void patrolBetweenWaypoints()
    {
        if (waypoints.Length == 0) return;

        if (agent.remainingDistance <= waypointReachingPoint && !agent.pathPending)
        {
            whatWaypointIsBroTravelingTo = (whatWaypointIsBroTravelingTo + 1) % waypoints.Length;
            moveTowardsNextWaypoint();
        }

    }

        void updatePatrolState(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRadius && canSeeThePlayer())
        {
            changeState(EnemyState.Chase);
        }
        else
        {
            patrolBetweenWaypoints();
        }
    }

    void updateChaseState(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRadius * buffer)
        {
            changeState(EnemyState.Patrol);
            return;
        }

        if (distanceToPlayer <= shootingDistance)
        {
            changeState(EnemyState.Shoot);
            return;
        }

        chasingPlayer();
    }

    void updateShootingState(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRadius + buffer)
        {
            changeState(EnemyState.Patrol);
            return;
        }

        if (distanceToPlayer > shootingDistance + buffer)
        {
            changeState(EnemyState.Chase);
            return;
        }

        agent.isStopped = true;
        rotateTowardsPlayer();

        shotTimer -= Time.deltaTime;

        if (shotTimer <= 0f)
        {
            Shoot();
            shotTimer = timeBetweenShots;
            shotsFired++;

            if (shotsFired >= shotCount)
            {
                // shotsFired = 0;
                changeState(EnemyState.Teleport);
            }
        }
    }

    void updateTeleportState()
    {
        teleportTimer -= Time.deltaTime;

        if(teleportTimer <= 0f)
        {
            Teleport();
            shotsFired = 0;
            changeState(EnemyState.Shoot);
        }
    }

    void changeState(EnemyState newState)
    {
        switch (currentState)
        {
            case EnemyState.Shoot:
                agent.isStopped = false;
                break;
        }

        switch (newState)
        {
            case EnemyState.Patrol:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = waypointReachingPoint;
                agent.isStopped = false;
                moveTowardsNextWaypoint();
                break;

            case EnemyState.Chase:
                agent.speed = chaseSpeed;
                agent.stoppingDistance = playerReachingPoint;
                agent.isStopped = false;
                break;

            case EnemyState.Shoot:
                agent.isStopped = true;
                shotTimer = 0f;
                break;

            case EnemyState.Teleport:
                agent.isStopped = true;
                teleportTimer = teleportDuration;
                break;
        }
        currentState = newState;
    }

        void Shoot()
    {
        if (pewpewPrefab == null)
        {
            Debug.LogError("Pewpew prefab is not assigned");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned");
            return;
        }

        GameObject bullet = Instantiate(pewpewPrefab, firePoint.position, firePoint.rotation);
        Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
        bullet.transform.forward = directionToPlayer;

        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
        if (rbBullet != null)
        {
            rbBullet.AddForce(directionToPlayer * bulletForce);
        }
        Debug.Log("Its shooting");
    }

    void rotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * buffer);
        }
    }

    void Teleport()
    {
        Vector3 newPosition = FindTeleportPosition();
        if (newPosition == Vector3.zero)
        {
            Debug.Log("Couldn't find teleportation spot around player");
            changeState(EnemyState.Shoot);
            return;
        }

        if (teleportVFX != null)
        {
            Instantiate(teleportVFX, transform.position, Quaternion.identity);
        }

        agent.enabled = false;
        transform.position = newPosition;
        agent.enabled = true;

        if(teleportVFX != null)
        {
            Instantiate(teleportVFX, transform.position, Quaternion.identity);
        }

        rotateTowardsPlayer();

        // teleportTimer = teleportDuration;
        Debug.Log("Enemy teleported to:" + newPosition);
    }


    Vector3 FindTeleportPosition()
    {
        for(int i = 0; i < 20; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * teleportWithinPlayerRadius;
            Vector3 targetPos = player.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (Vector3.Distance(targetPos, player.position) < minTeleportDistanceFromPlayer)
                continue;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, buffer, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        Vector3 behindPlayer = player.position - player.forward * minTeleportDistanceFromPlayer;
        if (NavMesh.SamplePosition(behindPlayer, out NavMeshHit fallbackHit, 2f, NavMesh.AllAreas))
        {
            return fallbackHit.position;
        }

        return Vector3.zero;
    }
}
