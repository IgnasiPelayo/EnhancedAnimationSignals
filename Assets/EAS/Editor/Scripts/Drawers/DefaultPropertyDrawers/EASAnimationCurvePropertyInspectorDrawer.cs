using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(AnimationCurve))]
    public class EASAnimationCurvePropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            EditorGUI.BeginChangeCheck();

            propertyValue = EditorGUI.CurveField(rect, label, (AnimationCurve)propertyValue);

            return EditorGUI.EndChangeCheck();
        }
    }
}
