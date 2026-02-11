using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

    // Goal:
    /*
    Enemy patrols between waypoints
    */

    // Overall Plan:
    /*
    ----> Flys up and shoots (timed)? or after three shots?
        ----> Then it teleports to either way point or within the radius of the player. 
            ----> Then it shoots (loop)
                ----> When health is low, it explodes and if player is within radius, player kaboom too.
                    -----> FIN.
    */

    // OLD SCRIPT ////////////////////////////////////////////////////////////////////////////////////////

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

    private NavMeshAgent agent;
    private bool isChasing = false;
    private int whatWaypointIsBroTravelingTo = 0;

    // Nav Mesh T T?

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

    }

    void Update()
    {
        if (agent == null)
        {
            Debug.Log("Agent is null");
            return;

        }

        if (isChasing && Vector3.Distance(transform.position, player.position) < playerReachingPoint)
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

}
