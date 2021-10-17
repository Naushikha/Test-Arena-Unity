using UnityEngine;

namespace Warper
{
    public class WarperAI : MonoBehaviour
    {
        public float sightRange = 10f, attackRange = 5f, damageRange = 2f, attackJumpDistance = 4f;
        public float moveSpeed = 2f;
        public float health = 100f;
        public float rageMultiplier = 2f;
        public Transform player;
        public Terrain ground;
        public ParticleSystem bloodEffect;
        protected internal Animator animator;
        protected internal float animFadeIn = 0.2f;

        protected internal bool playerInSightRange, playerInAttackRange, playerInDamageRange;
        protected internal StateMachine stateMachine = new StateMachine();

        private void Start()
        {
            animator = GetComponent<Animator>();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            ground = GameObject.FindGameObjectWithTag("Ground").GetComponent<Terrain>();
            stateMachine.ChangeState(new idleState(this));
            // Check difficulty, if defined
            if (GameManager.Instance)
            {
                // Get difficulty from game
                float difficulty = GameManager.Instance.difficulty;
                sightRange *= difficulty;
                attackRange *= difficulty;
                damageRange *= difficulty;
                attackJumpDistance *= difficulty;
                moveSpeed *= difficulty;
                health *= difficulty;
                rageMultiplier *= difficulty;
            }
        }

        private void Update()
        {
            float tmpPlayerDist = Vector3.Distance(transform.position, player.position);
            playerInSightRange = tmpPlayerDist <= sightRange;
            playerInAttackRange = tmpPlayerDist <= attackRange;
            playerInDamageRange = tmpPlayerDist <= damageRange;
            stateMachine.Update();
        }

        public void takeDamage(hitData dData)
        {
            GameObject bloodGO = Instantiate(bloodEffect.gameObject, dData.hit.point, Quaternion.LookRotation(dData.hit.normal));
            Destroy(bloodGO, 2f);
            GetComponent<WarperSFX>().shot();
            // Do not take damage if dead
            if (stateMachine.GetCurrentState() is deadState) return;
            if (health <= 0) { Die(); return; }
            else
            {
                health -= dData.damage;
                // If the player was not to be seen and this dude was just chillin', get enraged
                if (!playerInSightRange && (stateMachine.GetCurrentState() is idleState))
                {
                    // Go to raged state
                    stateMachine.ChangeState(new ragedState(this));
                    return;
                }
                // Brutal difficulty! - if already hurt make him rage!
                if (stateMachine.GetCurrentState() is hurtState && Random.value >= 0.5)
                {
                    stateMachine.ChangeState(new ragedState(this));
                }
                if (Random.value >= 0.7 && !(stateMachine.GetCurrentState() is ragedState)) // And if raged do not go into hurt state?
                {
                    stateMachine.ChangeState(new hurtState(this));
                }
            }
        }
        private void Die()
        {
            stateMachine.ChangeState(new deadState(this));
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

    }

    public class idleState : IState
    {
        WarperAI owner;
        public idleState(WarperAI owner) { this.owner = owner; }
        public void Enter() { owner.animator.Play("idle"); }
        public void Update()
        {
            if (owner.playerInSightRange && !owner.playerInAttackRange) owner.stateMachine.ChangeState(new chaseState(owner));
            if (owner.playerInAttackRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new attackState(owner));
        }
        public void Exit() { }
    }
    public class chaseState : IState
    {
        WarperAI owner;
        public chaseState(WarperAI owner) { this.owner = owner; }
        public void Enter()
        {
            owner.GetComponent<WarperSFX>().seen();
            owner.animator.Play("run");
        }
        public void Update()
        {
            if (!owner.playerInSightRange) owner.stateMachine.ChangeState(new ragedState(owner));
            if (owner.playerInAttackRange) owner.stateMachine.ChangeState(new attackState(owner));
            owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
            owner.transform.position += owner.transform.forward * owner.moveSpeed * Time.deltaTime;
        }
        public void Exit() { }
    }
    public class ragedState : IState
    {
        WarperAI owner;
        public ragedState(WarperAI owner) { this.owner = owner; }
        public void Enter()
        {
            owner.GetComponent<WarperSFX>().seen();
            owner.animator.Play("run_fast");
        }
        public void Update()
        {
            if (owner.playerInAttackRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new attackState(owner));
            owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
            // Move faster if in rage!
            owner.transform.position += owner.transform.forward * owner.moveSpeed * owner.rageMultiplier * Time.deltaTime;
        }
        public void Exit() { }
    }
    public class attackState : IState
    {
        WarperAI owner;
        public attackState(WarperAI owner) { this.owner = owner; }

        private Vector3 playerPos;
        private Vector3 enemyPos;
        private Vector3 targetPos;
        private Vector3 unitDirection; // Unit vector to target

        private float t;
        private float timeToTarget;
        public void Enter()
        {
            playerPos = owner.player.transform.position;
            playerPos.y = owner.ground.SampleHeight(playerPos); // Prevents jumping into mid-air when player's last position is on air.
            enemyPos = owner.transform.position;
            unitDirection = (playerPos - enemyPos).normalized;
            targetPos = (enemyPos + (unitDirection * owner.attackJumpDistance));
            targetPos.y = owner.ground.SampleHeight(targetPos);
            unitDirection = (targetPos - enemyPos).normalized;
            t = 0;
            owner.animator.Play("attack");
            owner.GetComponent<WarperSFX>().attack();
            timeToTarget = owner.animator.GetCurrentAnimatorStateInfo(0).length * 0.6f; // 0.6 // This is because the animation at this point, stops moving
            owner.transform.LookAt(targetPos); // Look at target
        }
        public void Update()
        {
            float animTime = owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (animTime > 1)
            {
                if (Random.value >= 0.0) // Put 0.2 for minimal effect
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
                LevelManager.Instance.playerHurt(2f);
            }
        }
        public void Exit() { }
    }
    public class jumpState : IState
    {
        WarperAI owner;
        public jumpState(WarperAI owner) { this.owner = owner; }
        public void Enter() { owner.animator.Play("jump"); }
        public void Update()
        {
            // If jump animation is done go back to idling
            if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
            {
                owner.stateMachine.ChangeState(new idleState(owner));
            }
        }
        public void Exit() { }
    }
    public class hurtState : IState
    {
        WarperAI owner;
        public hurtState(WarperAI owner) { this.owner = owner; }
        private bool wasEnraged;
        public void Enter()
        {
            owner.animator.Play("flinch");
            owner.GetComponent<WarperSFX>().flinch();
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
        public void Exit() { }
    }
    public class deadState : IState
    {
        WarperAI owner;
        public deadState(WarperAI owner) { this.owner = owner; }

        private bool killCountSet = false;
        public void Enter()
        {
            owner.animator.Play($"die{Random.Range(1, owner.animator.GetInteger("dieAnimNumber") + 1)}");
            owner.GetComponent<WarperSFX>().die();
        }
        public void Update()
        {
            if (!killCountSet)
            {
                if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
                {
                    LevelManager.Instance.alienKilled();
                    killCountSet = true;
                }
            }
        }
        public void Exit() { }
    }
}
