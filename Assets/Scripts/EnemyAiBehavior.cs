using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

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
    private GameObject pewpewPrefab;
    private Transform firePoint;
    private float shootCooldown = 1f;
    private float timeBetweenShots = 1f;
    private int shotCount;

    [Header("Teleport")]
    private float teleportWithinPlayerRadius = 5f;
    private float minTeleportDistanceFromPlayer = 2f;
    private float teleportCooldown = 5f;

    private EnemyState currentState = EnemyState.Patrol;
    private int shotsFired = 0;
    private float shotTimer = 0f;
    private float teleportTimer = 0f;
    private NavMeshAgent agent;
    private bool isChasing = false;
    private int whatWaypointIsBroTravelingTo = 0;
    private float buffer = 2f;
    private float bulletSpeed = 400f;

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

        agent.speed = patrolSpeed;
        agent.stoppingDistance = waypointReachingPoint;
        if (waypoints.Length > 0)
        {
            moveTowardsNextWaypoint();
            return;
        }

        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePoint = firePointObj.transform;
                firePoint.SetParent(transform);
                firePoint.localPosition = new Vector3(0, 1, 0);
                Debug.Log("Dale Fireball created");
            }
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


        /*
        switch (currentState)
        {
            case EnemyState.Patrol:
                if (canSeeThePlayer())
                {
                    startChasing();
                }
                else
                {
                    patrolBetweenWaypoints();
                }
                break;
        }
        */

        /*
        if (isChasing && distanceToPlayer < playerReachingPoint)
        {
            moveTowardsNextWaypoint();
        }

        if (canSeeThePlayer())
        {
            if (!isChasing)
            {
                startChasing();
            }
            else
            {
                chasingPlayer();
            }
        }
        else
        {
            if (isChasing)
            {
                stopChasing();
            }
            else
            {
                patrolBetweenWaypoints();
            }
        }
        */
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
            Debug.Log($"Set designination to player: {player.position}");
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
        if (waypoints.Length > 0 && agent != null && agent.isActiveAndEnabled)
        {
            agent.SetDestination(waypoints[whatWaypointIsBroTravelingTo].position);
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

    void updateChaseState()
    {
        if (distanceToPlayer > detectionRadius)
        {
            changeState(EnemyState.Patrol);
        }
        else
        {
            chasingPlayer();
        }
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
        // Need to add that shoot rotation

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
        // blah
    }

    void changeState()
    {
        // blah
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

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        bullet.transform.forward = directionToPlayer;

        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
        if (rbBullet != null)
        {
            rbBullet.AddForce(directionToPlayer * bulletSpeed);
        }
        Debug.Log("Its shooting");
    }

    void rotateTowardsPlayer()
    {

    }


}
