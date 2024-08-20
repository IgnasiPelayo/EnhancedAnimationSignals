using UnityEngine;
using UnityEditor;
using System;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(Color))]
    public class EASColorPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            ShowEventOptionsMenuOnRightClick(GetLabelRect(rect), baseEvent, propertyValue, propertyName, propertyType);

            EditorGUI.BeginChangeCheck();

            propertyValue = EditorGUI.ColorField(rect, label, (Color)propertyValue);

            return EditorGUI.EndChangeCheck();
        }

        protected override bool CanCopy()
        {
            return true;
        }

        protected override void OnCopy(object property, string propertyPath, Type propertyType)
        {
            GUIUtility.systemCopyBuffer = $"#{EASUtils.ColorToHex((Color)property, addAlpha: true)}";
        }

        public override object GetPasteValueFromClipboard()
        {
            if (GUIUtility.systemCopyBuffer.StartsWith("#") && GUIUtility.systemCopyBuffer.Length == 9)
            {
                return EASUtils.HexToColor(GUIUtility.systemCopyBuffer);
            }

            return null;
        }
    }
}
