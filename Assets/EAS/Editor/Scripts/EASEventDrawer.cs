using UnityEngine;
using UnityEditor;

namespace EAS
{
    public abstract class EASEventDrawer
    {
        public virtual void OnGUIBackground(Rect rect, EASBaseEvent baseEvent)
        {
            EditorGUI.DrawRect(rect, Color.magenta);
        }

        public virtual void OnGUISelected(Rect rect, EASBaseEvent baseEvent)
        {
            ExtendedGUI.ExtendedGUI.DrawOutlineRect(rect, Color.green, 1);
        }

        public abstract Color LabelColor { get; }
    }
}
