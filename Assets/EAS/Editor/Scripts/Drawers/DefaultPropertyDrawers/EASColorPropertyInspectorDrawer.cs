using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(Color))]
    public class EASColorPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            EditorGUI.BeginChangeCheck();

            propertyValue = EditorGUI.ColorField(rect, label, (Color)propertyValue);

            return EditorGUI.EndChangeCheck();
        }
    }
}
