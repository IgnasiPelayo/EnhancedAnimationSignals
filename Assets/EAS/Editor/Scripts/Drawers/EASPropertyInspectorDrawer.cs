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

        public bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, object propertyObject, FieldInfo property, object[] propertyAttributes, string propertyPath = "")
        {
            object propertyValue = property.GetValue(propertyObject);

            if (OnGUIProperty(rect, label, baseEvent, string.IsNullOrEmpty(propertyPath) ? property.Name : propertyPath, property.FieldType, ref propertyValue, propertyAttributes))
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

        protected Rect GetLabelRect(Rect propertyRect)
        {
            return new Rect(propertyRect.x, propertyRect.y, EditorGUIUtility.labelWidth, propertyRect.height);
        }
        
        protected virtual bool CanCopy()
        {
            return false;
        }

        protected void OnCopy(object data)
        {
            System.ValueTuple<object, string, System.Type> tuple = (System.ValueTuple<object, string, System.Type>)data;
            OnCopy(tuple.Item1, tuple.Item2, tuple.Item3);
        }

        protected virtual void OnCopy(object property, string propertyPath, System.Type propertyType)
        {
            GUIUtility.systemCopyBuffer = System.Convert.ChangeType(property, propertyType).ToString();
        }

        protected void ShowEventOptionsMenuOnRightClick(Rect propertyRect, object property, string propertyPath, System.Type propertyType)
        {
            if (EASInspectorEditor.Instance.CanShowEventOptionsMenu(propertyRect))
            {
                EASInspectorEditor.Instance.ShowEventOptionsMenu(property, propertyPath, propertyType, CanCopy() ? OnCopy : null, (property, propertyPath, propertyType));
            }
        }
    }
}