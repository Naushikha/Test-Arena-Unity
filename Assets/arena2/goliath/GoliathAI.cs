using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Goliath
{
    public class GoliathAI : MonoBehaviour
    {
        public float sightRange = 20f, fireRange = 15f, meleeRange = 2f;
        public float idleTime = 5.0f, patrolTime = 10.0f;
        public float fireChargeTime = 1.0f, fireRate = 15f, fireTime = 5.0f, fireReachSpeed = 5.0f;
        public float health = 100f;
        public ParticleSystem impactEffect;
        public ParticleSystem fireRight;
        public ParticleSystem fireLeft;
        protected internal Transform player;
        protected internal Terrain ground;
        protected internal Animator animator;
        protected internal NavMeshAgent agent;
        protected internal GoliathSFX sfx;
        protected internal string waypointsName = "waypoints";
        protected internal List<Transform> waypoints = new List<Transform>();
        protected internal float animFadeIn = 0.5f;
        protected internal bool playerInSightRange, playerInFireRange, playerInMeleeRange;
        protected internal StateMachine stateMachine = new StateMachine();
        private void Start()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            sfx = GetComponent<GoliathSFX>();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            ground = GameObject.FindGameObjectWithTag("Ground").GetComponent<Terrain>();
            if (!player || !ground) Debug.LogError("Player or Ground is not set for Goliath!");
            Transform waypointsObject = GameObject.FindGameObjectWithTag(waypointsName).transform;
            foreach (Transform t in waypointsObject) { waypoints.Add(t); }
            stateMachine.ChangeState(new idleState(this));
        }
        private void Update()
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            playerInSightRange = distToPlayer <= sightRange;
            playerInFireRange = distToPlayer <= fireRange;
            playerInMeleeRange = distToPlayer <= meleeRange;
            stateMachine.Update();
        }
        public Vector3 getRandomWaypoint() { return waypoints[Random.Range(0, waypoints.Count)].position; }
        public void playFireAnimation()
        {
            fireRight.Play();
            fireLeft.Play();
        }
        public void takeDamage(float amount) { }
        // private void Die() { stateMachine.ChangeState(new deadState(this)); }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fireRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

    }
    public class idleState : IState
    {
        GoliathAI owner;
        public idleState(GoliathAI owner) { this.owner = owner; }
        private float timer = 0;
        public void Enter() { owner.animator.CrossFade("idle", owner.animFadeIn); }
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer > owner.idleTime) owner.stateMachine.ChangeState(new patrolState(owner));
            if (owner.playerInSightRange) owner.stateMachine.ChangeState(new chaseState(owner));
            // if (owner.playerInSightRange && !owner.playerInFireRange) owner.stateMachine.ChangeState(new chaseState(owner));
            // if (owner.playerInFireRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new attackState(owner));
        }
        public void Exit() { }
    }
    public class patrolState : IState
    {
        GoliathAI owner;
        public patrolState(GoliathAI owner) { this.owner = owner; }
        private float timer = 0;
        public void Enter()
        {
            owner.agent.SetDestination(owner.getRandomWaypoint());
            owner.animator.CrossFade("walk", owner.animFadeIn);
        }
        public void Update()
        {
            if (owner.agent.remainingDistance <= owner.agent.stoppingDistance)
            {
                owner.agent.SetDestination(owner.getRandomWaypoint());
            }
            timer += Time.deltaTime;
            if (timer > owner.patrolTime) owner.stateMachine.ChangeState(new idleState(owner));
            if (owner.playerInSightRange) owner.stateMachine.ChangeState(new chaseState(owner));
        }
        public void Exit() { owner.agent.SetDestination(owner.agent.transform.position); }
    }

    public class chaseState : IState
    {
        GoliathAI owner;
        public chaseState(GoliathAI owner) { this.owner = owner; }
        private float prevSpeed;
        public void Enter()
        {
            prevSpeed = owner.agent.speed;
            owner.agent.speed = prevSpeed * 3;
            owner.animator.CrossFade("run", owner.animFadeIn);
        }
        public void Update()
        {
            owner.agent.SetDestination(owner.player.position);
            if (!owner.playerInSightRange) owner.stateMachine.ChangeState(new idleState(owner));
            if (owner.playerInFireRange) owner.stateMachine.ChangeState(new fireChargeState(owner));
        }
        public void Exit() { owner.agent.speed = prevSpeed; owner.agent.SetDestination(owner.agent.transform.position); }
    }
    public class fireChargeState : IState
    {
        GoliathAI owner;
        public fireChargeState(GoliathAI owner) { this.owner = owner; }
        private float timer = 0f;
        public void Enter() { owner.animator.CrossFade("fireCharge", owner.animFadeIn); owner.sfx.fireCharge(); }
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer > owner.fireChargeTime) owner.stateMachine.ChangeState(new fireState(owner));
            owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
        }
        public void Exit() { owner.sfx.fireChargeStop(); }
    }
    public class fireState : IState
    {
        GoliathAI owner;
        public fireState(GoliathAI owner) { this.owner = owner; }
        private float nextTimeToFire = 0f, timer = 0f;
        private Vector3 fireStartMarker, fireEndMarker;
        private float fireDistance, startTime;
        public void Enter()
        {
            owner.animator.Play("fire");
            Vector3 unitDirection = owner.transform.forward;
            Vector3 goliathPos = owner.transform.position;
            Vector3 playerPos = owner.player.transform.position;
            float trueDistance = Vector3.Distance(goliathPos, playerPos);
            fireDistance = trueDistance * 0.7f; // Start firing away from Goliath
            fireStartMarker = goliathPos + unitDirection * 0.3f * trueDistance;
            fireEndMarker = playerPos;
            startTime = Time.time;
        }
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer > owner.fireTime) owner.stateMachine.ChangeState(new fireDischargeState(owner));
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / owner.fireRate;
                owner.sfx.fire();
                owner.playFireAnimation();
                // Actual firing
                float distCovered = (Time.time - startTime) * owner.fireReachSpeed;
                Vector3 firePos = Vector3.Lerp(fireStartMarker, fireEndMarker, distCovered / fireDistance);
                if (distCovered >= fireDistance)
                {
                    Debug.Log("Player getting hit!");
                }
                else
                {
                    Vector3 fireRight = firePos + owner.transform.right * 2.0f;
                    Vector3 fireLeft = firePos + -owner.transform.right * 2.0f;
                    fireRight.y = owner.ground.SampleHeight(fireRight);
                    fireLeft.y = owner.ground.SampleHeight(fireLeft);
                    GameObject impactGO = GameObject.Instantiate(owner.impactEffect.gameObject, fireRight, Quaternion.Euler(-90, 0, 0));
                    GameObject.Destroy(impactGO, 1f);
                    impactGO = GameObject.Instantiate(owner.impactEffect.gameObject, fireLeft, Quaternion.Euler(-90, 0, 0));
                    GameObject.Destroy(impactGO, 1f);
                    updateFirePath(firePos);
                }
            }
            owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
        }
        public void Exit() { }
        void updateFirePath(Vector3 currentFirePos)
        {
            Vector3 playerPos = owner.player.transform.position;
            fireDistance = Vector3.Distance(currentFirePos, playerPos);
            fireEndMarker = playerPos;
            // startTime = Time.time;
        }
    }

    public class fireDischargeState : IState
    {
        GoliathAI owner;
        public fireDischargeState(GoliathAI owner) { this.owner = owner; }
        private float timer = 0f;
        public void Enter() { owner.animator.CrossFade("fireDischarge", owner.animFadeIn); }
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer > owner.fireChargeTime) owner.stateMachine.ChangeState(new fireChargeState(owner));
            owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
            if (!owner.playerInSightRange) owner.stateMachine.ChangeState(new idleState(owner));
        }
        public void Exit() { }
    }
}
