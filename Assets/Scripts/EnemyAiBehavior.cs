using UnityEngine;
using UnityEngine.AI;

public class EnemyAIBehavior : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Shoot, Reposition }

    [Header("Waypoints")]
    public Transform[] waypoints;
    public Transform player;

    [Header("Detection")]
    public float detectionRadius = 15f;
    public float shootingDistance = 10f;

    [Header("Flying Settings")]
    public float flyHeight = 6f;
    public float flySpeed = 5f;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1f;
    public float bodyRotationSpeed = 5f;

    [Header("Shooting")]
    public GameObject pewpewPrefab;
    public Transform firePoint;
    public float timeBetweenShots = 1f;
    public int shotCount = 3;
    public float bulletForce = 40f;

    [Header("Reposition (Swoop)")]
    public float swoopRadius = 8f;
    public float swoopSpeedMultiplier = 1.5f;

    private EnemyState currentState = EnemyState.Patrol;
    private int shotsFired = 0;
    private float shotTimer = 0f;
    private int waypointIndex = 0;
    private Vector3 targetFlyPos;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Start by hovering above the first waypoint or current position
        targetFlyPos = transform.position;
        changeState(EnemyState.Patrol);
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrolState(distanceToPlayer);
                break;
            case EnemyState.Chase:
                UpdateChaseState(distanceToPlayer);
                break;
            case EnemyState.Shoot:
                UpdateShootingState(distanceToPlayer);
                break;
            case EnemyState.Reposition:
                UpdateRepositionState();
                break;
        }

        ApplyFlyingMovement();
        RotateTowards(player.position);
    }

    void ApplyFlyingMovement()
    {
        // Add a sine wave bobbing effect to the target position
        Vector3 finalPos = targetFlyPos;
        finalPos.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        float currentSpeed = (currentState == EnemyState.Reposition) ? flySpeed * swoopSpeedMultiplier : flySpeed;
        transform.position = Vector3.MoveTowards(transform.position, finalPos, currentSpeed * Time.deltaTime);
    }

    void UpdatePatrolState(float dist)
    {
        if (dist <= detectionRadius)
        {
            changeState(EnemyState.Chase);
            return;
        }

        if (waypoints.Length > 0)
        {
            targetFlyPos = waypoints[waypointIndex].position;
            if (Vector3.Distance(transform.position, targetFlyPos) < 1f)
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }
    }

    void UpdateChaseState(float dist)
    {
        if (dist <= shootingDistance)
        {
            changeState(EnemyState.Shoot);
            return;
        }

        // Target a spot above the player
        targetFlyPos = player.position + Vector3.up * flyHeight;
    }

    void UpdateShootingState(float dist)
    {
        if (dist > detectionRadius + 2f)
        {
            changeState(EnemyState.Patrol);
            return;
        }

        // Stay hovering above player while shooting
        targetFlyPos = player.position + Vector3.up * flyHeight;

        shotTimer -= Time.deltaTime;
        if (shotTimer <= 0f)
        {
            Shoot();
            shotTimer = timeBetweenShots;
            shotsFired++;

            if (shotsFired >= shotCount)
                changeState(EnemyState.Reposition);
        }
    }

    void UpdateRepositionState()
    {
        // Move to the random offset picked in changeState()
        if (Vector3.Distance(transform.position, targetFlyPos) < 1f)
        {
            shotsFired = 0;
            changeState(EnemyState.Shoot);
        }
    }

    void changeState(EnemyState newState)
    {
        currentState = newState;

        if (newState == EnemyState.Reposition)
        {
            // Pick a new random spot in the air around the player
            Vector2 randomCircle = Random.insideUnitCircle.normalized * swoopRadius;
            targetFlyPos = player.position + new Vector3(randomCircle.x, flyHeight, randomCircle.y);
        }
        else if (newState == EnemyState.Shoot)
        {
            shotTimer = timeBetweenShots;
        }
    }

    void Shoot()
    {
        if (pewpewPrefab && firePoint)
        {
            GameObject bullet = Instantiate(pewpewPrefab, firePoint.position, Quaternion.identity);
            Vector3 dir = (player.position - firePoint.position).normalized;
            bullet.GetComponent<Rigidbody>()?.AddForce(dir * bulletForce, ForceMode.Impulse);
        }
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * bodyRotationSpeed);
        }
    }
}