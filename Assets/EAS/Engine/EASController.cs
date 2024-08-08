using UnityEngine;

namespace EAS
{
    public class EASController : EASBaseController
    {
        public override string[] GetAnimationNames()
        {
            Animator animator = GetComponent<Animator>();

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                string[] animationNames = new string[animator.runtimeAnimatorController.animationClips.Length];

                for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; ++i)
                {
                    animationNames[i] = animator.runtimeAnimatorController.animationClips[i].name;
                }
                return animationNames;
            }

            return new string[0];
        }
    }
}


