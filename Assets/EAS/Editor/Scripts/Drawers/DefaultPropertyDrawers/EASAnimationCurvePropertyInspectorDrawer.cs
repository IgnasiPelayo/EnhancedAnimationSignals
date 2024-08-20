using UnityEngine;
using UnityEditor;
using Unity.Plastic.Newtonsoft.Json;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(AnimationCurve))]
    public class EASAnimationCurvePropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            ShowEventOptionsMenuOnRightClick(rect, propertyValue, propertyName, propertyType);

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
