using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#4A90E2"), EASEventCategory("General"), EASEventTooltip("Adjusts the time scale of the animation by applying a random value between the specified minimum and maximum values.")]
    public class EASAnimationTimeScaleEvent : EASCustomOwnerEvent<EASController>
    {
        [Header("Animation Time Scale Settings")]
        [SerializeField, Tooltip("The range for the time scale. The time scale will be a random value between the x (minimum) and y (maximum) components.")]
        protected Vector2 m_TimeScaleRange = new Vector2(1.0f, 1.0f);

        protected float m_TimeScale;
        public float TimeScale { get => m_TimeScale; }

        [SerializeField, HideInInspector]
        protected EASReference<Animator>[] m_TargetAnimators;

        public override void OnStart(float currentFrame)
        {
            m_TimeScale = Random.Range(m_TimeScaleRange.x, m_TimeScaleRange.y);
            SetTimeScale(m_TimeScale);
        }

        public override void OnEnd()
        {
            SetTimeScale(1.0f);
        }

        protected void SetTimeScale(float timeScale)
        {
            EASUtils.SetTimeScale(Owner.Animator, timeScale);
            if (m_TargetAnimators != null)
            {
                for (int i = 0; i < m_TargetAnimators.Length; ++i)
                {
                    if (m_TargetAnimators[i] != null && m_TargetAnimators[i].Instance != null)
                    {
                        EASUtils.SetTimeScale(m_TargetAnimators[i].Instance, timeScale);
                    }
                }
            }
        }

#if UNITY_EDITOR
        public override void OnValidate()
        {
            m_TimeScaleRange = new Vector2(Mathf.Clamp(m_TimeScaleRange.x, 0.01f, float.MaxValue), Mathf.Clamp(m_TimeScaleRange.y, 0.01f, float.MaxValue));
        }

        public override void OnAnimationModified(IEASEditorBridge editorBridge)
        {
            List<EASPlayAnimationEvent> playAnimationEvents = editorBridge.GetEvents<EASPlayAnimationEvent>(addMuted: true);

            List<EASReference<Animator>> additionalTargets = new List<EASReference<Animator>>();
            foreach (EASPlayAnimationEvent playAnimationEvent in playAnimationEvents)
            {
                if (playAnimationEvent.AnimatorReference != null)
                {
                    additionalTargets.Add(playAnimationEvent.AnimatorReference);
                }
            }
            m_TargetAnimators = additionalTargets.ToArray();
        }

        public override string GetLabel()
        {
            return base.GetLabel() + ": " + m_TimeScaleRange;
        }

        public override string GetErrorMessage(EASBaseController controller)
        {
            string errorMessage = string.Empty;
            for (int i = 0; i < m_TargetAnimators.Length; ++i)
            {
                Animator animator = m_TargetAnimators[i].Resolve(controller);
                if (animator == null)
                {
                    errorMessage += (string.IsNullOrEmpty(errorMessage) ? "" : " and ") + $"Animator at index {i} is not set";
                }
                else if (animator == (controller as EASController).Animator)
                {
                    errorMessage += (string.IsNullOrEmpty(errorMessage) ? "" : " and ") + $"TargetAnimators can't contain owner's Animator at index {i}";
                }
            }

            return errorMessage;
        }

        public override void OnStartEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            m_TimeScale = Random.Range(m_TimeScaleRange.x, m_TimeScaleRange.y);
            editorBridge.SetTimeScale(m_TimeScale);
        }

        public override void OnEndEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            editorBridge.CancelTimeScale(m_TimeScale);
        }
#endif // UNITY_EDITOR
    }
}
