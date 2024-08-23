using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#66CDAA"), EASEventCategory("Animation"), EASEventTooltip("Plays a specified animation on a given Animator. Optionally, the animation can be synchronized with the current animation")]
    public class EASPlayAnimationEvent : EASEvent
    {
        [Header("Animator Settings")]
        [SerializeField, Tooltip("The Animator component that will play the animation.")]
        protected EASReference<Animator> m_Animator = new EASReference<Animator>();

        [Space(10)]

        [Header("Animation Settings")]
        [SerializeField, Tooltip("The name of the animation to play on the Animator.")]
        protected string m_AnimationName;

        [SerializeField, CustomAttributes.ReadOnly]
        protected int m_AnimationHash;

        [Space(5)]
        [SerializeField, Tooltip("If enabled, the animation will be synchronized with the current animation's time. If disabled, the animation will start from the beginning.")]
        protected bool m_SyncWithAnimation = true;

#if UNITY_EDITOR
        public override void OnValidate()
        {
            m_AnimationHash = Animator.StringToHash(m_AnimationName);
        }

        public override string GetErrorMessage(EASBaseController owner)
        {
            string errorMessage = string.Empty;
            if (m_Animator != null)
            {
                Animator animator = m_Animator.Resolve(owner);
                if (animator == null)
                {
                    errorMessage += "Animator is not set";
                }
                if (string.IsNullOrEmpty(m_AnimationName))
                {
                    errorMessage += (string.IsNullOrEmpty(errorMessage) ? "" : " and ") + "AnimationName is empty";
                }

                if (string.IsNullOrEmpty(errorMessage))
                {
                    UnityEditor.Animations.AnimatorController controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                    if (controller != null)
                    {
                        UnityEditor.Animations.AnimatorState state = GetAnimatorStateByHash(controller, m_AnimationHash);
                        if (state != null)
                        {
                            return errorMessage;
                        }

                        return $"AnimatorController {controller.name} does not contain any animation named {m_AnimationName}";
                    }

                    return $"Animator {animator.name} does not have any AnimatorController";
                }
            }

            return errorMessage;
        }

        public override string GetLabel()
        {
            return $"{(m_SyncWithAnimation ? "[Synced] " : "")}" + base.GetLabel() + $": {m_AnimationName}";
        }

        public override void OnUpdateEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            Animator animator = m_Animator.Resolve(editorBridge.Controller);
            if (animator != null)
            {
                UnityEditor.Animations.AnimatorController controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                if (controller != null)
                {
                    UnityEditor.Animations.AnimatorState animatorState = GetAnimatorStateByHash(controller, m_AnimationHash);
                    if (animatorState != null)
                    {
                        editorBridge.AddSecondaryPreviewAnimation(animatorState.motion as AnimationClip, m_SyncWithAnimation ? 0 : m_StartFrame, animator.gameObject);
                    }
                }
            }
        }

        protected UnityEditor.Animations.AnimatorState GetAnimatorStateByHash(UnityEditor.Animations.AnimatorController controller, int animationHash)
        {
            if (controller != null)
            {
                foreach (UnityEditor.Animations.AnimatorControllerLayer layer in controller.layers)
                {
                    foreach (UnityEditor.Animations.ChildAnimatorState state in layer.stateMachine.states)
                    {
                        if (state.state.nameHash == m_AnimationHash && state.state.motion != null && state.state.motion is AnimationClip)
                        {
                            return state.state;
                        }
                    }
                }
            }

            return null;
        }
#endif // UNITY_EDITOR
    }
}
