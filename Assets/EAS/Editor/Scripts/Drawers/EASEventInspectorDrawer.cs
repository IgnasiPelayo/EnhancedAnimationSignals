using UnityEngine;

namespace EAS
{
    public abstract class EASEventInspectorDrawer
    {
        public virtual void OnGUIInspector(Rect rect, EASBaseEvent baseEvent)
        {
            EASInspectorEditor.Instance.BaseOnGUIInspector(rect, baseEvent);
        }
    }
}

