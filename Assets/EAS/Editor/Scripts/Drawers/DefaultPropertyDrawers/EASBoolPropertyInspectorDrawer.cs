using UnityEditor;
using UnityEngine;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(bool))]
    public class EASBoolPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            EditorGUI.BeginChangeCheck();

            propertyValue = EditorGUI.Toggle(rect, label, (bool)propertyValue);

            return EditorGUI.EndChangeCheck();
        }
    }
}
