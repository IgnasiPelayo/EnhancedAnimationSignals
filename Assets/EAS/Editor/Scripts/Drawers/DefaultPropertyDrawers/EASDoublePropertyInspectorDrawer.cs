using UnityEditor;
using UnityEngine;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(double))]
    public class EASDoublePropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override void OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            RangeAttribute rangeAttribute = EASEditorUtils.GetAttribute<RangeAttribute>(propertyAttributes);
            if (rangeAttribute != null)
            {
                propertyValue = (double)EditorGUI.Slider(rect, label, (float)((double)propertyValue), rangeAttribute.min, rangeAttribute.max);
            }
            else
            {
                MinAttribute minAttribute = EASEditorUtils.GetAttribute<MinAttribute>(propertyAttributes);

                DelayedAttribute delayedAttribute = EASEditorUtils.GetAttribute<DelayedAttribute>(propertyAttributes);
                if (delayedAttribute != null)
                {
                    propertyValue = (double)Mathf.Clamp((float)EditorGUI.DelayedDoubleField(rect, label, (double)propertyValue), minAttribute != null ? minAttribute.min : (float)double.MinValue, (float)double.MaxValue);
                }
                else
                {
                    propertyValue = (double)Mathf.Clamp((float)EditorGUI.DoubleField(rect, label, (double)propertyValue), minAttribute != null ? minAttribute.min : (float)double.MinValue, (float)double.MaxValue);
                }
            }
        }
    }
}



