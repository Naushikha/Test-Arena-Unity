using UnityEngine;
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
        owner.SFX_seen.Play();
        owner.animator.Play("base.run", 0, 0);
    }
    public void Update()
    {
        if (!owner.playerInSightRange) owner.stateMachine.ChangeState(new ragedState(owner));
        if (owner.playerInAttackRange) owner.stateMachine.ChangeState(new attackState(owner));
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
        owner.SFX_seen.Play();
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
    private Vector3 targetPos;
    private Vector3 unitDirection; // Unit vector to target

    private float t;
    private float timeToTarget;
    public void Enter()
    {
        Debug.Log("entering attack state");
        playerPos = owner.player.transform.position;
        playerPos.y = owner.ground.SampleHeight(playerPos) + owner.halfHeight; // Prevents jumping into mid-air when player's last position is on air.
        enemyPos = owner.transform.position;
        unitDirection = (playerPos - enemyPos).normalized;
        targetPos = (enemyPos + (unitDirection * owner.attackJumpDistance));
        targetPos.y = owner.ground.SampleHeight(targetPos) + owner.halfHeight;
        unitDirection = (targetPos - enemyPos).normalized;
        t = 0;
        owner.animator.Play("base.attack", 0, 0);
        owner.SFX_attack.Play();
        timeToTarget = owner.animator.GetCurrentAnimatorStateInfo(0).length * 0.6f; // 0.6 // This is because the animation at this point, stops moving
        owner.transform.LookAt(targetPos); // Look at target
    }
    public void Update()
    {
        float animTime = owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (animTime > 1)
        {
            if (Random.value >= 0.2)
            {
                // Go back to idle state after attacking
                owner.stateMachine.ChangeState(new idleState(owner));
            }
            else
            { // There's a chance he'll jump in frustration
                owner.stateMachine.ChangeState(new jumpState(owner));
            }
        }
        else if (animTime < 0.6) // This is because the animation at this point, stops moving
        { // Move towards the target
            t += Time.deltaTime / timeToTarget;
            owner.transform.position = Vector3.Lerp(enemyPos, targetPos, t);
        }
        if (owner.playerInDamageRange && (0.3 <= animTime && animTime <= 0.45))
        {
            // Reduce player health
            GameManager.Instance.playerHurt(2f);
        }
    }
    public void Exit()
    {
        Debug.Log("exiting attack state");
    }
}
public class jumpState : IState
{
    Intelligence owner;
    public jumpState(Intelligence owner) { this.owner = owner; }
    public void Enter()
    {
        Debug.Log("entering jump state");
        owner.animator.Play("base.jump", 0, 0);
    }
    public void Update()
    {
        // If jump animation is done go back to idling
        if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            owner.stateMachine.ChangeState(new idleState(owner));
        }
    }
    public void Exit()
    {
        Debug.Log("exiting jump state");
    }
}
public class hurtState : IState
{
    Intelligence owner;
    public hurtState(Intelligence owner) { this.owner = owner; }
    private bool wasEnraged;
    public void Enter()
    {
        Debug.Log("entering hurt state");
        owner.animator.Play("base.flinch", 0, 0);
        owner.SFX_hurt.Play();
        if (owner.stateMachine.GetPreviousState() is ragedState)
        {
            owner.rageMultiplier *= 1.3f; // Make him even angrier
            wasEnraged = true;
        }
    }
    public void Update()
    {
        // If hurt animation is done go back to idling OR RAGE!
        if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            if (wasEnraged)
            {
                owner.stateMachine.ChangeState(new ragedState(owner));

            }
            else
            {
                owner.stateMachine.ChangeState(new idleState(owner));

            }
        }
    }
    public void Exit()
    {
        Debug.Log("exiting hurt state");
    }
}
public class deadState : IState
{
    Intelligence owner;
    public deadState(Intelligence owner) { this.owner = owner; }

    private bool killCountSet = false;
    public void Enter()
    {
        Debug.Log("entering dead state");
        owner.animator.Play("base." + owner.deathAnim[Random.Range(0, owner.deathAnim.Length)], 0, 0);
        owner.SFX_die.Play();
    }
    public void Update()
    {
        if (!killCountSet)
        {
            if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
            {
                GameManager.Instance.alienKilled();
                killCountSet = true;
            }
        }
    }
    public void Exit() { }
}

public class Intelligence : MonoBehaviour
{
    public float sightRange = 10f, attackRange = 5f, damageRange = 2f, attackJumpDistance = 4f;
    public float moveSpeed = 2f;
    public float health = 100f;
    public float rageMultiplier = 2f;

    public float height = 2f;
    public Transform player;
    public Terrain ground;
    // SFX
    public AudioSource SFX_seen;
    public AudioSource SFX_hurt;
    public AudioSource SFX_attack;
    public AudioSource SFX_die;
    public AudioSource[] SFX_hit;

    public string[] deathAnim;

    public ParticleSystem bloodSplash;

    protected internal Animator animator;


    protected internal float halfHeight;
    protected internal bool playerInSightRange, playerInAttackRange, playerInDamageRange;
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
        playerInDamageRange = Vector3.Distance(transform.position, player.position) <= damageRange;
        stateMachine.Update();
    }

    public void takeDamage(float amount)
    {
        bloodSplash.Play();
        SFX_hit[Random.Range(0, SFX_hit.Length)].Play();
        // Do not take damage if dead
        if (stateMachine.GetCurrentState() is deadState)
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
            // If the player was not to be seen and this dude was just chillin', get enraged
            if (!playerInSightRange && (stateMachine.GetCurrentState() is idleState))
            {
                // Go to raged state
                stateMachine.ChangeState(new ragedState(this));
                return;
            }
            // Brutal difficulty! - if already hurt make him rage!
            if (stateMachine.GetCurrentState() is hurtState && Random.value >= 0.9)
            {
                stateMachine.ChangeState(new ragedState(this));
            }
            if (Random.value >= 0.7) // And if raged do not go into hurt state?
            {
                stateMachine.ChangeState(new hurtState(this));
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
    }

}
