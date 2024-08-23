using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace EAS
{
    public class EASInspectorEditor : EditorWindow
    {
        protected static EASInspectorEditor m_Instance;
        public static EASInspectorEditor Instance { get { if (m_Instance == null) { OpenWindow(); } return m_Instance; } }
        public static bool HasInstance { get => m_Instance != null; }

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
            m_PropertyInspectorDrawers = EASEditorUtils.GetAllEASCustomPropertyInspectorDrawers();

            if (!HasInstance)
            {
                OpenWindow();
            }
        }

        protected void OnGUI()
        {
            if (!EASEditor.HasInstance)
            {
                return;
            }

            OnGUIInspector();
        }

        protected void OnGUIInspector()
        {
            Rect windowRect = position;
            windowRect.x = windowRect.y = 0;

            List<IEASSerializable> selectedEvents = EASEditor.Instance.GetSelected<EASBaseEvent>();
            if (selectedEvents.Count == 0)
            {
                List<IEASSerializable> selectedBaseTracks = EASEditor.Instance.GetSelected<EASBaseTrack>();
                if (selectedBaseTracks.Count == 0)
                {
                    return;
                }
                else
                {
                    OnGUIMultipleTracks(windowRect, selectedBaseTracks);
                }
            }
            else if (selectedEvents.Count == 1)
            {
                OnGUIEvent(windowRect, selectedEvents[0] as EASBaseEvent);
            }
            else
            {
                OnGUIMultipleEvents(windowRect, selectedEvents);
            }

            if (Event.current.type == EventType.MouseDown)
            {
                GUI.FocusControl(null);
                Repaint();
            }
        }

        protected void OnGUIMultipleTracks(Rect windowRect, List<IEASSerializable> tracks)
        {
            Rect inspectorRect = new Rect(windowRect.x, windowRect.y, windowRect.width, tracks.Count * (EASSkin.InspectorHeaderHeight + EditorGUIUtility.standardVerticalSpacing));
            if (inspectorRect.height >= windowRect.height)
            {
                inspectorRect.width -= 15;
            }

            m_Scroll = GUI.BeginScrollView(windowRect, m_Scroll, inspectorRect);

            Rect headerRect = new Rect(inspectorRect.x, inspectorRect.y, inspectorRect.width, EASSkin.InspectorHeaderHeight);
            for (int i = 0; i < tracks.Count; ++i)
            {
                OnGUIInspectorHeader(headerRect, tracks[i] as EASBaseTrack);
                headerRect.y = headerRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            }

            GUI.EndScrollView();
        }

        protected void OnGUIEvent(Rect windowRect, EASBaseEvent baseEvent)
        {
            Rect headerRect = new Rect(windowRect.x, windowRect.y, windowRect.width, EASSkin.InspectorHeaderHeight);
            OnGUIInspectorHeader(headerRect, baseEvent);

            Rect inspectorRect = Rect.MinMaxRect(headerRect.x, headerRect.yMax + EASSkin.InspectorUpperMargin,
                headerRect.xMax - EASSkin.InspectorRightMargin, windowRect.yMax - EASSkin.InspectorBottomMargin);
            OnGUIInspectorTooltip(headerRect, baseEvent, ref inspectorRect);
            OnGUIErrorMessage(headerRect, baseEvent, ref inspectorRect);

            EditorGUI.indentLevel++;

            GUILayout.BeginArea(inspectorRect);

            m_Scroll = GUILayout.BeginScrollView(m_Scroll);

            inspectorRect = new Rect(0, 0, inspectorRect.width, inspectorRect.height);
            BaseOnGUIInspector(inspectorRect, baseEvent);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        protected void OnGUIMultipleEvents(Rect windowRect, List<IEASSerializable> events)
        {
            Rect inspectorRect = new Rect(windowRect.x, windowRect.y, windowRect.width, (events.Count + 1) * (EASSkin.InspectorHeaderHeight + EditorGUIUtility.standardVerticalSpacing));
            if (inspectorRect.height >= windowRect.height)
            {
                inspectorRect.width -= 15;
            }

            m_Scroll = GUI.BeginScrollView(windowRect, m_Scroll, inspectorRect);

            Rect headerRect = new Rect(inspectorRect.x, inspectorRect.y, inspectorRect.width, EASSkin.InspectorHeaderHeight);
            for (int i = 0; i < events.Count; ++i)
            {
                EASBaseEvent baseEvent = events[i] as EASBaseEvent;

                OnGUIInspectorHeader(headerRect, baseEvent);
                headerRect.y = headerRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            }

            headerRect = Rect.MinMaxRect(headerRect.x + EASSkin.InspectorHeaderLeftMargin, headerRect.y + EditorGUIUtility.standardVerticalSpacing, headerRect.xMax, headerRect.yMax + EditorGUIUtility.standardVerticalSpacing);
            GUI.Label(headerRect, new GUIContent("EASEvents can't be multi-edited"));

            GUI.EndScrollView();
        }

        protected void OnGUIInspectorHeader(Rect rect, EASID baseID, Color color, string name)
        {
            EditorGUI.DrawRect(rect, color);

            GUIStyle labelGUIStyle = new GUIStyle(EditorStyles.label);
            labelGUIStyle.alignment = TextAnchor.MiddleLeft;

            labelGUIStyle.normal.textColor = labelGUIStyle.hover.textColor = labelGUIStyle.active.textColor = ExtendedGUI.ExtendedGUI.GetContrastingLabelColor(color);
            Rect labelRect = Rect.MinMaxRect(rect.x + EASSkin.InspectorHeaderLeftMargin, rect.y, rect.xMax, rect.yMax);

            labelRect.width = Mathf.Min(EditorStyles.label.CalcSize(new GUIContent(name)).x, labelRect.width);

            EditorGUI.BeginChangeCheck();
            name = EditorGUI.DelayedTextField(labelRect, name, labelGUIStyle);
            if (EditorGUI.EndChangeCheck())
            {
                baseID.Name = string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name) ? baseID.DefaultName : name;

                GUI.FocusControl(null);
                EASEditor.Instance.Repaint();
            }

            labelGUIStyle.fontSize = 10;
            labelGUIStyle.alignment = TextAnchor.LowerRight;
            Rect idRect = Rect.MinMaxRect(labelRect.x, labelRect.y, rect.xMax - EASSkin.InspectorRightMargin, labelRect.yMax - 2);
            GUI.Label(idRect, new GUIContent($"ID {baseID.ID}"), labelGUIStyle);
        }

        protected void OnGUIInspectorHeader(Rect rect, EASBaseEvent baseEvent)
        {
            Color eventColor = EASEditorUtils.GetEASEventColorAttribute(baseEvent.GetType());
            string eventName = baseEvent.Name == baseEvent.DefaultName ? EASUtils.GetReadableEventName(baseEvent.GetType(), replaceEvent: false) : baseEvent.Name;
            OnGUIInspectorHeader(rect, baseEvent, eventColor, eventName);
        }

        protected void OnGUIInspectorHeader(Rect rect, EASBaseTrack baseTrack)
        {
            Color trackColor = (baseTrack is EASTrackGroup) ? EASSkin.HierarchyTrackGroupColor : EASSkin.HierarchyTrackColor;
            OnGUIInspectorHeader(rect, baseTrack, trackColor, baseTrack.Name);
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

        protected void OnGUIErrorMessage(Rect headerRect, EASBaseEvent baseEvent, ref Rect inspectorRect)
        {
            GUIContent errorMessageGUIConent = new GUIContent(baseEvent.GetErrorMessage(EASEditor.Instance.Controller));
            if (baseEvent.HasError(errorMessageGUIConent.text))
            {
                GUIStyle errorMessageGUIStyle = EASSkin.InspectorErrorMessageStyle;

                float errorMessageHeight = errorMessageGUIStyle.CalcHeight(errorMessageGUIConent, headerRect.width);
                Rect errorMessageRect = new Rect(headerRect.x, inspectorRect.yMax - errorMessageHeight + EASSkin.InspectorBottomMargin, headerRect.width, errorMessageHeight);

                EditorGUI.DrawRect(errorMessageRect, EASSkin.ErrorColor);
                GUI.Label(errorMessageRect, errorMessageGUIConent, errorMessageGUIStyle);

                inspectorRect.height -= errorMessageHeight;
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
                baseEvent.OnValidate();

                EASEditor.Instance.Repaint();
                EditorUtility.SetDirty(EASEditor.Instance.Controller.Data);
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

            if (typeof(EASBaseReference).IsAssignableFrom(type))
            {
                System.Type easBaseReferenceType = typeof(EASBaseReference);
                if (m_PropertyInspectorDrawers.ContainsKey(easBaseReferenceType))
                {
                    return m_PropertyInspectorDrawers[easBaseReferenceType];
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

            System.Type rootType = type.BaseType;
            while (rootType.BaseType != null)
            {
                rootType = rootType.BaseType;
            }

            if (rootType == typeof(System.Object) && type.IsDefined(typeof(System.SerializableAttribute)))
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

        public T GetVariable<T>(IEASSerializable serializable, string variableName)
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

        public void SetVariable<T>(IEASSerializable serializable, string variableName, T variable)
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

        public bool CanShowEventOptionsMenu(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition);
        }

        public void ShowEventOptionsMenu(EASBaseEvent baseEvent, object property, string propertyPath, System.Type propertyType, GenericMenu.MenuFunction2 copyFunction, object copyData)
        {
            GenericMenu propertyOptionsMenu = new GenericMenu();

            propertyOptionsMenu.AddItem(new GUIContent("Copy Property Path"), false, () => { GUIUtility.systemCopyBuffer = propertyPath; });

            if (copyFunction != null)
            {
                propertyOptionsMenu.AddSeparator("");
                ExtendedGUI.ExtendedGUI.GenericMenuAddItem(propertyOptionsMenu, new GUIContent("Copy"), true, copyFunction, copyData);
                ExtendedGUI.ExtendedGUI.GenericMenuAddItem(propertyOptionsMenu, new GUIContent("Paste"), ShowPasteOption(propertyType), () => { OnPasteProperty(baseEvent, baseEvent, propertyPath); });
            }

            propertyOptionsMenu.ShowAsContext();
        }

        public bool ShowPasteOption(System.Type propertyType)
        {
            EASPropertyInspectorDrawer propertyInspectorDrawer = GetPropertyInspectorDrawer(propertyType);
            if (propertyInspectorDrawer != null)
            {
                return propertyInspectorDrawer.PasteValueFromClipboardIsValid();
            }

            return false;
        }

        public void OnPasteProperty(EASBaseEvent baseEvent, object property, string propertyPath)
        {
            FieldInfo field = null;
            object finalInstance = null;

            if (CanPasteProperty(baseEvent, property, propertyPath, ref field, ref finalInstance))
            {
                EASPropertyInspectorDrawer propertyInspectorDrawer = GetPropertyInspectorDrawer(field);
                field.SetValue(finalInstance, propertyInspectorDrawer.GetPasteValueFromClipboard());

                EditorUtility.SetDirty(EASEditor.Instance.Controller.Data);
            }
        }

        public bool CanPasteProperty(EASBaseEvent baseEvent, object property, string propertyPath, ref FieldInfo refField, ref object refFinalInstance)
        {
            List<FieldInfo> fields = GetFields(property.GetType());
            for (int i = 0; i < fields.Count; ++i)
            {
                EASPropertyInspectorDrawer propertyInspectorDrawer = GetPropertyInspectorDrawer(fields[i]);
                if (propertyInspectorDrawer != null)
                {
                    if (propertyInspectorDrawer.CanPaste(baseEvent, fields[i].GetValue(property), fields[i].Name, propertyPath, ref refField, ref refFinalInstance))
                    {
                        if (refField == null || refFinalInstance == null)
                        {
                            refField = fields[i];
                            refFinalInstance = property;
                        }

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
