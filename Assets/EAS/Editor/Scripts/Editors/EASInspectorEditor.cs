using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace EAS
{
    public class EASInspectorEditor : EditorWindow
    {
        protected static EASInspectorEditor m_Instance;
        public static EASInspectorEditor Instance { get { if (m_Instance == null) { m_Instance = GetWindow<EASInspectorEditor>(); } return m_Instance; } }

        [SerializeField]
        protected Dictionary<System.Type, EASEventInspectorDrawer> m_EventInspectorDrawers = new Dictionary<System.Type, EASEventInspectorDrawer>();

        [SerializeField]
        protected Dictionary<System.Type, EASPropertyInspectorDrawer> m_PropertyInspectorDrawers = new Dictionary<System.Type, EASPropertyInspectorDrawer>();

        [SerializeField]
        protected List<EASPropertyInspectorVariable> m_PropertyInspectorVariables = new List<EASPropertyInspectorVariable>();

        protected Vector2 m_Scroll;

        [MenuItem("Window/Animation/Enhanced Animation Signals Inspector")]
        public static void OpenWindow()
        {
            EASInspectorEditor window = EditorWindow.GetWindow<EASInspectorEditor>(false, "EAS Inspector");
            window.titleContent = new GUIContent(window.titleContent.text, EASSkin.Icon("d_UnityEditor.InspectorWindow"), window.titleContent.tooltip);
            window.Show();

            m_Instance = window;
        }

        protected void OnEnable()
        {
            m_EventInspectorDrawers = EASEditorUtils.GetAllEASCustomEventInspectorDrawers();
            m_PropertyInspectorDrawers = EASEditorUtils.GetAllEASCustomPropertyInspectorDrawers();
        }

        protected void OnGUI()
        {
            if (EASEditor.Instance == null)
            {
                return;
            }

            OnGUIInspector();
        }

        protected void OnGUIInspector()
        {
            Rect windowRect = position;
            windowRect.x = windowRect.y = 0;

            List<EASSerializable> selectedEvents = EASEditor.Instance.GetSelected<EASBaseEvent>();
            if (selectedEvents.Count == 0)
            {
                return;
            }

            if (selectedEvents.Count == 1)
            {
                EASBaseEvent baseEvent = selectedEvents[0] as EASBaseEvent;

                Rect headerRect = new Rect(windowRect.x, windowRect.y, windowRect.width, EASSkin.InspectorHeaderHeight);
                OnGUIInspectorHeader(headerRect, baseEvent);

                Rect inspectorRect = Rect.MinMaxRect(headerRect.x, headerRect.yMax + EASSkin.InspectorUpperMargin,
                    headerRect.xMax - EASSkin.InspectorRightMargin, windowRect.yMax - EASSkin.InspectorBottomMargin);
                OnGUIInspectorTooltip(headerRect, baseEvent, ref inspectorRect);

                EditorGUI.indentLevel++;

                GUILayout.BeginArea(inspectorRect);

                m_Scroll = GUILayout.BeginScrollView(m_Scroll);

                inspectorRect = new Rect(0, 0, inspectorRect.width, inspectorRect.height);

                EASEventInspectorDrawer eventInspectorDrawer = GetEventInspectorDrawer(baseEvent.GetType());
                if (eventInspectorDrawer != null)
                {
                    eventInspectorDrawer.OnGUIInspector(inspectorRect, baseEvent);
                }
                else
                {
                    BaseOnGUIInspector(inspectorRect, baseEvent);
                }

                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            else
            {

            }

            if (Event.current.type == EventType.MouseDown)
            {
                GUI.FocusControl(null);
                Repaint();
            }
        }

        protected void OnGUIInspectorHeader(Rect rect, EASBaseEvent baseEvent)
        {
            Color eventColor = EASEditorUtils.GetEASEventColorAttribute(baseEvent.GetType());
            EditorGUI.DrawRect(rect, eventColor);

            GUIStyle labelGUIStyle = new GUIStyle(EditorStyles.label);
            labelGUIStyle.alignment = TextAnchor.MiddleLeft;

            labelGUIStyle.normal.textColor = labelGUIStyle.hover.textColor = labelGUIStyle.active.textColor = ExtendedGUI.ExtendedGUI.GetContrastingLabelColor(EASEditorUtils.GetEASEventColorAttribute(baseEvent.GetType()));
            Rect eventLabelRect = Rect.MinMaxRect(rect.x + EASSkin.InspectorHeaderLeftMargin, rect.y, rect.xMax - EASSkin.InspectorRightMargin, rect.yMax);
            GUI.Label(eventLabelRect, new GUIContent(EASUtils.GetReadableEventName(baseEvent.GetType(), replaceEvent: false)), labelGUIStyle);

            labelGUIStyle.fontSize = 10;
            labelGUIStyle.alignment = TextAnchor.LowerRight;
            Rect idRect = Rect.MinMaxRect(eventLabelRect.x, eventLabelRect.y, eventLabelRect.xMax, eventLabelRect.yMax - 2);
            GUI.Label(idRect, new GUIContent($"ID {EASEditorUtils.GetSerializableID(baseEvent)}"), labelGUIStyle);
        }

        protected void OnGUIInspectorTooltip(Rect headerRect, EASBaseEvent baseEvent, ref Rect inspectorRect)
        {
            GUIContent eventTooltipGUIContent = new GUIContent(EASEditorUtils.GetEASEventTooltipAttribute(baseEvent.GetType()));
            if (!string.IsNullOrEmpty(eventTooltipGUIContent.text))
            {
                GUIStyle eventTooltipGUIStyle = EASSkin.InspectorTooltipStyle;

                float eventTooltipHeight = eventTooltipGUIStyle.CalcHeight(eventTooltipGUIContent, headerRect.width);
                Rect eventTooltipRect = new Rect(headerRect.x, headerRect.yMax, headerRect.width, eventTooltipHeight);
                GUI.Box(eventTooltipRect, eventTooltipGUIContent, eventTooltipGUIStyle);

                inspectorRect.y += eventTooltipRect.height;
                inspectorRect.height -= eventTooltipRect.height;
            }
        }

        public void BaseOnGUIInspector(Rect rect, EASBaseEvent baseEvent)
        {
            List<FieldInfo> fields = GetFields(baseEvent.GetType());

            bool hasChanged = false;
            for (int i = 0; i < fields.Count; ++i)
            {
                object[] fieldAttributes = fields[i].GetCustomAttributes(true);
                OnGUIHeaderAndSpaces(rect, fieldAttributes);

                GUIContent label = GetPropertyLabel(fields[i], fieldAttributes);

                EditorGUI.BeginDisabledGroup(EASEditorUtils.GetAttribute<CustomAttributes.ReadOnlyAttribute>(fieldAttributes) != null);

                EASPropertyInspectorDrawer propertyInspectorDrawer = GetPropertyInspectorDrawer(fields[i]);
                if (propertyInspectorDrawer != null)
                {
                    FieldInfo property = fields[i];
                    Rect controlRect = EditorGUILayout.GetControlRect(hasLabel: true, height: propertyInspectorDrawer.GetPropertyHeight(baseEvent, property.Name, property.FieldType, property.GetValue(baseEvent), fieldAttributes));
                    hasChanged |= propertyInspectorDrawer.OnGUIProperty(controlRect, label, baseEvent, baseEvent, fields[i], fieldAttributes);
                }
                else
                {
                    Rect controlRect = EditorGUILayout.GetControlRect(hasLabel: true, height: BaseGetPorpertyHeight());
                    hasChanged |= BaseOnGUIProperty(controlRect, label, fields[i].FieldType);
                }

                EditorGUI.EndDisabledGroup();
            }

            if (hasChanged)
            {
                Debug.Log("Has changed");
            }
        }

        public void OnGUIHeaderAndSpaces(Rect rect, object[] fieldAttributes)
        {
            SpaceAttribute spaceAttribute = EASEditorUtils.GetAttribute<SpaceAttribute>(fieldAttributes);
            if (spaceAttribute != null)
            {
                EditorGUILayout.GetControlRect(hasLabel: false, height: spaceAttribute.height);
            }

            HeaderAttribute headerAttribute = EASEditorUtils.GetAttribute<HeaderAttribute>(fieldAttributes);
            if (headerAttribute != null)
            {
                int previousIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;

                GUIContent headerGUIContent = new GUIContent(headerAttribute.header);
                GUIStyle headerGUISytle = new GUIStyle(EditorStyles.boldLabel);
                headerGUISytle.alignment = TextAnchor.LowerLeft;

                Rect headerRect = EditorGUILayout.GetControlRect(hasLabel: true, height: headerGUISytle.CalcHeight(headerGUIContent, rect.width) + EASSkin.InspectorHeaderSpacing);
                headerRect = EditorGUI.IndentedRect(headerRect);
                GUI.Label(headerRect, headerGUIContent, headerGUISytle);

                EditorGUI.indentLevel = previousIndentLevel;
            }
        }

        public float GetHeaderAndSpacesHeight(object[] fieldAttributes)
        {
            float height = 0;

            SpaceAttribute spaceAttribute = EASEditorUtils.GetAttribute<SpaceAttribute>(fieldAttributes);
            if (spaceAttribute != null)
            {
                height += spaceAttribute.height;
            }

            HeaderAttribute headerAttribute = EASEditorUtils.GetAttribute<HeaderAttribute>(fieldAttributes);
            if (headerAttribute != null)
            {
                GUIContent headerGUIContent = new GUIContent(headerAttribute.header);
                GUIStyle headerGUISytle = new GUIStyle(EditorStyles.boldLabel);
                headerGUISytle.alignment = TextAnchor.LowerLeft;

                Vector2 size = headerGUISytle.CalcSize(headerGUIContent);
                height += headerGUISytle.CalcHeight(headerGUIContent, size.x) + EASSkin.InspectorHeaderSpacing;
            }

            return height;
        }

        public GUIContent GetPropertyLabel(FieldInfo property, object[] propertyAttributes)
        {
            GUIContent label = new GUIContent();

            InspectorNameAttribute inspectorNameAttribute = EASEditorUtils.GetAttribute<InspectorNameAttribute>(propertyAttributes);
            if (inspectorNameAttribute != null)
            {
                label.text = inspectorNameAttribute.displayName;
            }
            else
            {
                label.text = EASUtils.FromCamelCase(property.Name.StartsWith("m_") ? property.Name.Substring(2, property.Name.Length - 2) : property.Name);
            }

            TooltipAttribute tooltipAttribute = EASEditorUtils.GetAttribute<TooltipAttribute>(propertyAttributes);
            if (tooltipAttribute != null)
            {
                label.tooltip = tooltipAttribute.tooltip;
            }

            return label;
        }

        public float BaseGetPorpertyHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public bool BaseOnGUIProperty(Rect rect, GUIContent label, System.Type propertyType)
        {
            GUI.Label(rect, new GUIContent($"({propertyType.Name}) {label.text} needs a EASPropertyInspectorDrawer", label.tooltip));
            return false;
        }

        public EASEventInspectorDrawer GetEventInspectorDrawer(System.Type type)
        {
            if (m_EventInspectorDrawers.ContainsKey(type))
            {
                return m_EventInspectorDrawers[type];
            }

            return null;
        }

        public EASPropertyInspectorDrawer GetPropertyInspectorDrawer(System.Type type, bool getArrayAndListDrawers = true)
        {
            if (m_PropertyInspectorDrawers.ContainsKey(type))
            {
                return m_PropertyInspectorDrawers[type];
            }

            System.Type unityEngineObjectType = typeof(UnityEngine.Object);
            if (type.IsSubclassOf(unityEngineObjectType))
            {
                if (m_PropertyInspectorDrawers.ContainsKey(unityEngineObjectType))
                {
                    return m_PropertyInspectorDrawers[unityEngineObjectType];
                }
            }

            if (getArrayAndListDrawers)
            {
                if (type.IsArray)
                {
                    if (m_PropertyInspectorDrawers.ContainsKey(typeof(System.Array)))
                    {
                        return m_PropertyInspectorDrawers[typeof(System.Array)];
                    }
                }

                if (type.IsGenericType)
                {
                    System.Type genericTypeDefinition = type.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(List<>))
                    {
                        if (m_PropertyInspectorDrawers.ContainsKey(typeof(List<>)))
                        {
                            return m_PropertyInspectorDrawers[typeof(List<>)];
                        }
                    }
                }
            }

            if (type.IsEnum)
            {
                if (m_PropertyInspectorDrawers.ContainsKey(typeof(System.Enum)))
                {
                    return m_PropertyInspectorDrawers[typeof(System.Enum)];
                }
            }

            if ((type.BaseType == typeof(System.Object) || type.BaseType == typeof(System.ValueType)) && type.IsDefined(typeof(System.SerializableAttribute)))
            {
                if (m_PropertyInspectorDrawers.ContainsKey(typeof(System.Object)))
                {
                    return m_PropertyInspectorDrawers[typeof(System.Object)];
                }
            }

            return null;
        }

        public EASPropertyInspectorDrawer GetPropertyInspectorDrawer(FieldInfo property)
        {
            return GetPropertyInspectorDrawer(property.FieldType);
        }

        public List<FieldInfo> GetFields(System.Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<FieldInfo> inspectorFields = new List<FieldInfo>();

            for (int i = 0; i < fields.Length; ++i)
            {
                if (IsInspectorField(fields[i]))
                {
                    inspectorFields.Add(fields[i]);
                }
            }

            return inspectorFields;
        }

        protected bool IsInspectorField(FieldInfo fieldInfo)
        {
            if (!fieldInfo.IsPublic && !fieldInfo.IsDefined(typeof(SerializeField), false))
            {
                return false;
            }

            object[] attributes = fieldInfo.GetCustomAttributes(true);
            if (EASEditorUtils.GetAttribute<HideInInspector>(attributes) != null || EASEditorUtils.GetAttribute<System.NonSerializedAttribute>(attributes) != null)
            {
                return false;
            }

            if (fieldInfo.Name == "m_ID" && fieldInfo.FieldType == typeof(System.Int32))
            {
                return false;
            }

            return true;
        }

        public T GetVariable<T>(EASSerializable serializable, string variableName)
        {
            for (int i = 0; i < m_PropertyInspectorVariables.Count; ++i)
            {
                if (m_PropertyInspectorVariables[i].Serializable == serializable && m_PropertyInspectorVariables[i].VariableName == variableName)
                {
                    return (T)m_PropertyInspectorVariables[i].Variable;
                }
            }

            T defaultVariableAsT = default(T);
            m_PropertyInspectorVariables.Add(new EASPropertyInspectorVariable(serializable, variableName, defaultVariableAsT));
            return defaultVariableAsT;
        }

        public void SetVariable<T>(EASSerializable serializable, string variableName, T variable)
        {
            for (int i = 0; i < m_PropertyInspectorVariables.Count; ++i)
            {
                if (m_PropertyInspectorVariables[i].Serializable == serializable && m_PropertyInspectorVariables[i].VariableName == variableName)
                {
                    m_PropertyInspectorVariables[i].Variable = variable;
                    return;
                }
            }

            m_PropertyInspectorVariables.Add(new EASPropertyInspectorVariable(serializable, variableName, variable));
        }
    }
}


