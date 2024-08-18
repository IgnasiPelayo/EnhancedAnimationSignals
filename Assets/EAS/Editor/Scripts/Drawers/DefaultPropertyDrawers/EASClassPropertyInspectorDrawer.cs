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

                List<FieldInfo> fields = EASInspectorEditor.Instance.GetFields(propertyType);
                for (int i = 0; i < fields.Count; ++i)
                {
                    object[] fieldAttributes = fields[i].GetCustomAttributes(true);
                    height += EASInspectorEditor.Instance.GetHeaderAndSpacesHeight(fieldAttributes);

                    EASPropertyInspectorDrawer propertyInspectorDrawer = EASInspectorEditor.Instance.GetPropertyInspectorDrawer(fields[i]);
                    if (propertyInspectorDrawer != null)
                    {
                        FieldInfo property = fields[i];
                        height += propertyInspectorDrawer.GetPropertyHeight(baseEvent, property.Name, property.FieldType, property.GetValue(propertyValue), fieldAttributes) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else
                    {
                        height += EASInspectorEditor.Instance.BaseGetPorpertyHeight();
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

            if (GetInspectorVariable<bool>(baseEvent, propertyName))
            {
                EditorGUI.indentLevel++;

                List<FieldInfo> fields = EASInspectorEditor.Instance.GetFields(propertyType);

                Rect propertyRect = new Rect(rect.x, foldoutRect.y, rect.width, foldoutRect.height);

                bool hasChanged = false;
                for (int i = 0; i < fields.Count; ++i)
                {
                    object[] fieldAttributes = fields[i].GetCustomAttributes(true);
                    EASInspectorEditor.Instance.OnGUIHeaderAndSpaces(rect, fieldAttributes);

                    GUIContent propertyLabel = EASInspectorEditor.Instance.GetPropertyLabel(fields[i], fieldAttributes);

                    EditorGUI.BeginDisabledGroup(EASEditorUtils.GetAttribute<CustomAttributes.ReadOnlyAttribute>(fieldAttributes) != null);

                    EASPropertyInspectorDrawer propertyInspectorDrawer = EASInspectorEditor.Instance.GetPropertyInspectorDrawer(fields[i]);
                    if (propertyInspectorDrawer != null)
                    {
                        FieldInfo property = fields[i];
                        propertyRect = new Rect(propertyRect.x, propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing, propertyRect.width, propertyInspectorDrawer.GetPropertyHeight(baseEvent, property.Name, property.FieldType, property.GetValue(propertyValue), fieldAttributes));
                        hasChanged |= propertyInspectorDrawer.OnGUIProperty(propertyRect, propertyLabel, baseEvent, propertyValue, fields[i], fieldAttributes);
                    }
                    else
                    {
                        propertyRect = new Rect(propertyRect.x, propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing, propertyRect.width, EASInspectorEditor.Instance.BaseGetPorpertyHeight());
                        hasChanged |= EASInspectorEditor.Instance.BaseOnGUIProperty(propertyRect, label, fields[i].FieldType);
                    }

                    EditorGUI.EndDisabledGroup();
                }

                EditorGUI.indentLevel--;

                return hasChanged;
            }

            return false;
        }
    }
}


