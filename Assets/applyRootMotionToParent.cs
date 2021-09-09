using UnityEngine;

public class applyRootMotionToParent : MonoBehaviour
{
    void OnAnimatorMove()
    {
        Animator animator = GetComponent<Animator>();
        // Model is the child of character (enemy) entity
        // transform.parent.gameObject.transform.position += animator.deltaPosition;
        // transform.parent.transform.rotation += animator.deltaRotation;
        // transform.parent.rotation = animator.rootRotation;
        // transform.parent.position += animator.deltaPosition;


        // transform.parent.rotation = animator.rootRotation;
        // Vector3 deltaPos = animator.deltaPosition;
        // deltaPos.y = 0;
        // transform.parent.position += animator.deltaPosition;
        animator.ApplyBuiltinRootMotion();
    }
}
