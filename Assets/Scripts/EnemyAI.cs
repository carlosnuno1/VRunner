using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState {Idle, Chase, Attack}

    public EnemyState currentState = EnemyState.Idle;

    public NavMeshAgent agent;
    public Transform player;

    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    private float lastAttackTime;



void Start(){
    agent = GetComponent<NavMeshAgent>();
}

void Update(){
    float distance = Vector3.Distance(transform.position, player.position);

    switch (currentState){
        case EnemyState.Idle:
            if (distance <= detectionRange){
                currentState = EnemyState.Chase;
            }
            break;
        
        case EnemyState.Chase:
            MoveToPlayer();
            if (distance <= attackRange){
                currentState = EnemyState.Attack;
            }
            break;
        
        case EnemyState.Attack:
            AttackPlayer(distance);
            break;
    }

    void MoveToPlayer(){
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void AttackPlayer(float distance){
        agent.isStopped = true;

        if (Time.time > lastAttackTime + attackCooldown){
            int randomAttack = Random.Range(0,2);

            if (randomAttack == 0){
                Debug.Log("Enemy used Attack 1");
            }
            else if (randomAttack == 1){
                Debug.Log("Enemy used Attack 2");
            }
        }

        lastAttackTime = Time.time;
    }
    if (distance > attackRange){
        currentState = EnemyState.Chase;
    }
}
}