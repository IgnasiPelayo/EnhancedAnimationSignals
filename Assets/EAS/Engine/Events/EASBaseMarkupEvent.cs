
using UnityEngine;

namespace EAS
{
    public abstract class EASBaseMarkupEvent : EASEvent
    {
        protected void AddMarkup(EASBaseController controller, System.Type markupType, int markup, bool wasSkipped)
        {
            if (wasSkipped)
            {
                controller.AddSkippedMarkup(markupType, markup);
            }
            else
            {
                controller.AddActiveMarkup(markupType, markup);
            }
        }

        protected void RemoveMarkup(EASBaseController controller, System.Type markupType, int markup)
        {
            controller.RemoveActiveMarkup(markupType, markup);
        }

#if UNITY_EDITOR
        public override bool CanPreviewInEditor(IEASEditorBridge editorBridge)
        {
            return false;
        }
#endif // UNITY_EDITOR
    }

    [System.Serializable]
    [EASEventColor("#F5F5F5"), EASEventCategory("General"), EASEventTooltip("Applies a predefined markup or flag to control conditional logic or state management within animations.")]
    public class EASMarkupExampleEvent : EASBaseMarkupEvent
    {
        [Header("Markup Settings")]
        [SerializeField, Tooltip("The predefined markup or flag to apply or clear.")]
        protected EASMarkup m_Markup;

        public override void OnStart(float currentFrame)
        {
            AddMarkup(Owner, typeof(EASMarkup), (int)m_Markup, wasSkipped: false);
        }

        public override void OnEnd()
        {
            RemoveMarkup(Owner, typeof(EASMarkup), (int)m_Markup);
        }

        public override void OnSkip(float currentFrame)
        {
            AddMarkup(Owner, typeof(EASMarkup), (int)m_Markup, wasSkipped: true);
        }

#if UNITY_EDITOR
        public override string GetLabel()
        {
            return $"Markup: {m_Markup}";
        }
#endif // UNITY_EDITOR
    }
}
