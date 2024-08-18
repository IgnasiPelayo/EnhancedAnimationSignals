using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(string))]
    public class EASStringPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        public override float GetPropertyHeight(EASBaseEvent baseEvent, string propertyName, System.Type propertyType, object propertyValue, object[] propertyAttributes)
        {
            if (EASEditorUtils.GetAttribute<TextAreaAttribute>(propertyAttributes) != null)
            {
                return 3.5f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return base.GetPropertyHeight(baseEvent, propertyName, propertyType, propertyValue, propertyAttributes);
        }

        protected override void OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            DelayedAttribute delayedAttribute = EASEditorUtils.GetAttribute<DelayedAttribute>(propertyAttributes);
            TextAreaAttribute textAreaAttribute = EASEditorUtils.GetAttribute<TextAreaAttribute>(propertyAttributes);

            if (delayedAttribute != null)
            {
                propertyValue = EditorGUI.DelayedTextField(rect, label, (string)propertyValue);
            }
            else
            {
                if (textAreaAttribute != null)
                {
                    Rect labelRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

                    Rect textAreaRect = new Rect(rect.x, labelRect.yMax + EditorGUIUtility.standardVerticalSpacing, rect.width, 2.5f * EditorGUIUtility.singleLineHeight);

                    float textAreaViewWidth = textAreaRect.width - 15;
                    Rect textAreaViewRect = new Rect(textAreaRect.x, textAreaRect.y, textAreaViewWidth, GUI.skin.textArea.CalcHeight(new GUIContent((string)propertyValue), textAreaViewWidth));

                    SetInspectorVariable(baseEvent, propertyName, GUI.BeginScrollView(textAreaRect, GetInspectorVariable<Vector2>(baseEvent, propertyName), textAreaViewRect));

                    GUI.SetNextControlName("TextArea");
                    propertyValue = EditorGUI.TextArea(textAreaRect, (string)propertyValue);
                    GUI.EndScrollView();

                    if (GUI.GetNameOfFocusedControl() == "TextArea")
                    {
                        GUI.Label(labelRect, label, EASSkin.InspectorFocusedLabelStyle);
                    }
                    else
                    {
                        GUI.Label(labelRect, label);
                    }
                }
                else
                {
                    propertyValue = EditorGUI.TextField(rect, label, (string)propertyValue);
                }
            }
        }
    }
}


