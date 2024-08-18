using UnityEngine;
using UnityEditor;

namespace EAS
{
    public abstract class EASVectorPropertyInspectorDrawer<T> : EASPropertyInspectorDrawer
    {
        protected override void OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            float rectWidth = rect.width;
            rect = EditorGUI.IndentedRect(rect);

            Rect labelFieldRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth - (rectWidth - rect.width), rect.height);
            GUI.Label(labelFieldRect, label);

            T propertyValueAsVector = (T)propertyValue;
            Rect vectorRect = Rect.MinMaxRect(labelFieldRect.xMax, rect.y, rect.xMax, rect.yMax);

            propertyValue = OnGUIVectorProperty(vectorRect, propertyValueAsVector);
        }

        protected abstract T OnGUIVectorProperty(Rect vectorRect, T vectorValue);
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

        protected override void OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            Rect foldoutRect = new Rect(rect.x + EASSkin.InspectorFoldoutIndentLeftMargin, rect.y, rect.width - EASSkin.InspectorFoldoutIndentLeftMargin, EditorGUIUtility.singleLineHeight);
            SetInspectorVariable(baseEvent, propertyName, EditorGUI.Foldout(foldoutRect, GetInspectorVariable<bool>(baseEvent, propertyName), label, toggleOnLabelClick: true));

            if (GetInspectorVariable<bool>(baseEvent, propertyName))
            {
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
            }
        }
    }
}
