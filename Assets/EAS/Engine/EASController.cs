using UnityEngine;
using System.Collections.Generic;

namespace EAS
{
    public class EASController : EASBaseController
    {
        [SerializeField]
        protected Animator m_Animator;
        public Animator Animator { get => m_Animator; }

        public override bool HasAnimations()
        {
            if (m_Animator != null && m_Animator.runtimeAnimatorController != null)
            {
                return m_Animator.runtimeAnimatorController.animationClips.Length > 0;
            }

            return false;
        }

        public override string[] GetAnimationNames()
        {
            if (m_Animator != null && m_Animator.runtimeAnimatorController != null)
            {
                string[] animationNames = new string[m_Animator.runtimeAnimatorController.animationClips.Length];

                for (int i = 0; i < m_Animator.runtimeAnimatorController.animationClips.Length; ++i)
                {
                    animationNames[i] = m_Animator.runtimeAnimatorController.animationClips[i].name;
                }
                return animationNames;
            }

            return new string[0];
        }

        public override EASAnimationState GetCurrentAnimationState()
        {
            AnimatorStateInfo animatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            return new EASAnimationState(animatorStateInfo.shortNameHash, animatorStateInfo.normalizedTime, animatorStateInfo.loop);
        }

#if UNITY_EDITOR
        public override EASAnimationInformation GetAnimation(string animationName)
        {
            if (m_Animator != null && m_Animator.runtimeAnimatorController != null)
            {
                AnimationClip animationClip = GetAnimationClipByName(m_Animator.runtimeAnimatorController, animationName);
                if (animationClip != null)
                {
                    return new EASAnimationInformation(animationClip, animationClip.name, animationClip.length, animationClip.frameRate, GetKeyFrames(animationClip));
                }
            }

            return null;
        }

        protected AnimationClip GetAnimationClipByName(RuntimeAnimatorController runtimeAnimatorController, string animationName)
        {
            for (int i = 0; i < runtimeAnimatorController.animationClips.Length; ++i)
            {
                if (runtimeAnimatorController.animationClips[i].name == animationName)
                {
                    return runtimeAnimatorController.animationClips[i];
                }
            }

            return null;
        }

        public override EASAnimationInformation[] GetAnimations()
        {
            List<EASAnimationInformation> animations = new List<EASAnimationInformation>();
            foreach (AnimationClip animationClip in m_Animator.runtimeAnimatorController.animationClips)
            {
                animations.Add(new EASAnimationInformation(animationClip, animationClip.name, animationClip.length, animationClip.frameRate, GetKeyFrames(animationClip)));
            }

            return animations.ToArray();
        }

        public List<int> GetKeyFrames(AnimationClip animationClip)
        {
            int lastFrame = Mathf.RoundToInt(animationClip.length * animationClip.frameRate);
            List<int> keyFrames = new List<int>();

            UnityEditor.EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
            for (int i = 0; i < curveBindings.Length; ++i)
            {
                UnityEditor.ObjectReferenceKeyframe[] objectReferenceKeyFrames = UnityEditor.AnimationUtility.GetObjectReferenceCurve(animationClip, curveBindings[i]);
                for (int j = 0; j < objectReferenceKeyFrames.Length; ++j)
                {
                    int keyFrame = Mathf.RoundToInt(objectReferenceKeyFrames[j].time * animationClip.frameRate);
                    if (!keyFrames.Contains(keyFrame))
                    {
                        keyFrames.Add(keyFrame);
                    }
                }
            }

            return keyFrames;
        }

        public override void PreviewAnimations(float time, EASAnimationInformation animation, List<EASAdditionalAnimationInformation> animations)
        {
            UnityEditor.AnimationMode.BeginSampling();
            Animator animator = GetComponent<Animator>();

            UnityEditor.AnimationMode.SampleAnimationClip(gameObject, animation.Animation as AnimationClip, time);

            for (int i = 0; i < animations.Count; ++i)
            {
                EASAdditionalAnimationInformation additionalAnimationInformation = animations[i];
                UnityEditor.AnimationMode.SampleAnimationClip(additionalAnimationInformation.GameObject, additionalAnimationInformation.AnimationClip, additionalAnimationInformation.Time);
            }

            UnityEditor.AnimationMode.EndSampling();
        }
#endif // UNITY_EDITOR
    }
}


