using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(Object))]
    public class EASObjectPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            ShowEventOptionsMenuOnRightClick(rect, baseEvent, propertyValue, propertyName, propertyType);

            EditorGUI.BeginChangeCheck();

            propertyValue = EditorGUI.ObjectField(rect, label, (Object)propertyValue, propertyType, allowSceneObjects: false);

            return EditorGUI.EndChangeCheck();
        }
    }
}
