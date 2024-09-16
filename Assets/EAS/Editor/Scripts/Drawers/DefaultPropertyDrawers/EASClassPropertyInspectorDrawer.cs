using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(System.Object))]
    public class EASClassPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        public override float GetPropertyHeight(EASBaseEvent baseEvent, string propertyName, System.Type propertyType, object propertyValue, object[] propertyAttributes)
        {
            if (GetInspectorVariable<bool>(baseEvent, propertyName))
            {
                float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                List<FieldInfo> fields = EASEditorUtils.GetFields(propertyType);
                for (int i = 0; i < fields.Count; ++i)
                {
                    object[] fieldAttributes = fields[i].GetCustomAttributes(true);
                    height += EASInspector.Instance.GetHeaderAndSpacesHeight(fieldAttributes);

                    EASPropertyInspectorDrawer propertyInspectorDrawer = EASInspector.Instance.GetPropertyInspectorDrawer(fields[i]);
                    if (propertyInspectorDrawer != null)
                    {
                        FieldInfo property = fields[i];
                        string propertyPath = propertyName + "." + property.Name;

                        height += propertyInspectorDrawer.GetPropertyHeight(baseEvent, propertyPath, property.FieldType, property.GetValue(propertyValue), fieldAttributes) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else
                    {
                        height += EASInspector.Instance.BaseGetPorpertyHeight();
                    }
                }

                return height;
            }

            return base.GetPropertyHeight(baseEvent, propertyName, propertyType, propertyValue, propertyAttributes);
        }

        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            Rect foldoutRect = new Rect(rect.x + EASSkin.InspectorFoldoutIndentLeftMargin, rect.y, rect.width - EASSkin.InspectorFoldoutIndentLeftMargin, EditorGUIUtility.singleLineHeight);
            SetInspectorVariable(baseEvent, propertyName, EditorGUI.Foldout(foldoutRect, GetInspectorVariable<bool>(baseEvent, propertyName), label, toggleOnLabelClick: true));

            ShowEventOptionsMenuOnRightClick(foldoutRect, baseEvent, propertyValue, propertyName, propertyType);

            if (GetInspectorVariable<bool>(baseEvent, propertyName))
            {
                EditorGUI.indentLevel++;

                List<FieldInfo> fields = EASEditorUtils.GetFields(propertyType);

                Rect propertyRect = new Rect(rect.x, foldoutRect.y, rect.width, foldoutRect.height);

                bool hasChanged = false;
                for (int i = 0; i < fields.Count; ++i)
                {
                    object[] fieldAttributes = fields[i].GetCustomAttributes(true);
                    EASInspector.Instance.OnGUIHeaderAndSpaces(rect, fieldAttributes);

                    GUIContent propertyLabel = EASInspector.Instance.GetPropertyLabel(fields[i], fieldAttributes);

                    EditorGUI.BeginDisabledGroup(EASEditorUtils.GetAttribute<CustomAttributes.ReadOnlyAttribute>(fieldAttributes) != null);

                    EASPropertyInspectorDrawer propertyInspectorDrawer = EASInspector.Instance.GetPropertyInspectorDrawer(fields[i]);
                    if (propertyInspectorDrawer != null)
                    {
                        FieldInfo property = fields[i];
                        string propertyPath = propertyName + "." + property.Name;

                        propertyRect = new Rect(propertyRect.x, propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing, propertyRect.width,
                            propertyInspectorDrawer.GetPropertyHeight(baseEvent, propertyPath, property.FieldType, property.GetValue(propertyValue), fieldAttributes));
                        hasChanged |= propertyInspectorDrawer.OnGUIProperty(propertyRect, propertyLabel, baseEvent, propertyValue, fields[i], fieldAttributes, propertyPath);
                    }
                    else
                    {
                        propertyRect = new Rect(propertyRect.x, propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing, propertyRect.width, EASInspector.Instance.BaseGetPorpertyHeight());
                        hasChanged |= EASInspector.Instance.BaseOnGUIProperty(propertyRect, label, fields[i].FieldType);
                    }

                    EditorGUI.EndDisabledGroup();
                }

                EditorGUI.indentLevel--;

                return hasChanged;
            }

            return false;
        }

        public override bool CanPaste(EASBaseEvent baseEvent, object property, string propertyName, string copiedPropertyPath, ref FieldInfo field, ref object finalInstance)
        {
            int indexOfPoint = copiedPropertyPath.IndexOf(".");
            if (indexOfPoint != -1)
            {
                string className = copiedPropertyPath.Substring(0, indexOfPoint);
                if (className == propertyName)
                {
                    propertyName = copiedPropertyPath.Substring(copiedPropertyPath.IndexOf(className + ".") + (className + ".").Length);
                    return EASInspector.Instance.CanPasteProperty(baseEvent, property, propertyName, ref field, ref finalInstance);
                }
            }

            return false;
        }
    }
}


