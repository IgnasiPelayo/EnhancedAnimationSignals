using UnityEngine;

namespace EAS
{
    public abstract class EASEventTimelineDrawer
    {
        public virtual void OnGUIBackground(Rect rect, EASBaseEvent baseEvent)
        {
            EASEditor.Instance.Timeline.BaseOnGUIEventBackground(rect, baseEvent);
        }

        public virtual void OnGUISelected(Rect rect, EASBaseEvent baseEvent)
        {
            EASEditor.Instance.Timeline.BaseOnGUIEventSelected(rect, baseEvent);
        }

        public abstract Color LabelColor { get; }
    }
}
