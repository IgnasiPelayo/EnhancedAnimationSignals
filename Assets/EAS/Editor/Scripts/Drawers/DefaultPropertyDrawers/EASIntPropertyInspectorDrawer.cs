using UnityEditor;
using UnityEngine;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(int))]
    public class EASIntPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            EditorGUI.BeginChangeCheck();

            RangeAttribute rangeAttribute = EASEditorUtils.GetAttribute<RangeAttribute>(propertyAttributes);
            if (rangeAttribute != null)
            {
                propertyValue = EditorGUI.IntSlider(rect, label, (int)propertyValue, Mathf.RoundToInt(rangeAttribute.min), Mathf.RoundToInt(rangeAttribute.max));
            }
            else
            {
                MinAttribute minAttribute = EASEditorUtils.GetAttribute<MinAttribute>(propertyAttributes);

                DelayedAttribute delayedAttribute = EASEditorUtils.GetAttribute<DelayedAttribute>(propertyAttributes);
                if (delayedAttribute != null)
                {
                    propertyValue = Mathf.RoundToInt(Mathf.Clamp(EditorGUI.DelayedIntField(rect, label, (int)propertyValue), minAttribute != null ? minAttribute.min : int.MinValue, int.MaxValue));
                }
                else
                {
                    propertyValue = Mathf.RoundToInt(Mathf.Clamp(EditorGUI.IntField(rect, label, (int)propertyValue), minAttribute != null ? minAttribute.min : int.MinValue, int.MaxValue));
                }
            }

            return EditorGUI.EndChangeCheck();
        }
    }
}

