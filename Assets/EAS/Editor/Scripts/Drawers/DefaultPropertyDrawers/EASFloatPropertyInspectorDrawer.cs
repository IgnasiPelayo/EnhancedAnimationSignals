using System;
using UnityEditor;
using UnityEngine;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(float))]
    public class EASFloatPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            ShowEventOptionsMenuOnRightClick(rect, propertyValue, propertyName, propertyType);

            EditorGUI.BeginChangeCheck();

            RangeAttribute rangeAttribute = EASEditorUtils.GetAttribute<RangeAttribute>(propertyAttributes);
            if (rangeAttribute != null)
            {
                propertyValue = EditorGUI.Slider(rect, label, (float)propertyValue, rangeAttribute.min, rangeAttribute.max);
            }
            else
            {
                MinAttribute minAttribute = EASEditorUtils.GetAttribute<MinAttribute>(propertyAttributes);

                DelayedAttribute delayedAttribute = EASEditorUtils.GetAttribute<DelayedAttribute>(propertyAttributes);
                if (delayedAttribute != null)
                {
                    propertyValue = Mathf.Clamp(EditorGUI.DelayedFloatField(rect, label, (float)propertyValue), minAttribute != null ? minAttribute.min : float.MinValue, float.MaxValue);
                }
                else
                {
                    propertyValue = Mathf.Clamp(EditorGUI.FloatField(rect, label, (float)propertyValue), minAttribute != null ? minAttribute.min : float.MinValue, float.MaxValue);
                }
            }

            return EditorGUI.EndChangeCheck();
        }

        protected override bool CanCopy()
        {
            return true;
        }

        protected override void OnCopy(object property, string propertyPath, Type propertyType)
        {
            base.OnCopy(property, propertyPath, propertyType);
            GUIUtility.systemCopyBuffer = GUIUtility.systemCopyBuffer.Replace(",", ".");
        }
    }
}


