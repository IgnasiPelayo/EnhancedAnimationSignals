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

        public bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, object propertyObject, FieldInfo property, object[] propertyAttributes)
        {
            object propertyValue = property.GetValue(propertyObject);
            if (OnGUIProperty(rect, label, baseEvent, property.Name, property.FieldType, ref propertyValue, propertyAttributes))
            {
                property.SetValue(propertyObject, propertyValue);
                return true;
            }

            return false;
        }

        public bool OnGUIElementProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            return OnGUIProperty(rect, label, baseEvent, propertyName, propertyType, ref propertyValue, propertyAttributes);
        }

        protected abstract bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes);

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