using UnityEngine;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor;
using System.Linq;

namespace EAS
{
    [EASCustomPropertyInspectorDrawer(typeof(System.Array))]
    public class EASArrayPropertyInspectorDrawer : EASPropertyInspectorDrawer
    {
        public override float GetPropertyHeight(EASBaseEvent baseEvent, string propertyName, System.Type propertyType, object propertyValue, object[] propertyAttributes)
        {
            if (GetInspectorVariable<bool>(baseEvent, GetExpandedVariableName(propertyName)))
            {
                System.Array propertyValueAsArray = propertyValue as System.Array;
                ReorderableList reorderableList = GetReorderableList(baseEvent, propertyName, propertyType, propertyValueAsArray);

                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + reorderableList.GetHeight();
            }

            return base.GetPropertyHeight(baseEvent, propertyName, propertyType, propertyValue, propertyAttributes);
        }

        protected override void OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            Rect foldoutRect = new Rect(rect.x + EASSkin.InspectorFoldoutIndentLeftMargin, rect.y, rect.width - EASSkin.InspectorFoldoutIndentLeftMargin - EASSkin.InspectorListSizeWidth, EditorGUIUtility.singleLineHeight);
            bool foldout = EditorGUI.Foldout(foldoutRect, GetInspectorVariable<bool>(baseEvent, GetExpandedVariableName(propertyName)), label, toggleOnLabelClick: true);
            SetInspectorVariable(baseEvent, GetExpandedVariableName(propertyName), foldout);

            System.Array propertyValueAsArray = propertyValue as System.Array;
            ReorderableList reorderableList = GetReorderableList(baseEvent, propertyName, propertyType, propertyValueAsArray);

            Rect arrayLengthRect = new Rect(foldoutRect.xMax, foldoutRect.y, EASSkin.InspectorListSizeWidth, foldoutRect.height);
            int arrayLength = propertyValueAsArray.Length;
            arrayLength = Mathf.Clamp(EditorGUI.DelayedIntField(arrayLengthRect, propertyValueAsArray.Length), 0, int.MaxValue);
            if (arrayLength != propertyValueAsArray.Length)
            {
                ResizeArray(ref propertyValueAsArray, propertyType.GetElementType(), arrayLength);
                reorderableList.list = propertyValueAsArray;
            }

            if (foldout)
            {
                Rect reorderableListRect = EditorGUI.IndentedRect(new Rect(rect.x, foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing, rect.width, reorderableList.GetHeight()));
                reorderableList.DoList(reorderableListRect);
            }
            propertyValue = ConvertArray(reorderableList, propertyType.GetElementType());
        }

        protected ReorderableList GetReorderableList(EASBaseEvent baseEvent, string propertyName, System.Type propertyType, System.Array propertyValue)
        {
            ReorderableList reorderableList = GetInspectorVariable<ReorderableList>(baseEvent, GetReorderableListVariableName(propertyName));
            if (reorderableList == null || reorderableList.list == null)
            {
                reorderableList = new ReorderableList(propertyValue, propertyType, draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true);
                reorderableList.multiSelect = true;

                System.Type elementType = propertyType.GetElementType();
                EASPropertyInspectorDrawer arrayElementPropertyInspectorDrawer = EASInspectorEditor.Instance.GetPropertyInspectorDrawer(elementType, getArrayAndListDrawers: false);
                reorderableList.drawNoneElementCallback = (Rect rect) =>
                {
                    GUI.Label(rect, "Array is Empty");
                };

                reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    GUIContent label = new GUIContent($"Element {index}");

                    if (arrayElementPropertyInspectorDrawer != null)
                    {
                        EditorGUI.indentLevel++;

                        object element = reorderableList.list[index];
                        if (arrayElementPropertyInspectorDrawer.OnGUIElementProperty(rect, label, baseEvent, label.text, elementType, ref element, new object[] { }))
                        {
                            System.Array array = ConvertArray(reorderableList, elementType) as System.Array;
                            array.SetValue(element, index);
                            reorderableList.list = array;
                        }

                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EASInspectorEditor.Instance.BaseOnGUIProperty(rect, label, elementType);
                    }
                };

                reorderableList.elementHeightCallback = (int index) =>
                {
                    GUIContent label = new GUIContent($"Element {index}");
                    object element = reorderableList.list[index];

                    return arrayElementPropertyInspectorDrawer != null ? arrayElementPropertyInspectorDrawer.GetPropertyHeight(baseEvent, label.text, elementType, element, new object[] { }) : EditorGUIUtility.singleLineHeight;
                };

                reorderableList.onAddCallback = (ReorderableList list) =>
                {
                    object newItem = elementType.IsValueType ? System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(elementType) : System.Activator.CreateInstance(elementType);

                    System.Array array = ConvertArray(reorderableList, elementType) as System.Array;
                    ResizeArray(ref array, elementType, array.Length + 1);
                    reorderableList.list = array;
                };

                reorderableList.onCanRemoveCallback = (ReorderableList list) =>
                {
                    return list.list.Count > 0;
                };

                reorderableList.onRemoveCallback = (ReorderableList list) =>
                {
                    System.Array array = ConvertArray(reorderableList, elementType) as System.Array;
                    if (list.selectedIndices.Count == 0)
                    {
                        RemoveAt(ref array, elementType, list.list.Count - 1);
                    }
                    else
                    {
                        for (int i = list.selectedIndices.Count - 1; i >= 0; --i)
                        {
                            RemoveAt(ref array, elementType, list.selectedIndices[i]);
                        }
                    }

                    list.list = array;
                };

                SetInspectorVariable(baseEvent, GetReorderableListVariableName(propertyName), reorderableList);
            }

            return reorderableList;
        }

        protected void ResizeArray(ref System.Array array, System.Type elementType, int newSize)
        {
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            System.Array.Copy(array, newArray, Mathf.Min(array.Length, newArray.Length));

            if (array.Length < newArray.Length)
            {
                for (int i = array.Length; i < newArray.Length; ++i)
                {
                    newArray.SetValue(elementType.IsValueType ? System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(elementType) : System.Activator.CreateInstance(elementType), i);
                }
            }

            array = newArray;
        }

        protected void RemoveAt(ref System.Array array, System.Type elementType, int index)
        {
            System.Array newArray = System.Array.CreateInstance(elementType, array.Length - 1);
            int insertionIndex = 0;
            for (int i = 0; i < array.Length; ++i)
            {
                if (index != i)
                {
                    newArray.SetValue(array.GetValue(i), insertionIndex);
                    ++insertionIndex;
                }
            }

            array = newArray;
        }

        protected object ConvertArray(ReorderableList list, System.Type elementType)
        {
            System.Array newArray = System.Array.CreateInstance(elementType, list.list.Count);
            for (int i = 0; i < newArray.Length; ++i)
            {
                newArray.SetValue(list.list[i], i); 
            }

            return newArray;
        }

        protected string GetExpandedVariableName(string propertyName) => propertyName + "Expanded";
        protected string GetReorderableListVariableName(string propertyName) => propertyName + "ReorderableList";
    }
}
