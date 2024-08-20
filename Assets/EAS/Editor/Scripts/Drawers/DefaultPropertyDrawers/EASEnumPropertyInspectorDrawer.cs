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
            ShowEventOptionsMenuOnRightClick(rect, propertyValue, propertyName, propertyType);

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

        protected override bool CanCopy()
        {
            return true;
        }

        protected override void OnCopy(object property, string propertyPath, System.Type propertyType)
        {
            if (propertyType.IsDefined(typeof(System.FlagsAttribute)))
            {
                GUIUtility.systemCopyBuffer = $"Enum:{System.Convert.ToString((int)property, 2).PadLeft(8, '0')}";
            }
            else
            {
                base.OnCopy(property, propertyPath, propertyType);
                GUIUtility.systemCopyBuffer = $"Enum:{EASEditorUtils.FromCamelCase(GUIUtility.systemCopyBuffer)}";
            }
        }
    }
}
