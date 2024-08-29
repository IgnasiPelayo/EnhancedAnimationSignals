using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#66CDAA"), EASEventCategory("General"), EASEventTooltip("Plays a specified animation on a given Animator. Optionally, the animation can be synchronized with the current animation")]
    public class EASPlayAnimationEvent : EASEvent
    {
        [Header("Animator Settings")]
        [SerializeField, Tooltip("The Animator component that will play the animation.")]
        protected EASReference<Animator> m_Animator = new EASReference<Animator>();
        public EASReference<Animator> AnimatorReference { get => m_Animator; }

        [Space(10)]

        [Header("Animation Settings")]
        [SerializeField, Tooltip("The name of the animation to play on the Animator.")]
        protected string m_AnimationName;

        [SerializeField, CustomAttributes.ReadOnly]
        protected int m_AnimationHash;

        [Space(5)]
        [SerializeField, Tooltip("If enabled, the animation will be synchronized with the current animation's time. If disabled, the animation will start from the beginning.")]
        protected bool m_SyncWithAnimation = true;

        public override void OnStart(float currentFrame)
        {
            if (m_Animator != null && m_Animator.Instance != null)
            {
                if (m_SyncWithAnimation)
                {
                    EASAnimationState animationState = m_Controller.GetCurrentAnimationState();
                    m_Animator.Instance.Play(m_AnimationHash, -1, animationState.NormalizedTime);
                    m_Animator.Instance.Update(0.0f);
                }
                else
                {
                    m_Animator.Instance.Play(m_AnimationHash);
                }
            }
            else
            {
                Debug.LogError("Animator is not resolved");
            }
        }

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
                        UnityEditor.Animations.AnimatorState state = EASUtils.GetAnimatorStateByHash(controller, m_AnimationHash);
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

        public override void OnUpdateTrackEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            Animator animator = m_Animator.Resolve(editorBridge.Controller);
            if (animator != null)
            {
                UnityEditor.Animations.AnimatorController controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                if (controller != null)
                {
                    UnityEditor.Animations.AnimatorState animatorState = EASUtils.GetAnimatorStateByHash(controller, m_AnimationHash);
                    if (animatorState != null)
                    {
                        editorBridge.AddSecondaryPreviewAnimation(animatorState.motion as AnimationClip, m_SyncWithAnimation ? 0 : m_StartFrame, animator.gameObject);
                    }
                }
            }
        }
#endif // UNITY_EDITOR
    }
}
