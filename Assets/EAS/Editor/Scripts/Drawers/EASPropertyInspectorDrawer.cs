using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace EAS
{
    public abstract class EASPropertyInspectorDrawer
    {
        public virtual float GetPropertyHeight(EASBaseEvent baseEvent, string propertyName, System.Type propertyType, object propertyValue, object[] propertyAttributes)
        {
            return EASInspectorEditor.Instance.BaseGetPorpertyHeight();
        }

        public bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, FieldInfo property, object[] propertyAttributes)
        {
            object propertyValue = property.GetValue(baseEvent);

            EditorGUI.BeginChangeCheck();

            OnGUIProperty(rect, label, baseEvent, property.Name, property.FieldType, ref propertyValue, propertyAttributes);

            if (EditorGUI.EndChangeCheck())
            {
                property.SetValue(baseEvent, propertyValue);
                return true;
            }

            return false;
        }

        public bool OnGUIElementProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            EditorGUI.BeginChangeCheck();

            OnGUIProperty(rect, label, baseEvent, propertyName, propertyType, ref propertyValue, propertyAttributes);

            return EditorGUI.EndChangeCheck();
        }

        protected abstract void OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes);

        protected T GetInspectorVariable<T>(EASSerializable serializable, string propertyName)
        {
            return EASInspectorEditor.Instance.GetVariable<T>(serializable, propertyName);
        }

        protected void SetInspectorVariable<T>(EASSerializable serializable, string propertyName, T value)
        {
            EASInspectorEditor.Instance.SetVariable<T>(serializable, propertyName, value);
        }
    }
}