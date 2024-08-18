using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;

namespace EAS
{
    public abstract class EASBaseArrayPropertyInspectorDrawer : EASPropertyInspectorDrawer
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

        protected override bool OnGUIProperty(Rect rect, GUIContent label, EASBaseEvent baseEvent, string propertyName, System.Type propertyType, ref object propertyValue, object[] propertyAttributes)
        {
            IList propertyValueAsIList = propertyValue as IList;

            if (propertyValueAsIList == null)
            {
                propertyValue = CreateInstance(GetElementType(propertyType));
                propertyValueAsIList = propertyValue as IList;
            }

            Rect foldoutRect = new Rect(rect.x + EASSkin.InspectorFoldoutIndentLeftMargin, rect.y, rect.width - EASSkin.InspectorFoldoutIndentLeftMargin - EASSkin.InspectorListSizeWidth, EditorGUIUtility.singleLineHeight);
            bool foldout = EditorGUI.Foldout(foldoutRect, GetInspectorVariable<bool>(baseEvent, GetExpandedVariableName(propertyName)), label, toggleOnLabelClick: true);
            SetInspectorVariable(baseEvent, GetExpandedVariableName(propertyName), foldout);

            ReorderableList reorderableList = GetReorderableList(baseEvent, propertyName, propertyType, propertyValueAsIList);

            Rect arrayLengthRect = new Rect(foldoutRect.xMax, foldoutRect.y, EASSkin.InspectorListSizeWidth, foldoutRect.height);
            int arrayLength = Mathf.Clamp(EditorGUI.DelayedIntField(arrayLengthRect, propertyValueAsIList.Count), 0, int.MaxValue);
            if (arrayLength != propertyValueAsIList.Count)
            {
                OnArrayResized(ref propertyValueAsIList, GetElementType(propertyType), arrayLength);
                reorderableList.list = propertyValueAsIList;
            }

            EditorGUI.BeginChangeCheck();

            if (foldout)
            {
                Rect reorderableListRect = EditorGUI.IndentedRect(new Rect(rect.x, foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing, rect.width, reorderableList.GetHeight()));
                reorderableList.DoList(reorderableListRect);
            }
            propertyValue = ConvertIList(reorderableList.list, GetElementType(propertyType));

            return EditorGUI.EndChangeCheck();
        }

        protected ReorderableList GetReorderableList(EASBaseEvent baseEvent, string propertyName, System.Type propertyType, IList propertyValue)
        {
            ReorderableList reorderableList = GetInspectorVariable<ReorderableList>(baseEvent, GetReorderableListVariableName(propertyName));
            if (reorderableList == null || reorderableList.list == null)
            {
                System.Type elementType = GetElementType(propertyType);
                reorderableList = new ReorderableList(propertyValue, elementType, draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true);
                reorderableList.multiSelect = true;

                EASPropertyInspectorDrawer arrayElementPropertyInspectorDrawer = EASInspectorEditor.Instance.GetPropertyInspectorDrawer(elementType, getArrayAndListDrawers: false);
                reorderableList.drawNoneElementCallback = (Rect rect) =>
                {
                    OnGUINoneElements(rect);
                };

                reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    GUIContent label = new GUIContent($"Element {index}");

                    if (arrayElementPropertyInspectorDrawer != null)
                    {
                        EditorGUI.indentLevel++;

                        object element = reorderableList.list[index];
                        if (arrayElementPropertyInspectorDrawer.OnGUIElementProperty(rect, label, baseEvent, propertyName + "." + label.text, elementType, ref element, new object[] { }))
                        {
                            IList iList = reorderableList.list;
                            iList[index] = element;

                            reorderableList.list = iList;
                            EASInspectorEditor.Instance.Repaint();
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

                    return arrayElementPropertyInspectorDrawer != null ? arrayElementPropertyInspectorDrawer.GetPropertyHeight(baseEvent, propertyName + "." + label.text, elementType, element, new object[] { }) : EditorGUIUtility.singleLineHeight;
                };

                reorderableList.onAddCallback = (ReorderableList list) =>
                {
                    IList iList = list.list;
                    OnAddElement(ref iList, elementType);

                    list.list = iList;
                };

                reorderableList.onCanRemoveCallback = (ReorderableList list) =>
                {
                    return list.list.Count > 0;
                };

                reorderableList.onRemoveCallback = (ReorderableList list) =>
                {
                    IList iList = list.list;
                    if (list.selectedIndices.Count == 0)
                    {
                        OnRemoveElement(ref iList, elementType, iList.Count - 1);
                    }
                    else
                    {
                        for (int i = list.selectedIndices.Count - 1; i >= 0; --i)
                        {
                            OnRemoveElement(ref iList, elementType, list.selectedIndices[i]);
                        }
                    }

                    list.list = iList;
                };

                SetInspectorVariable(baseEvent, GetReorderableListVariableName(propertyName), reorderableList);
            }

            return reorderableList;
        }

        protected abstract System.Type GetElementType(System.Type type);

        protected abstract object CreateInstance(System.Type elementType);

        protected abstract void OnGUINoneElements(Rect rect);
        protected abstract void OnAddElement(ref IList iList, System.Type elementType);
        protected abstract void OnRemoveElement(ref IList iList, System.Type elementType, int index);
        protected abstract void OnArrayResized(ref IList iList, System.Type elementType, int newSize);

        protected abstract object ConvertIList(IList iList, System.Type elementType);

        protected object GetDefaultValue(System.Type elementType)
        {
            return elementType.IsValueType ? System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(elementType) : null;
        }

        protected string GetExpandedVariableName(string propertyName) => propertyName + "Expanded";
        protected string GetReorderableListVariableName(string propertyName) => propertyName + "ReorderableList";
    }

    [EASCustomPropertyInspectorDrawer(typeof(System.Array))]
    public class EASArrayPropertyInspectorDrawer : EASBaseArrayPropertyInspectorDrawer
    {
        protected override System.Type GetElementType(System.Type type)
        {
            return type.GetElementType();
        }

        protected override object CreateInstance(System.Type elementType)
        {
            return System.Array.CreateInstance(elementType, 0);
        }

        protected override void OnGUINoneElements(Rect rect)
        {
            GUI.Label(rect, "Array is Empty");
        }

        protected override void OnAddElement(ref IList iList, System.Type elementType)
        {
            System.Array array = ConvertIList(iList, elementType) as System.Array;
            ResizeArray(ref array, elementType, array.Length + 1);

            iList = array;
        }

        protected override void OnRemoveElement(ref IList iList, System.Type elementType, int index)
        {
            System.Array newArray = System.Array.CreateInstance(elementType, iList.Count - 1);
            int insertionIndex = 0;
            for (int i = 0; i < iList.Count; ++i)
            {
                if (index != i)
                {
                    newArray.SetValue(iList[i], insertionIndex);
                    ++insertionIndex;
                }
            }

            iList = newArray;
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

        protected override void OnArrayResized(ref IList iList, System.Type elementType, int newSize)
        {
            System.Array array = ConvertIList(iList, elementType) as System.Array;
            ResizeArray(ref array, elementType, newSize);

            iList = array;
        }

        protected override object ConvertIList(IList iList, System.Type elementType)
        {
            System.Array newArray = System.Array.CreateInstance(elementType, iList.Count);
            for (int i = 0; i < newArray.Length; ++i)
            {
                newArray.SetValue(iList[i], i);
            }

            return newArray;
        }

        protected void ResizeArray(ref System.Array array, System.Type elementType, int newSize)
        {
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            System.Array.Copy(array, newArray, Mathf.Min(array.Length, newArray.Length));

            if (array.Length < newArray.Length)
            {
                for (int i = array.Length; i < newArray.Length; ++i)
                {
                    newArray.SetValue(GetDefaultValue(elementType), i);
                }
            }

            array = newArray;
        }
    }

    [EASCustomPropertyInspectorDrawer(typeof(System.Collections.Generic.List<>))]
    public class EASListPropertyInspectorDrawer : EASBaseArrayPropertyInspectorDrawer
    {
        protected override System.Type GetElementType(System.Type type)
        {
            return type.GetGenericArguments()[0];
        }

        protected override object CreateInstance(System.Type elementType)
        {
            return System.Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(elementType));
        }

        protected override void OnGUINoneElements(Rect rect)
        {
            GUI.Label(rect, "List is Empty");
        }

        protected override void OnAddElement(ref IList iList, System.Type elementType)
        {
            iList.Add(GetDefaultValue(elementType));
        }

        protected override void OnRemoveElement(ref IList iList, System.Type elementType, int index)
        {
            iList.RemoveAt(index);
        }

        protected override object ConvertIList(IList iList, System.Type elementType)
        {
            return iList;
        }

        protected override void OnArrayResized(ref IList iList, System.Type elementType, int newSize)
        {
            if (newSize > iList.Count)
            {
                for (int i = iList.Count; i < newSize; ++i)
                {
                    OnAddElement(ref iList, elementType);
                }
            }
            else if (newSize < iList.Count)
            {
                while (iList.Count != newSize)
                {
                    OnRemoveElement(ref iList, elementType, iList.Count - 1);
                }
            }
        }
    }
}
