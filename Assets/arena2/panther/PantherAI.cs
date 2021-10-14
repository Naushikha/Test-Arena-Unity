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
        protected internal NavMeshAgent agent;
        protected internal PantherSFX sfx;
        protected internal string waypointsName = "waypoints";
        protected internal List<Transform> waypoints = new List<Transform>();
        protected internal float animFadeIn = 0.5f;
        protected internal bool playerInSightRange, playerInMeleeRange;
        protected internal StateMachine stateMachine = new StateMachine();
        private void Start()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            sfx = GetComponent<PantherSFX>();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            ground = GameObject.FindGameObjectWithTag("Ground").GetComponent<Terrain>();
            if (!player || !ground) Debug.LogError("Player or Ground is not set for Panther!");
            Transform waypointsObject = GameObject.FindGameObjectWithTag(waypointsName).transform;
            foreach (Transform t in waypointsObject) { waypoints.Add(t); }
            stateMachine.ChangeState(new idleState(this));
        }
        private void Update()
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            playerInSightRange = distToPlayer <= sightRange;
            playerInMeleeRange = distToPlayer <= meleeRange;
            stateMachine.Update();
        }
        public Vector3 getRandomWaypoint() { return waypoints[Random.Range(0, waypoints.Count)].position; }
        public void takeDamage(float amount) { }
        // private void Die() { stateMachine.ChangeState(new deadState(this)); }
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
            // if (owner.playerInSightRange && !owner.playerInFireRange) owner.stateMachine.ChangeState(new chaseState(owner));
            // if (owner.playerInFireRange && owner.playerInSightRange) owner.stateMachine.ChangeState(new attackState(owner));
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
        PantherAI owner;
        public chaseState(PantherAI owner) { this.owner = owner; }
        private float prevSpeed;
        public void Enter()
        {
            prevSpeed = owner.agent.speed;
            owner.agent.speed = prevSpeed * 5;
            owner.animator.CrossFade("run", owner.animFadeIn);
            owner.sfx.seen();
        }
        public void Update()
        {
            owner.agent.SetDestination(owner.player.position);
            if (!owner.playerInSightRange) owner.stateMachine.ChangeState(new idleState(owner));
            // if (owner.playerInFireRange) owner.stateMachine.ChangeState(new fireChargeState(owner));
        }
        public void Exit() { owner.agent.speed = prevSpeed; owner.agent.SetDestination(owner.agent.transform.position); }
    }
}
