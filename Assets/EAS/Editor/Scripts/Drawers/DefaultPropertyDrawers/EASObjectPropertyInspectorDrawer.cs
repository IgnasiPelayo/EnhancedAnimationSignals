using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(Object))]
    public class EASObjectPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override void OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            propertyValue = EditorGUI.ObjectField(rect, label, (Object)propertyValue, propertyType, allowSceneObjects: true);
        }
    }
}
