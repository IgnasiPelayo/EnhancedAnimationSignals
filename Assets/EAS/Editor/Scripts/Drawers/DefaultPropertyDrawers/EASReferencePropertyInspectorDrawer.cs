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
            if (baseReference != null)
            {
                UnityEngine.Object propertyValueAsObject = baseReference.ResolveReference(EASEditor.Instance.Controller) as UnityEngine.Object;

                EditorGUI.BeginChangeCheck();

                propertyValueAsObject = EditorGUI.ObjectField(rect, label, propertyValueAsObject, baseReference.GetObjectType(), allowSceneObjects: !baseReference.IsGlobal);

                if (EditorGUI.EndChangeCheck())
                {
                    baseReference.UpdateValue(propertyValueAsObject, EASEditor.Instance.Controller.DataRoot);
                    return true;
                }
            }
            else
            {
                string referenceName = propertyType.Name.Substring(0, propertyType.Name.Length - 2);
                GUI.Label(EditorGUI.IndentedRect(rect), new GUIContent($"{propertyName} is null. Declare it as {referenceName}<{propertyType.GetGenericArguments()[0]}> {propertyName} = new {referenceName}<{propertyType.GetGenericArguments()[0]}>"));
            }

            return false;
        }
    }
}
