using UnityEngine;
using UnityEngine.AI;

// public class Intelligence : MonoBehaviour
// {
//     public Transform player;
//     public LayerMask playerMask;
//     public LayerMask groundMask;
//     public float moveSpeed = 2f;

//     // Patrolling
//     public float walkPointRange;
//     Vector3 walkPoint;
//     private bool walkPointSet;

//     // Attacking
//     public float timeBetweenAttacks;
//     private bool alreadyAttacked;

//     // States
//     public float sightRange, attackRange;
//     private bool playerInSightRange, playerInAttackRange;

//     public Animator animator;

//     private void Update()
//     {
//         // Check for sight and attack range
//         playerInSightRange = Vector3.Distance(transform.position, player.position) <= sightRange;
//         playerInAttackRange = Vector3.Distance(transform.position, player.position) <= attackRange;

//         if (!playerInSightRange && !playerInAttackRange) Patroling();
//         if (playerInSightRange && !playerInAttackRange) ChasePlayer();
//         if (playerInAttackRange && playerInSightRange) AttackPlayer();
//     }
//     private void Patroling()
//     {
//         Debug.Log("Patrol");
//         // if (!walkPointSet) SearchWalkPoint();
//         // if (walkPointSet) agent.SetDestination(walkPoint);

//         // Vector3 distanceToWalkPoint = transform.position - walkPoint;

//         // // Walkpoint reached
//         // if (distanceToWalkPoint.magnitude < 1f)
//         // {
//         //     walkPointSet = false;
//         // }
//     }
//     private void SearchWalkPoint()
//     {
//         // Calculate random point in range
//         float randomZ = Random.Range(-walkPointRange, walkPointRange);
//         float randomX = Random.Range(-walkPointRange, walkPointRange);

//         walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

//         if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
//         {
//             walkPointSet = true;
//         }
//     }
//     private void ChasePlayer()
//     {
//         // transform.LookAt(player);
//         // Vector3 playerTerrainPos = new Vector3(player.position.x, transform.position.y, player.position.z);
//         transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
//         transform.position += transform.forward * moveSpeed * Time.deltaTime;
//     }
//     private void AttackPlayer()
//     {
//         // Make sure enemy doesn't move
//         // agent.SetDestination(transform.position);

//         if (!alreadyAttacked)
//         {
//             // Attack code here
//             animator.Play("Base Layer.attack", 0, 0);
//             Debug.Log("Attacking player");


//             alreadyAttacked = true;
//             Invoke(nameof(ResetAttack), timeBetweenAttacks);
//         }
//     }
//     private void ResetAttack()
//     {
//         alreadyAttacked = false;
//     }

//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, attackRange);
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, sightRange);
//     }
// }


public class idleState : IState
{
    Intelligence owner;
    public idleState(Intelligence owner) { this.owner = owner; }
    public void Enter()
    {
        Debug.Log("entering idle state");
        owner.animator.Play("Base Layer.idle", 0, 0);
    }
    public void Update()
    {
        // if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (owner.playerInSightRange && !owner.playerInAttackRange) owner.stateMachine.ChangeState(new chaseState(owner));
        if (owner.playerInAttackRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new attackState(owner));
    }
    public void Exit()
    {
        Debug.Log("exiting idle state");
    }
}
public class chaseState : IState
{
    Intelligence owner;
    public chaseState(Intelligence owner) { this.owner = owner; }
    public void Enter()
    {
        Debug.Log("entering chase state");
        owner.animator.Play("Base Layer.run", 0, 0);
    }
    public void Update()
    {
        // if (!owner.playerInSightRange && !owner.playerInAttackRange) owner.stateMachine.ChangeState(new idleState(owner));
        if (owner.playerInAttackRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new attackState(owner));
        owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
        if (owner.inRage) // Move faster if in rage!
        {
            owner.transform.position += owner.transform.forward * owner.moveSpeed * owner.rageMultiplier * Time.deltaTime;
        }
        else
        {
            owner.transform.position += owner.transform.forward * owner.moveSpeed * Time.deltaTime;
        }
        owner.setOnGround();
    }
    public void Exit()
    {
        Debug.Log("exiting chase state");
    }
}
public class attackState : IState
{
    Intelligence owner;
    public attackState(Intelligence owner) { this.owner = owner; }
    public void Enter()
    {
        Debug.Log("entering attack state");
        owner.animator.Play("Base Layer.attack", 0, 0);
    }
    public void Update()
    {
        if (!owner.playerInSightRange && !owner.playerInAttackRange) owner.stateMachine.ChangeState(new idleState(owner));
        if (!owner.playerInAttackRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new chaseState(owner));
    }
    public void Exit()
    {
        Debug.Log("exiting attack state");
    }
}

public class Intelligence : MonoBehaviour
{
    public float sightRange = 10f, attackRange = 5f, rageRange = 20f;
    public float moveSpeed = 2f;
    public float health = 100f;
    public float timeTillNextAttack = 100f;
    public bool inRage = false;
    public float rageMultiplier = 2f;

    public float height = 2f;
    public Transform player;
    public Terrain ground;
    public Animator animator;


    protected internal float halfHeight;
    protected internal bool playerInSightRange, playerInAttackRange;
    protected internal StateMachine stateMachine = new StateMachine();

    private void Start()
    {
        // Calculate and set half the height of enemy
        halfHeight = height / 2;
        setOnGround(); // Set him on the ground
        stateMachine.ChangeState(new idleState(this));
    }

    private void Update()
    {
        playerInSightRange = Vector3.Distance(transform.position, player.position) <= sightRange;
        playerInAttackRange = Vector3.Distance(transform.position, player.position) <= attackRange;
        inRage = Vector3.Distance(transform.position, player.position) <= rageRange;
        stateMachine.Update();
    }
    protected internal void setOnGround()
    {
        float terrainY;
        terrainY = ground.SampleHeight(transform.position);
        transform.position = new Vector3(transform.position.x, terrainY, transform.position.z);
        // Also set the angle respective to the terrain if possible
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, sightRange * rageMultiplier);
    }

}
