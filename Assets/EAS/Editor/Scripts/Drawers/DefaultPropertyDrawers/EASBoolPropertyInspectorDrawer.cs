using UnityEditor;
using UnityEngine;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(bool))]
    public class EASBoolPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            ShowEventOptionsMenuOnRightClick(rect, baseEvent, propertyValue, propertyName, propertyType);

            EditorGUI.BeginChangeCheck();

            propertyValue = EditorGUI.Toggle(rect, label, (bool)propertyValue);

            return EditorGUI.EndChangeCheck();
        }

        protected override bool CanCopy()
        {
            return true;
        }

        public override object GetPasteValueFromClipboard()
        {
            if (GUIUtility.systemCopyBuffer == "True")
            {
                return true;
            }

            if (GUIUtility.systemCopyBuffer == "False")
            {
                return false;
            }

            return null;
        }
    }
}
