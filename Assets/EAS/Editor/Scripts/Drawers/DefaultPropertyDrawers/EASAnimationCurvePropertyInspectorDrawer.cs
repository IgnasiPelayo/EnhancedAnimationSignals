using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(AnimationCurve))]
    public class EASAnimationCurvePropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override void OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            propertyValue = EditorGUI.CurveField(rect, label, (AnimationCurve)propertyValue);
        }
    }
}
