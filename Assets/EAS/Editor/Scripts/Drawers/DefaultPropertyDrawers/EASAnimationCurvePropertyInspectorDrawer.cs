using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(AnimationCurve))]
    public class EASAnimationCurvePropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            AnimationCurve propertyValueAsAnimationCurve = propertyValue as AnimationCurve;
            if (propertyValueAsAnimationCurve == null)
            {
                propertyValue = new AnimationCurve();
            }

            ShowEventOptionsMenuOnRightClick(rect, baseEvent, propertyValue, propertyName, propertyType);

            EditorGUI.BeginChangeCheck();

            propertyValue = EditorGUI.CurveField(rect, label, (AnimationCurve)propertyValue);

            return EditorGUI.EndChangeCheck();
        }

        protected override bool CanCopy()
        {
            return false;
        }
    }
}
