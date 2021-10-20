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
        public string waypointsName = "waypoints";
        public bool randomPatrolNavigation = true;
        public float randomPatrolRadius = 10f;
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
            // Transform waypointsObject = GameObject.FindGameObjectWithTag(waypointsName).transform;
            // if (waypointsObject is null) foreach (Transform t in waypointsObject) { waypoints.Add(t); }
            // Snap monster back to NavMesh if he's not set properly
            // https://stackoverflow.com/questions/46495820/unity3d-how-to-connect-navmesh-and-navmeshagent
            while (!agent.isOnNavMesh)
            {
                agent.transform.position = getRandomPatrolLocation();
                agent.enabled = false;
                agent.enabled = true;
            }
            // Randomize activity times so that the audio doesnt sound odd
            idleTime *= Random.Range(0.7f, 1.3f);
            patrolTime *= Random.Range(0.7f, 1.3f);
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
        public void playFireAnimation()
        {
            fireRight.Play();
            fireLeft.Play();
        }
        public bool testLineOfSight(Vector3 goliathFirePos, Vector3 startPos)
        {
            // We know that the muzzleFlash particle systems are in proper place with regards to Goliath's model
            // Let's exploit that to figure out whether the line of sight of the weapon is ok
            // Vector3 goliathRightGun = fireRight.transform.position;
            // Vector3 goliathLeftGun = fireRight.transform.position;
            // Vector3 playerPos = player.position;
            // playerPos.y += 0.2f;
            RaycastHit hit;
            if (Physics.Raycast(goliathFirePos, (startPos - goliathFirePos).normalized, out hit, Vector3.Distance(startPos, goliathFirePos)))
            {
                if (hit.transform.tag == "Player") return true;
            }
            return false;
        }
        public void takeDamage(hitData dData)
        {
            GameObject hitGO = Instantiate(impactEffect.gameObject, dData.hit.point, Quaternion.LookRotation(dData.hit.normal));
            Destroy(hitGO, 2f);
            sfx.shot();
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
            Vector3 playerPos = owner.player.position;
            playerPos.y = owner.ground.SampleHeight(playerPos);
            NavMeshPath path = new NavMeshPath();
            if (owner.agent.CalculatePath(playerPos, path)) owner.agent.SetDestination(playerPos);
            else owner.stateMachine.ChangeState(new fireChargeState(owner));
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
            // First look at player
            owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
            owner.animator.Play("fire");
            Vector3 unitDirection = owner.transform.forward;
            Vector3 goliathPos = owner.transform.position;
            Vector3 playerPos = owner.player.transform.position;
            // float trueDistance = Vector3.Distance(goliathPos, playerPos);
            Vector3 fireStartPos = goliathPos + unitDirection * 0.3f; // Start firing away from Goliath
            // If player is within range set the fire to player position, else set it to edge of range
            Vector3 fireEndPos;
            if (Vector3.Distance(goliathPos, playerPos) > owner.fireRange)
            {
                fireEndPos = goliathPos + unitDirection * owner.fireRange; // End fire at range
            }
            else fireEndPos = playerPos;
            fireDistance = Vector3.Distance(fireStartPos, fireEndPos);
            // Set the markers
            fireStartMarker = fireStartPos;
            fireEndMarker = fireEndPos;
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
                Vector3 goliathRightGun = owner.fireRight.transform.position;
                Vector3 goliathLeftGun = owner.fireLeft.transform.position;
                Vector3 playerPos = owner.player.position;
                playerPos.y += 0.2f;
                bool rightGunTest = owner.testLineOfSight(goliathRightGun, playerPos);
                bool leftGunTest = owner.testLineOfSight(goliathLeftGun, playerPos);
                if (Vector3.Distance(firePos, owner.player.position) <= 10 && (rightGunTest || leftGunTest))
                {
                    // Reduce player health
                    LevelManager.Instance.playerHurt(2f);
                }
                else
                {
                    Vector3 fireRight = firePos + -owner.transform.right * 2.0f;
                    Vector3 fireLeft = firePos + owner.transform.right * 2.0f;
                    fireRight.y = owner.ground.SampleHeight(fireRight);
                    fireLeft.y = owner.ground.SampleHeight(fireLeft);
                    GameObject impactGO;
                    if (rightGunTest)
                    {
                        impactGO = GameObject.Instantiate(owner.impactEffect.gameObject, fireRight, Quaternion.Euler(-90, 0, 0));
                        GameObject.Destroy(impactGO, 1f);
                    }
                    if (leftGunTest)
                    {
                        impactGO = GameObject.Instantiate(owner.impactEffect.gameObject, fireLeft, Quaternion.Euler(-90, 0, 0));
                        GameObject.Destroy(impactGO, 1f);
                    }
                    updateFirePath(firePos);
                }
            }
            owner.transform.LookAt(new Vector3(owner.player.position.x, owner.transform.position.y, owner.player.position.z));
        }
        public void Exit() { }
        void updateFirePath(Vector3 currentFirePos)
        {
            Vector3 unitDirection = owner.transform.forward;
            Vector3 goliathPos = owner.transform.position;
            Vector3 playerPos = owner.player.position;
            // If player is within range set the fire to player position, else set it to edge of range
            Vector3 fireEndPos;
            if (Vector3.Distance(goliathPos, playerPos) > owner.fireRange)
            {
                fireEndPos = goliathPos + unitDirection * owner.fireRange; // End fire at range
            }
            else fireEndPos = playerPos;
            fireDistance = Vector3.Distance(currentFirePos, fireEndPos);
            fireEndMarker = fireEndPos;
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

    public class deadState : IState
    {
        GoliathAI owner;
        public deadState(GoliathAI owner) { this.owner = owner; }
        private bool killCountSet = false;
        public void Enter()
        {
            owner.animator.Play("die");
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
