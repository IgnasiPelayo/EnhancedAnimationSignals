using UnityEngine;
using UnityEditor;

namespace EAS
{
    public abstract class EASVectorPropertyInspectorDrawer<T> : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            float rectWidth = rect.width;
            rect = EditorGUI.IndentedRect(rect);

            Rect labelFieldRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth - (rectWidth - rect.width), rect.height);
            GUI.Label(labelFieldRect, label);
            ShowEventOptionsMenuOnRightClick(labelFieldRect, baseEvent, propertyValue, propertyName, propertyType);

            T propertyValueAsVector = (T)propertyValue;
            Rect vectorRect = Rect.MinMaxRect(labelFieldRect.xMax, rect.y, rect.xMax, rect.yMax);

            EditorGUI.BeginChangeCheck();

            propertyValue = OnGUIVectorProperty(vectorRect, propertyValueAsVector);

            return EditorGUI.EndChangeCheck();
        }

        protected abstract T OnGUIVectorProperty(Rect vectorRect, T vectorValue);

        protected override bool CanCopy()
        {
            return true;
        }

        protected override void OnCopy(object property, string propertyPath, System.Type propertyType)
        {
            base.OnCopy(property, propertyPath, propertyType);
            GUIUtility.systemCopyBuffer = $"{propertyType.Name}{GUIUtility.systemCopyBuffer}";
        }

        protected string[] GetVectorValues(string vectorAsString, System.Type vectorType)
        {
            string vectorTypeNameWithParenthesis = vectorType.Name + "(";
            if (vectorAsString.StartsWith(vectorTypeNameWithParenthesis) && vectorAsString.EndsWith(")"))
            {
                string innerValues = vectorAsString.Substring(vectorTypeNameWithParenthesis.Length, vectorAsString.Length - vectorTypeNameWithParenthesis.Length - 1);
                return innerValues.Split(',');
            }

            return null;
        }
    }

    [EASCustomPropertyInspectorDrawer(typeof(Vector2))]
    public class EASVector2PropertyInspectorDrawer : EASVectorPropertyInspectorDrawer<Vector2>
    {
        protected override Vector2 OnGUIVectorProperty(Rect vectorRect, Vector2 vectorValue)
        {
            float[] vectorValues = new float[] { vectorValue.x, vectorValue.y };

            vectorRect.width = 2.0f * Mathf.FloorToInt(vectorRect.width / 3.0f);
            EditorGUI.MultiFloatField(vectorRect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y") }, vectorValues);

            return new Vector2(vectorValues[0], vectorValues[1]);
        }

        public override object GetPasteValueFromClipboard()
        {
            string[] vectorValues = GetVectorValues(GUIUtility.systemCopyBuffer, typeof(Vector2));
            if (vectorValues != null && vectorValues.Length == 2)
            {
                if (float.TryParse(vectorValues[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(vectorValues[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y))
                {
                    return new Vector2(x, y);
                }
            }

            return null;
        }
    }

    [EASCustomPropertyInspectorDrawer(typeof(Vector2Int))]
    public class EASVector2IntPropertyInspectorDrawer : EASVectorPropertyInspectorDrawer<Vector2Int>
    {
        protected override Vector2Int OnGUIVectorProperty(Rect vectorRect, Vector2Int vectorValue)
        {
            int[] vectorValues = new int[] { vectorValue.x, vectorValue.y };
            EditorGUI.MultiIntField(vectorRect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y") }, vectorValues);

            return new Vector2Int(vectorValues[0], vectorValues[1]);
        }

        public override object GetPasteValueFromClipboard()
        {
            string[] vectorValues = GetVectorValues(GUIUtility.systemCopyBuffer, typeof(Vector2Int));
            if (vectorValues != null && vectorValues.Length == 2)
            {
                if (int.TryParse(vectorValues[0].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int x) &&
                    int.TryParse(vectorValues[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int y))
                {
                    return new Vector2Int(x, y);
                }
            }

            return null;
        }
    }

    [EASCustomPropertyInspectorDrawer(typeof(Vector3))]
    public class EASVector3PropertyInspectorDrawer : EASVectorPropertyInspectorDrawer<Vector3>
    {
        protected override Vector3 OnGUIVectorProperty(Rect vectorRect, Vector3 vectorValue)
        {
            float[] vectorValues = new float[] { vectorValue.x, vectorValue.y, vectorValue.z };
            EditorGUI.MultiFloatField(vectorRect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") }, vectorValues);

            return new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]);
        }

        public override object GetPasteValueFromClipboard()
        {
            string[] vectorValues = GetVectorValues(GUIUtility.systemCopyBuffer, typeof(Vector3));
            if (vectorValues != null && vectorValues.Length == 3)
            {
                if (float.TryParse(vectorValues[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(vectorValues[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(vectorValues[2].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z))
                {
                    return new Vector3(x, y, z);
                }
            }

            return null;
        }
    }

    [EASCustomPropertyInspectorDrawer(typeof(Vector3Int))]
    public class EASVector3IntPropertyInspectorDrawer : EASVectorPropertyInspectorDrawer<Vector3Int>
    {
        protected override Vector3Int OnGUIVectorProperty(Rect vectorRect, Vector3Int vectorValue)
        {
            int[] vectorValues = new int[] { vectorValue.x, vectorValue.y, vectorValue.z };
            EditorGUI.MultiIntField(vectorRect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") }, vectorValues);

            return new Vector3Int(vectorValues[0], vectorValues[1], vectorValues[2]);
        }

        public override object GetPasteValueFromClipboard()
        {
            string[] vectorValues = GetVectorValues(GUIUtility.systemCopyBuffer, typeof(Vector3Int));
            if (vectorValues != null && vectorValues.Length == 3)
            {
                if (int.TryParse(vectorValues[0].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int x) &&
                    int.TryParse(vectorValues[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int y) &&
                    int.TryParse(vectorValues[2].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int z))
                {
                    return new Vector3Int(x, y, z);
                }
            }

            return null;
        }
    }

    [EASCustomPropertyInspectorDrawer(typeof(Vector4))]
    public class EASVector4PropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        public override float GetPropertyHeight(EASBaseEvent baseEvent, string propertyName, System.Type propertyType, object propertyValue, object[] propertyAttributes)
        {
            if (GetInspectorVariable<bool>(baseEvent, propertyName))
            {
                return EditorGUIUtility.singleLineHeight + 4 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
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
                EditorGUI.BeginChangeCheck();

                Vector4 propertyValueAsVector4 = (Vector4)propertyValue;

                Rect vectorFieldRect = EditorGUI.IndentedRect(new Rect(rect.x, foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing, rect.width, EditorGUIUtility.singleLineHeight));
                propertyValueAsVector4.x = EditorGUI.FloatField(vectorFieldRect, new GUIContent("X"), propertyValueAsVector4.x);

                vectorFieldRect.y = vectorFieldRect.yMax + EditorGUIUtility.standardVerticalSpacing; 
                propertyValueAsVector4.y = EditorGUI.FloatField(vectorFieldRect, new GUIContent("Y"), propertyValueAsVector4.y);

                vectorFieldRect.y = vectorFieldRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                propertyValueAsVector4.z = EditorGUI.FloatField(vectorFieldRect, new GUIContent("Z"), propertyValueAsVector4.z);

                vectorFieldRect.y = vectorFieldRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                propertyValueAsVector4.w = EditorGUI.FloatField(vectorFieldRect, new GUIContent("W"), propertyValueAsVector4.w);

                propertyValue = propertyValueAsVector4;

                return EditorGUI.EndChangeCheck();
            }

            return false;
        }

        protected override bool CanCopy()
        {
            return true;
        }

        protected override void OnCopy(object property, string propertyPath, System.Type propertyType)
        {
            base.OnCopy(property, propertyPath, propertyType);
            if (propertyType.Equals(typeof(Vector4)))
            {
                GUIUtility.systemCopyBuffer = $"{propertyType.Name}{GUIUtility.systemCopyBuffer}";
            }
            else
            {
                GUIUtility.systemCopyBuffer = GUIUtility.systemCopyBuffer.Replace(",", ".");
            }
        }

        public override object GetPasteValueFromClipboard()
        {
            string vectorTypeNameWithParenthesis = "Vector4(";
            if (GUIUtility.systemCopyBuffer.StartsWith(vectorTypeNameWithParenthesis) && GUIUtility.systemCopyBuffer.EndsWith(")"))
            {
                string innerValues = GUIUtility.systemCopyBuffer.Substring(vectorTypeNameWithParenthesis.Length, GUIUtility.systemCopyBuffer.Length - vectorTypeNameWithParenthesis.Length - 1);
                string[] vectorValues = innerValues.Split(',');

                if (vectorValues.Length == 4)
                {
                    if (float.TryParse(vectorValues[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(vectorValues[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y) &&
                        float.TryParse(vectorValues[2].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z) &&
                        float.TryParse(vectorValues[3].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float w))
                    {
                        return new Vector4(x, y, z, w);
                    }
                }
            }

            return null;
        }
    }
}
