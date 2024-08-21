using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(Object))]
    public class EASObjectPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            GUI.Label(EditorGUI.IndentedRect(rect), new GUIContent($"Use EASReference<{propertyType}> to display and use Object properties"));
            return false;
        }
    }
}
