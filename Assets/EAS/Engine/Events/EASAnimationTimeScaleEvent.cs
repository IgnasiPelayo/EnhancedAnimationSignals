using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#4A90E2"), EASEventCategory("Animation"), EASEventTooltip("Adjusts the time scale of the animation by applying a random value between the specified minimum and maximum values")]
    public class EASAnimationTimeScaleEvent : EASEvent
    {
        [Header("Animation Time Scale Settings")]
        [SerializeField, Tooltip("The range for the time scale. The time scale will be a random value between the x (minimum) and y (maximum) components.")]
        private Vector2 m_TimeScaleRange = new Vector2(1.0f, 1.0f);

        protected float m_TimeScaleApplied;

#if UNITY_EDITOR
        public override void OnValidate()
        {
            m_TimeScaleRange = new Vector2(Mathf.Clamp(m_TimeScaleRange.x, 0.01f, float.MaxValue), Mathf.Clamp(m_TimeScaleRange.y, 0.01f, float.MaxValue));
        }

        public override string GetLabel()
        {
            return base.GetLabel() + ": " + m_TimeScaleRange;
        }

        public override void OnStartEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            m_TimeScaleApplied = Random.Range(m_TimeScaleRange.x, m_TimeScaleRange.y);
            editorBridge.SetTimeScale(m_TimeScaleApplied);
        }

        public override void OnEndEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            editorBridge.CancelTimeScale(m_TimeScaleApplied);
        }
#endif // UNITY_EDITOR
    }
}
