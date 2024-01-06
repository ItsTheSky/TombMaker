using UnityEngine;

public class ShopSkinDemoController : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SwitchSkin(Constants.SkinMeta skinMeta)
    {
        // stop all animations
        animator.StopPlayback();
        
        /* AnimationClip clip = Resources.Load<AnimationClip>(skinMeta.name + "/PlayerIdleUI");
        animator.runtimeAnimatorController.animationClips[0] = clip; */
        animator.Play(skinMeta.id.ToString());
    }
}
