using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomAttributes
{
    [CustomPropertyDrawer(typeof(ArrayElementTitleAttribute))]
    public class ArrayElementTitleAttributeDrawer : PropertyDrawer
    {
        protected ArrayElementTitleAttribute Attribute { get => (ArrayElementTitleAttribute)attribute; }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string titleLabel = label.text;
            if (Attribute.ElementTitleVariableName.Contains("$"))
            {
                string allText = Attribute.ElementTitleVariableName;
                var reg = new Regex(@"(?<open>\$).*?(?<final-open>\$)");
                var matches = reg.Matches(allText).Cast<Match>().Select(m => m.Groups["final"].Value).ToList();

                foreach (var item in matches)
                {
                    allText = allText.Replace("$" + item + "$", GetVariableTitle(property, item));
                }

                titleLabel = allText;
            }
            else
            {
                string newlabel = GetVariableTitle(property, Attribute.ElementTitleVariableName);
                if (!string.IsNullOrEmpty(newlabel))
                {
                    titleLabel = newlabel;
                }
            }

            EditorGUI.PropertyField(position, property, new GUIContent(titleLabel, label.tooltip), true);
        }

        protected string GetVariableTitle(SerializedProperty property, string varName)
        {
            string fullPath = property.propertyPath + "." + varName;
            SerializedProperty varProperty = property.serializedObject.FindProperty(fullPath);
            return GetTitle(varProperty);
        }

        private string GetTitle(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString();
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    {
                        if (property.objectReferenceValue == null)
                            return "No Object Reference";
                        else
                        {
                            string typeName = property.objectReferenceValue.GetType().ToString();
                            string objectName = property.objectReferenceValue.ToString().Replace("(" + typeName + ")", "");

                            return GetReadableName(objectName, "");
                        }
                    }
                case SerializedPropertyType.LayerMask:
                    break;
                case SerializedPropertyType.Enum:
                    if (property.enumValueIndex >= 0 && property.enumValueIndex <= property.enumNames.Length)
                    {
                        return property.enumNames[property.enumValueIndex];
                    }
                    break;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    break;
                case SerializedPropertyType.ArraySize:
                    break;
                case SerializedPropertyType.Character:
                    break;
                case SerializedPropertyType.AnimationCurve:
                    break;
                case SerializedPropertyType.Bounds:
                    break;
                case SerializedPropertyType.Gradient:
                    break;
                case SerializedPropertyType.Quaternion:
                    break;
                default:
                    break;
            }
            return "";
        }

        protected string GetReadableName(string typeName, string textToRemove)
        {
            typeName = typeName.Replace("Home.", "");

            if (!string.IsNullOrEmpty(textToRemove))
            {
                typeName = typeName.Replace(textToRemove, "");
            }

            return Regex.Replace(typeName, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
        }
    }
}
