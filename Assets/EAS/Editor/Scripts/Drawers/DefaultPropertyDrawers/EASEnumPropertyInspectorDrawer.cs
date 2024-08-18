using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(System.Enum))]
    class EASEnumPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            EditorGUI.BeginChangeCheck();

            if (propertyType.IsDefined(typeof(System.FlagsAttribute)))
            {
                propertyValue = EditorGUI.EnumFlagsField(rect, label, (System.Enum)propertyValue);
            }
            else
            {
                propertyValue = EditorGUI.EnumPopup(rect, label, (System.Enum)propertyValue);
            }

            return EditorGUI.EndChangeCheck();
        }
    }
}
