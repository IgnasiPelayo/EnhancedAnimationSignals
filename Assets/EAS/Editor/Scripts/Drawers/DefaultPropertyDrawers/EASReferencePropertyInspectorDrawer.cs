using UnityEngine;
using UnityEditor;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(EASBaseReference))]
    public class EASReferencePropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            ShowEventOptionsMenuOnRightClick(rect, baseEvent, propertyValue, propertyName, propertyType);

            EASBaseReference baseReference = propertyValue as EASBaseReference;
            UnityEngine.Object propertyValueAsObject = baseReference.Resolve(EASEditor.Instance.Controller) as UnityEngine.Object;

            EditorGUI.BeginChangeCheck();

            propertyValueAsObject = EditorGUI.ObjectField(rect, label, propertyValueAsObject, baseReference.GetObjectType(), allowSceneObjects: true);

            if (EditorGUI.EndChangeCheck())
            {
                baseReference.UpdateValue(propertyValueAsObject, EASEditor.Instance.Controller.DataRoot);
                return true;
            }

            return false;
        }
    }
}
