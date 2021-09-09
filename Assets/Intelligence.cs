using UnityEngine;
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
//             animator.Play("base.attack", 0, 0);
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
        owner.animator.Play("base.idle", 0, 0);
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
        owner.animator.Play("base.run", 0, 0);
    }
    public void Update()
    {
        // if (!owner.playerInSightRange && !owner.playerInAttackRange) owner.stateMachine.ChangeState(new idleState(owner));
        if (owner.playerInAttackRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new attackState(owner));
        owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
        owner.transform.position += owner.transform.forward * owner.moveSpeed * Time.deltaTime;
        owner.setOnGround();
    }
    public void Exit()
    {
        Debug.Log("exiting chase state");
    }
}
public class ragedState : IState
{
    Intelligence owner;
    public ragedState(Intelligence owner) { this.owner = owner; }
    public void Enter()
    {
        Debug.Log("entering raged state");
        owner.animator.Play("base.run_fast", 0, 0);
    }
    public void Update()
    {
        // if (!owner.playerInSightRange && !owner.playerInAttackRange) owner.stateMachine.ChangeState(new idleState(owner));
        if (owner.playerInAttackRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new attackState(owner));
        owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
        // Move faster if in rage!
        owner.transform.position += owner.transform.forward * owner.moveSpeed * owner.rageMultiplier * Time.deltaTime;
        owner.setOnGround();
    }
    public void Exit()
    {
        Debug.Log("exiting raged state");
    }
}
public class attackState : IState
{
    Intelligence owner;
    public attackState(Intelligence owner) { this.owner = owner; }

    private Vector3 playerPos;
    private Vector3 enemyPos;
    private Vector3 unitDirection; // Unit vector to target
    private float timeToReach;
    private float t = 0;
    public void Enter()
    {
        Debug.Log("entering attack state");
        playerPos = owner.player.transform.position;
        playerPos.y = owner.ground.SampleHeight(playerPos) + owner.halfHeight; // Prevents jumping into mid-air when player's last position is on air.
        enemyPos = owner.transform.position;
        unitDirection = (playerPos - enemyPos).normalized;
        owner.animator.Play("base.attack", 0, 0);
        owner.transform.LookAt(playerPos); // Look at target
        timeToReach = owner.animator.GetCurrentAnimatorStateInfo(0).length;
    }
    public void Update()
    {
        if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            // Go back to idle state after attacking
            owner.stateMachine.ChangeState(new idleState(owner));
        }
        else if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.6) // This is because the animation at this point, stops moving
        { // Move towards the target
            owner.transform.position += unitDirection * owner.moveSpeed * 4 * Time.deltaTime;
        }
    }
    public void Exit()
    {
        Debug.Log("exiting attack state");
    }
}
public class hurtState : IState
{
    Intelligence owner;
    public hurtState(Intelligence owner) { this.owner = owner; }
    public void Enter()
    {
        Debug.Log("entering hurt state");
        owner.animator.Play("base.flinch", 0, 0);
    }
    public void Update()
    {
        // If hurt animation is done go back to idling
        if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            owner.stateMachine.ChangeState(new idleState(owner));
        }
    }
    public void Exit()
    {
        Debug.Log("exiting hurt state");
        // owner.Destroy(owner);
    }
}
public class deadState : IState
{
    Intelligence owner;
    public deadState(Intelligence owner) { this.owner = owner; }
    public void Enter()
    {
        Debug.Log("entering dead state");
        owner.animator.Play("base.die_spin", 0, 0);
    }
    public void Update()
    {
        // Something here 
        // Fancy animations should go here
    }
    public void Exit()
    {
        Debug.Log("exiting dead state");
        // owner.Destroy(owner);
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
    protected internal Animator animator;


    protected internal float halfHeight;
    protected internal bool playerInSightRange, playerInAttackRange;
    protected internal StateMachine stateMachine = new StateMachine();

    private void Start()
    {
        animator = GetComponent<Animator>();
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

    public void takeDamage(float amount)
    {
        // Do not take damage if dead
        if (stateMachine.GetState() is deadState)
        {
            return;
        }
        if (health <= 0)
        {
            Die();
            return;
        }
        else
        {
            health -= amount;
            if (Random.value >= 0.7)
            {
                stateMachine.ChangeState(new hurtState(this));
            }
            // If the player was not to be seen, get enraged
            if (!playerInSightRange)
            {
                // Go to raged state
                stateMachine.ChangeState(new ragedState(this));
            }
        }
    }
    private void Die()
    {
        stateMachine.ChangeState(new deadState(this));
    }
    protected internal void setOnGround()
    {
        float terrainY;
        terrainY = ground.SampleHeight(transform.position) + halfHeight;
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
