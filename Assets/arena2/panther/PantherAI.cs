using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Panther
{
    public class PantherAI : MonoBehaviour
    {
        public float sightRange = 20f, meleeRange = 2f;
        public float idleTime = 5.0f, patrolTime = 10.0f;
        public float health = 100f;
        protected internal Transform player;
        protected internal Terrain ground;
        protected internal Animator animator;
        public ParticleSystem bloodEffect;
        protected internal NavMeshAgent agent;
        protected internal PantherSFX sfx;
        public string waypointsName = "waypoints";
        public bool randomPatrolNavigation = true;
        public float randomPatrolRadius = 25f;
        protected internal List<Transform> waypoints = new List<Transform>();
        protected internal float animFadeIn = 0.5f;
        protected internal bool playerInSightRange, playerInMeleeRange, playerInDamageRange;
        protected internal StateMachine stateMachine = new StateMachine();
        private void Start()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            sfx = GetComponent<PantherSFX>();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            ground = GameObject.FindGameObjectWithTag("Ground").GetComponent<Terrain>();
            if (!player || !ground) Debug.LogError("Player or Ground is not set for Panther!");
            // Transform waypointsObject = GameObject.FindGameObjectWithTag(waypointsName).transform;
            // foreach (Transform t in waypointsObject) { waypoints.Add(t); }
            stateMachine.ChangeState(new idleState(this));
        }
        private void Update()
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            playerInSightRange = distToPlayer <= sightRange;
            playerInMeleeRange = distToPlayer <= meleeRange;
            playerInDamageRange = distToPlayer <= meleeRange + 1; // This is to give the damage a bit of a range when he swings limbs
            stateMachine.Update();
        }
        public Vector3 getRandomWaypoint() { return waypoints[Random.Range(0, waypoints.Count)].position; }
        // https://answers.unity.com/questions/475066/how-to-get-a-random-point-on-navmesh.html
        public Vector3 getRandomPatrolLocation()
        {
            Vector3 randomDirection = Random.insideUnitSphere * randomPatrolRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, randomPatrolRadius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }
        public void takeDamage(hitData dData)
        {
            GameObject bloodGO = Instantiate(bloodEffect.gameObject, dData.hit.point, Quaternion.LookRotation(dData.hit.normal));
            Destroy(bloodGO, 2f);
            if (dData.type == hitType.knife) sfx.stabbed();
            else sfx.shot();
            // Do not take damage if dead
            if (stateMachine.GetCurrentState() is deadState) return;
            if (health <= 0) { Die(); return; }
            else
            {
                health -= dData.damage;
                // If the player was not to be seen and this dude was just chillin', start chasing
                if (!playerInSightRange && (stateMachine.GetCurrentState() is idleState || stateMachine.GetCurrentState() is patrolState))
                {
                    // Go to chase state
                    stateMachine.ChangeState(new chaseState(this));
                    return;
                }
            }
        }
        private void Die() { stateMachine.ChangeState(new deadState(this)); }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }
    }

    public class idleState : IState
    {
        PantherAI owner;
        public idleState(PantherAI owner) { this.owner = owner; }
        private float timer = 0;
        public void Enter() { owner.animator.CrossFade("idle", owner.animFadeIn); }
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer > owner.idleTime) owner.stateMachine.ChangeState(new patrolState(owner));
            if (owner.playerInSightRange) owner.stateMachine.ChangeState(new chaseState(owner));
            if (owner.playerInMeleeRange) owner.stateMachine.ChangeState(new attackState(owner));
            // if (owner.playerInSightRange && !owner.playerInFireRange) owner.stateMachine.ChangeState(new chaseState(owner));
        }
        public void Exit() { }
    }
    public class patrolState : IState
    {
        PantherAI owner;
        public patrolState(PantherAI owner) { this.owner = owner; }
        private float timer = 0;
        public void Enter()
        {
            if (owner.randomPatrolNavigation) owner.agent.SetDestination(owner.getRandomPatrolLocation());
            else owner.agent.SetDestination(owner.getRandomWaypoint());
            owner.animator.CrossFade("walk", owner.animFadeIn);
        }
        public void Update()
        {
            if (owner.agent.remainingDistance <= owner.agent.stoppingDistance)
            {
                if (owner.randomPatrolNavigation) owner.agent.SetDestination(owner.getRandomPatrolLocation());
                else owner.agent.SetDestination(owner.getRandomWaypoint());
            }
            timer += Time.deltaTime;
            if (timer > owner.patrolTime) owner.stateMachine.ChangeState(new idleState(owner));
            if (owner.playerInSightRange) owner.stateMachine.ChangeState(new chaseState(owner));
        }
        public void Exit() { owner.agent.SetDestination(owner.agent.transform.position); }
    }
    public class chaseState : IState
    {
        PantherAI owner;
        public chaseState(PantherAI owner) { this.owner = owner; }
        private float prevSpeed;
        public void Enter()
        {
            prevSpeed = owner.agent.speed;
            owner.agent.speed = prevSpeed * 8;
            owner.animator.CrossFade("run", owner.animFadeIn);
            owner.sfx.seen();
        }
        public void Update()
        {
            owner.agent.SetDestination(owner.player.position);
            if (owner.playerInMeleeRange) owner.stateMachine.ChangeState(new attackState(owner));
        }
        public void Exit() { owner.agent.speed = prevSpeed; owner.agent.SetDestination(owner.agent.transform.position); }
    }

    public class attackState : IState
    {
        PantherAI owner;
        public attackState(PantherAI owner) { this.owner = owner; }
        public void Enter() { owner.animator.Play("attack"); }
        public void Update()
        {
            owner.transform.LookAt(owner.player.position); // Look at player
            float animTime = owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (animTime > 1)
            {
                owner.stateMachine.ChangeState(new idleState(owner));
            }
            if (owner.playerInDamageRange && (0.3 <= animTime && animTime <= 0.6))
            {
                // Reduce player health
                LevelManager.Instance.playerHurt(2f);
            }
        }
        public void Exit() { }
    }

    public class deadState : IState
    {
        PantherAI owner;
        public deadState(PantherAI owner) { this.owner = owner; }
        private bool killCountSet = false;
        public void Enter()
        {
            owner.animator.Play($"die{Random.Range(1, 3 + 1)}");
            owner.sfx.die();
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
