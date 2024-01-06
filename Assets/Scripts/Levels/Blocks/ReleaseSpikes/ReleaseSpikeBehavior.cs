using UnityEngine;

public class ReleaseSpikeBehavior : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animator.gameObject.GetComponent<ReleaseSpikes>().EnableReleased();
    }
}