using UnityEngine;
using System.Collections.Generic;

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

        public override object GetAnimation(string animationName)
        {
            Animator animator = GetComponent<Animator>();

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; ++i)
                {
                    if (animator.runtimeAnimatorController.animationClips[i].name == animationName)
                    {
                        return animator.runtimeAnimatorController.animationClips[i];
                    }
                }
            }

            return null;
        }

        public override float GetLength(object animation)
        {
            AnimationClip animationClip = animation as AnimationClip;
            if (animationClip != null)
            {
                return animationClip.length;
            }

            return 0;
        }

        public override float GetFrameRate(object animation)
        {
            AnimationClip animationClip = animation as AnimationClip;
            if (animationClip != null)
            {
                return animationClip.frameRate;
            }

            return 0;
        }

#if UNITY_EDITOR
        public override List<int> GetKeyFrames(object animation)
        {
            AnimationClip animationClip = animation as AnimationClip;
            if (animationClip != null)
            {
                int lastFrame = Mathf.RoundToInt(animationClip.length * animationClip.frameRate);
                List<int> keyFrames = new List<int>();

                UnityEditor.EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(animationClip);
                for (int i = 0; i < curveBindings.Length; ++i)
                {
                    AnimationCurve animationCurve = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveBindings[i]);
                    for (int j = 0; j < animationCurve.keys.Length; ++j)
                    {
                        int frame = Mathf.RoundToInt(Mathf.Clamp(animationCurve.keys[j].time * animationClip.frameRate, 0, lastFrame - 1));
                        if (!keyFrames.Contains(frame))
                        {
                            keyFrames.Add(frame);
                        }
                    }
                }

                return keyFrames;
            }

            return null;
        }
#endif // UNITY_EDITOR
    }
}


