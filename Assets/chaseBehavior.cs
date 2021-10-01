using UnityEngine;
using UnityEngine.AI;

public class chaseBehavior : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;
    float attackRange;
    float chaseRange;
    float prevSpeed;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        prevSpeed = agent.speed;
        agent.speed = prevSpeed * 3;
        attackRange = animator.GetFloat("attackRange");
        chaseRange = animator.GetFloat("chaseRange");
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.SetDestination(player.position);

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance < attackRange)
            animator.SetBool("isAttacking", true);
        if (distance > chaseRange)
            animator.SetBool("isChasing", false);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.speed = prevSpeed;
        agent.SetDestination(agent.transform.position);
    }
}
