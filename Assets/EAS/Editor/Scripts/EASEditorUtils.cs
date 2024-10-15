using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Linq;
using static PlasticPipe.Server.MonitorStats;
using System.IO;

namespace EAS
{
    public class EASBaseGUIItem
    {
        protected Rect m_Rect;
        public Rect Rect { get => m_Rect; }

        protected IEASSerializable m_EASSerializable;
        public IEASSerializable EASSerializable { get => m_EASSerializable; }

        public EASBaseGUIItem(Rect rect, IEASSerializable serializable)
        {
            m_Rect = rect;
            m_EASSerializable = serializable;
        }
    }

    public class EASEventGUIItem : EASBaseGUIItem
    {
        protected Rect m_ResizeLeftRect;
        public Rect ResizeLeftRect { get => m_ResizeLeftRect; }

        protected Rect m_ResizeRightRect;
        public Rect ResizeRightRect { get => m_ResizeRightRect; }

        public bool HasResizeRects { get => m_ResizeLeftRect != null || m_ResizeRightRect != null; }
        public Vector2Int EventStartPositionAndDuration { get { EASBaseEvent baseEvent = m_EASSerializable as EASBaseEvent; return new Vector2Int(baseEvent.StartFrame, baseEvent.Duration); } }

        public EASEventGUIItem(Rect rect, EASBaseEvent baseEvent) : base(rect, baseEvent)
        {

        }

        public EASEventGUIItem(Rect rect, Rect resizeLeftRect, Rect resizeRightRect, EASBaseEvent baseEvent) : base(rect, baseEvent)
        {
            m_ResizeLeftRect = resizeLeftRect;
            m_ResizeRightRect = resizeRightRect;
        }
    }

    public enum EASTimelineFrameType
    {
        MainFrame,
        SecondaryFrame,
        TertiaryFrame,
        InvisibleFrame
    };

    public class EASTimelineFrame
    {
        protected int m_Frame;
        public int Frame { get => m_Frame; }

        protected float m_HorizontalPosition;
        public float HorizontalPosition { get => m_HorizontalPosition; }

        protected EASTimelineFrameType m_FrameType;
        public EASTimelineFrameType FrameType { get => m_FrameType; }

        protected Color m_FrameColor;
        public Color FrameColor { get => m_FrameColor; }

        public EASTimelineFrame(int frame, float horizontalPosition, EASTimelineFrameType frameType, Color frameColor)
        {
            m_Frame = frame;
            m_HorizontalPosition = horizontalPosition;
            m_FrameType = frameType;
            m_FrameColor = frameColor;
        }
    }

    public class EASDragGUIItem : EASBaseGUIItem
    {
        protected object m_Context;
        public object Context { get => m_Context; }

        public EASDragGUIItem(Rect rect, IEASSerializable serializable) : base(rect, serializable)
        {
            if (serializable is EASBaseEvent)
            {
                m_Context = (serializable as EASBaseEvent).ParentTrack;
            }
        }

        public EASDragGUIItem(Rect rect, IEASSerializable serializable, object context) : base(rect, serializable)
        {
            m_Context = context;
        }
    }

    public class EASDragInformation
    {
        protected Vector2 m_InitialPosition;
        public Vector2 InitialPosition { get => m_InitialPosition; }

        protected List<EASDragGUIItem> m_Items = new List<EASDragGUIItem>();
        public List<EASDragGUIItem> Items { get => m_Items; }

        protected bool m_DragPerformed = false;
        public bool DragPerformed { get => m_DragPerformed; }

        protected bool m_IsValidDrag = true;
        public bool IsValidDrag { get => m_IsValidDrag; }

        public enum EASDragType
        {
            NormalDrag,
            ResizeLeft,
            ResizeRight,
            TimerLine,
        }
        protected EASDragType m_DragType;
        public EASDragType DragType { get => m_DragType; }

        public EASDragInformation(Vector2 initialPosition, List<EASDragGUIItem> items, EASDragType dragType)
        {
            m_InitialPosition = initialPosition;
            m_Items = items;
            m_DragType = dragType;

            GUIUtility.hotControl = GetControlId();
        }

        public void OnDragPerformed(Vector2 position, bool isValidDrag, float threshold = float.Epsilon)
        {
            if (!m_DragPerformed)
            {
                m_DragPerformed = Vector2.Distance(position, m_InitialPosition) > threshold;
            }

            m_IsValidDrag = isValidDrag;
        }

        protected static int GetControlId() => GUIUtility.GetControlID(FocusType.Passive);

        public EventType GetEventType() => Event.current.GetTypeForControl(GetControlId());

        public bool AllowVerticalDrag()
        {
            if (m_Items.Count == 1)
            {
                return true;
            }

            for (int i = 1; i < m_Items.Count; ++i)
            {
                if (m_Items[i - 1].Rect.y != m_Items[i].Rect.y)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [System.Serializable]
    public class EASPropertyInspectorVariable
    {
        [SerializeField]
        protected IEASSerializable m_Serializable;
        public IEASSerializable Serializable { get => m_Serializable; }

        [SerializeField]
        protected string m_VariableName;
        public string VariableName { get => m_VariableName; }

        [SerializeField]
        protected object m_Variable;
        public object Variable { get => m_Variable; set => m_Variable = value; }

        public EASPropertyInspectorVariable(IEASSerializable serializable, string variableName, object variable)
        {
            m_Serializable = serializable;
            m_VariableName = variableName;
            m_Variable = variable;
        }
    }

    public class EASEditorUtils : EASUtils
    {
        public class EASTimescaledTimer
        {
            protected bool m_Started;
            public bool IsStarted { get => m_Started; }


            protected float m_ElapsedTime;
            public float ElapsedTime { get => m_ElapsedTime; }

            protected float m_Duration;

            protected float m_LastUpdateTime;

            protected bool m_IsPaused;
            public bool IsPaused { get => m_IsPaused; }

            public void Start(float duration, float elapsedTime = 0.0f)
            {
                m_Started = true;
                m_IsPaused = false;

                m_ElapsedTime = elapsedTime;
                m_LastUpdateTime = (float)EditorApplication.timeSinceStartup;
                m_Duration = duration;
            }

            public void Update(float timeScale)
            {
                if (m_Started && !m_IsPaused)
                {
                    float deltaTime = (float)EditorApplication.timeSinceStartup - m_LastUpdateTime;
                    m_ElapsedTime += deltaTime * timeScale;

                    m_LastUpdateTime = (float)EditorApplication.timeSinceStartup;
                }
            }

            public bool IsElapsed() => m_ElapsedTime >= m_Duration;

            public bool StopIfElapsed()
            {
                if (IsStarted && IsElapsed())
                {
                    m_Started = false;
                    m_ElapsedTime = m_Duration;
                    return true;
                }

                return false;
            }

            public void Pause()
            {
                m_IsPaused = true;
                m_Started = false;
            }

            public void Resume()
            {
                m_IsPaused = false;
                m_Started = true;

                m_LastUpdateTime = (float)EditorApplication.timeSinceStartup;
            }
        }

        protected static Dictionary<System.Type, EASPropertyInspectorDrawer> ms_PropertyInspectorDrawers = new Dictionary<System.Type, EASPropertyInspectorDrawer>();

        public static T GetAttribute<T>(System.Type type) where T : System.Attribute
        {
            MemberInfo memberInfo = type;
            object[] allAttributes = memberInfo.GetCustomAttributes(inherit: true);

            for (int i = 0; i < allAttributes.Length; ++i)
            {
                if (allAttributes[i] is T)
                {
                    return allAttributes[i] as T;
                }
            }

            return null;
        }

        public static T GetAttribute<T>(object[] attributes) where T : System.Attribute
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; ++i)
                {
                    if (attributes[i] is T)
                    {
                        return attributes[i] as T;
                    }
                }
            }

            return null;
        }

        public static string GetEASEventCategoryAttribute(System.Type type)
        {
            EASEventCategoryAttribute attribute = GetAttribute<EASEventCategoryAttribute>(type);
            if (attribute != null)
            {
                return attribute.Category.EndsWith('/') ? attribute.Category : attribute.Category + "/";
            }

            return string.Empty;
        }

        public static Color GetEASEventColorAttribute(System.Type type)
        {
            EASEventColorAttribute attribute = GetAttribute<EASEventColorAttribute>(type);
            if (attribute != null)
            {
                return attribute.Color;
            }

            return HexToColor("#C0C0C0");
        }

        public static Dictionary<System.Type, Color> GetAllEASEventColorAttributes()
        {
            Dictionary<System.Type, Color> allEventColorAttributes = new Dictionary<System.Type, Color>();

            List<System.Type> allEASBaseEvents = GetAllDerivedTypesOf<EASBaseEvent>();
            for (int i = 0; i < allEASBaseEvents.Count; ++i)
            {
                allEventColorAttributes.Add(allEASBaseEvents[i], GetEASEventColorAttribute(allEASBaseEvents[i]));
            }

            return allEventColorAttributes;
        }

        public static void LogAllEASEventColorAttributes()
        {
            string message = string.Empty;
            Dictionary<System.Type, Color> allEventColorAttributes = GetAllEASEventColorAttributes();

            foreach (KeyValuePair<System.Type, Color> typeAndColor in allEventColorAttributes)
            {
                message += $"{typeAndColor.Key.Name}: #{ColorToHex(typeAndColor.Value)} ";
            }
            Debug.Log(message);
        }

        public static string GetEASEventTooltipAttribute(System.Type type)
        {
            EASEventTooltipAttribute attribute = GetAttribute<EASEventTooltipAttribute>(type);
            if (attribute != null)
            {
                return attribute.Tooltip;
            }

            return string.Empty;
        }

        public static System.Type GetEASCustomEventDrawerAttribute(System.Type type)
        {
            EASCustomEventDrawerAttribute attribute = GetAttribute<EASCustomEventDrawerAttribute>(type);
            if (attribute != null)
            {
                return attribute.Type;
            }

            return null;
        }

        public static System.Type GetEASCustomEventInspectorDrawerAttribute(System.Type type)
        {
            EASCustomEventInspectorDrawerAttribute attribute = GetAttribute<EASCustomEventInspectorDrawerAttribute>(type);
            if (attribute != null)
            {
                return attribute.Type;
            }

            return null;
        }

        public static System.Type GetEASCustomPropertyInspectorDrawerAttribute(System.Type type)
        {
            EASCustomPropertyInspectorDrawerAttribute attribute = GetAttribute<EASCustomPropertyInspectorDrawerAttribute>(type);
            if (attribute != null)
            {
                return attribute.Type;
            }

            return null;
        }

        public static void GetAllEASCustomPropertyInspectorDrawers()
        {
            ms_PropertyInspectorDrawers = new Dictionary<System.Type, EASPropertyInspectorDrawer>();

            List<System.Type> allPropertyInspectorDrawers = GetAllDerivedTypesOf<EASPropertyInspectorDrawer>();
            for (int i = 0; i < allPropertyInspectorDrawers.Count; ++i)
            {
                System.Type fieldTypeOfPropertyInspectorDrawer = GetEASCustomPropertyInspectorDrawerAttribute(allPropertyInspectorDrawers[i]);
                if (fieldTypeOfPropertyInspectorDrawer != null && !ms_PropertyInspectorDrawers.ContainsKey(fieldTypeOfPropertyInspectorDrawer))
                {
                    ms_PropertyInspectorDrawers.Add(fieldTypeOfPropertyInspectorDrawer, System.Runtime.Serialization.FormatterServices.GetUninitializedObject(allPropertyInspectorDrawers[i]) as EASPropertyInspectorDrawer);
                }
            }
        }

        public static EASPropertyInspectorDrawer GetPropertyInspectorDrawer(System.Type type, bool getArrayAndListDrawers = true)
        {
            if (ms_PropertyInspectorDrawers.ContainsKey(type))
            {
                return ms_PropertyInspectorDrawers[type];
            }

            System.Type unityEngineObjectType = typeof(UnityEngine.Object);
            if (type.IsSubclassOf(unityEngineObjectType))
            {
                if (ms_PropertyInspectorDrawers.ContainsKey(unityEngineObjectType))
                {
                    return ms_PropertyInspectorDrawers[unityEngineObjectType];
                }
            }

            if (typeof(EASBaseReference).IsAssignableFrom(type))
            {
                System.Type easBaseReferenceType = typeof(EASBaseReference);
                if (ms_PropertyInspectorDrawers.ContainsKey(easBaseReferenceType))
                {
                    return ms_PropertyInspectorDrawers[easBaseReferenceType];
                }
            }

            if (getArrayAndListDrawers)
            {
                if (type.IsArray)
                {
                    if (ms_PropertyInspectorDrawers.ContainsKey(typeof(System.Array)))
                    {
                        return ms_PropertyInspectorDrawers[typeof(System.Array)];
                    }
                }

                if (type.IsGenericType)
                {
                    System.Type genericTypeDefinition = type.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(List<>))
                    {
                        if (ms_PropertyInspectorDrawers.ContainsKey(typeof(List<>)))
                        {
                            return ms_PropertyInspectorDrawers[typeof(List<>)];
                        }
                    }
                }
            }

            if (type.IsEnum)
            {
                if (ms_PropertyInspectorDrawers.ContainsKey(typeof(System.Enum)))
                {
                    return ms_PropertyInspectorDrawers[typeof(System.Enum)];
                }
            }

            System.Type rootType = type.BaseType;
            while (rootType.BaseType != null)
            {
                rootType = rootType.BaseType;
            }

            if (rootType == typeof(System.Object) && type.IsDefined(typeof(System.SerializableAttribute)))
            {
                if (ms_PropertyInspectorDrawers.ContainsKey(typeof(System.Object)))
                {
                    return ms_PropertyInspectorDrawers[typeof(System.Object)];
                }
            }

            return null;
        }

        public static string GetReadableEventNameWithCategory(System.Type type)
        {
            return FromCamelCase(GetEASEventCategoryAttribute(type)) + GetReadableEventName(type);
        }

        public static List<System.Type> GetAllDerivedTypesOf<T>()
        {
            return (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
                    from assemblyType in domainAssembly.GetTypes()
                    where typeof(T).IsAssignableFrom(assemblyType) && !assemblyType.IsAbstract
                    select assemblyType).ToList();
        }

        public static List<System.Type> GetValidEventsForTrack(GameObject root, EASBaseController owner)
        {
            List<System.Type> validEvents = new List<System.Type>();

            List<System.Type> allEvents = GetAllDerivedTypesOf<EASBaseEvent>();
            for (int i = 0; i < allEvents.Count; ++i)
            {
                EASBaseEvent baseEvent = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(allEvents[i]) as EASBaseEvent;
                if (baseEvent.IsObjectCompatible(root) && baseEvent.HasOwnerType(owner))
                {
                    validEvents.Add(allEvents[i]);
                }
            }

            return validEvents.OrderBy(e => e.Name).ToList();
        }

        public static int GetSerializableID(IEASSerializable serializable)
        {
            return (serializable as EASID).ID;
        }

        public static bool ValidateTrackGroupEventPositions(EASTrackGroup trackGroup, int trackLength)
        {
            for (int i = 0; i < trackGroup.Tracks.Count; ++i)
            {
                if (!ValidateTrackEventPositions(trackGroup.Tracks[i], trackLength))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateTrackEventPositions(EASTrack track, int trackLength)
        {
            bool[] occupiedFrames = new bool[trackLength];
            for (int i = 0; i < track.Events.Count; ++i)
            {
                EASBaseEvent baseEvent = track.Events[i] as EASBaseEvent;
                for (int j = 0; j < baseEvent.Duration; ++j)
                {
                    if (occupiedFrames[baseEvent.StartFrame + j])
                    {
                        return false;
                    }
                    occupiedFrames[baseEvent.StartFrame + j] = true;
                }
            }

            return true;
        }

        public static bool HasSpaceForNewEvents(EASTrack track, int trackLength)
        {
            int occupiedFramesCount = 0;
            for (int i = 0; i < track.Events.Count; ++i)
            {
                EASBaseEvent baseEvent = track.Events[i] as EASBaseEvent;
                occupiedFramesCount += baseEvent.Duration;
            }

            return trackLength > occupiedFramesCount;
        }

        public static bool SetStartFrameAndDurationForNewEvent(EASBaseEvent baseEvent, int trackLength)
        {
            List<Vector2Int> freeEventSpaces = GetFreeEventSpaces(baseEvent.ParentTrack, trackLength);
            if (freeEventSpaces.Count > 0)
            {
                baseEvent.StartFrame = freeEventSpaces[0].x;
                baseEvent.Duration = Mathf.Min(freeEventSpaces[0].y - freeEventSpaces[0].x, baseEvent.DefaultDuration);

                return true;
            }

            return false;
        }

        public static bool SetStartFrameAndDurationForNewEvent(EASBaseEvent baseEvent, int frameAtMousePosition, int trackLength)
        {
            List<Vector2Int> freeEventSpaces = GetFreeEventSpaces(baseEvent.ParentTrack, trackLength);
            for (int i = 0; i < freeEventSpaces.Count; ++i)
            {
                if (freeEventSpaces[i].x <= frameAtMousePosition && freeEventSpaces[i].y >= frameAtMousePosition)
                {
                    baseEvent.StartFrame = frameAtMousePosition;
                    baseEvent.Duration = Mathf.Min(freeEventSpaces[i].y - frameAtMousePosition, baseEvent.DefaultDuration);
                  
                    return true;
                }
            }

            return false;
        }

        protected static List<Vector2Int> GetFreeEventSpaces(EASTrack track, int trackLength)
        {
            List<Vector2Int> freeSpaces = new List<Vector2Int>();
            Vector2Int latestFreeSpace = Vector2Int.zero;

            for (int i = 0; i < track.Events.Count; ++i)
            {
                EASBaseEvent baseEvent = track.Events[i] as EASBaseEvent;
                if (baseEvent.StartFrame == latestFreeSpace.y)
                {
                    latestFreeSpace = Vector2Int.one * baseEvent.LastFrame;
                }
                else if (baseEvent.StartFrame > latestFreeSpace.y)
                {
                    latestFreeSpace.y = baseEvent.StartFrame;
                    freeSpaces.Add(latestFreeSpace);

                    latestFreeSpace = Vector2Int.one * baseEvent.LastFrame;
                }
            }

            if (latestFreeSpace.y < trackLength)
            {
                latestFreeSpace.y = trackLength;
                freeSpaces.Add(latestFreeSpace);
            }

            return freeSpaces;
        }

        public static List<FieldInfo> GetFields(System.Type type, params string[] additionalFields)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<FieldInfo> inspectorFields = new List<FieldInfo>();

            for (int i = 0; i < fields.Length; ++i)
            {
                if (IsInspectorField(fields[i]) || additionalFields.Contains(fields[i].Name))
                {
                    inspectorFields.Add(fields[i]);
                }
            }

            return inspectorFields.OrderByDescending(i => GetDepthLevelOfFieldInfo(type, i.DeclaringType)).ToList();
        }

        protected static bool IsInspectorField(FieldInfo fieldInfo)
        {
            if (!fieldInfo.IsPublic && !fieldInfo.IsDefined(typeof(SerializeField), false))
            {
                return false;
            }

            object[] attributes = fieldInfo.GetCustomAttributes(true);
            if (GetAttribute<HideInInspector>(attributes) != null || GetAttribute<System.NonSerializedAttribute>(attributes) != null)
            {
                return false;
            }

            if (fieldInfo.Name == "m_ID" && fieldInfo.FieldType == typeof(System.Int32))
            {
                return false;
            }

            return true;
        }

        protected static int GetDepthLevelOfFieldInfo(System.Type initialType, System.Type targetType)
        {
            if (initialType == targetType)
            {
                return 0;
            }

            return GetDepthLevelOfFieldInfo(initialType.BaseType, targetType) + 1;
        }

        public static void OnCopy(IEASSerializable serializable)
        {
            string json = OnCopyInternal(serializable);

            if (!string.IsNullOrEmpty(json))
            {
                GUIUtility.systemCopyBuffer = json;
            }
        }

        protected static string OnCopyInternal(IEASSerializable serializable)
        {
            return ConvertToJSON(serializable, copyStartFrameAndDuration: false).ToString();
        }

        public static bool CanPaste(IEASSerializable serializable, int trackLength)
        {
            if (!string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
            {
                try
                {
                    JObject copyBufferJSON = JObject.Parse(GUIUtility.systemCopyBuffer);

                    if (copyBufferJSON.HasValues)
                    {
                        JToken assemblyToken = copyBufferJSON.SelectToken("Assembly");
                        JToken typeToken = copyBufferJSON.SelectToken("Type");

                        if (assemblyToken != null && typeToken != null)
                        {
                            Assembly assembly = Assembly.Load(assemblyToken.Value<string>());
                            System.Type type = assembly.GetType(typeToken.Value<string>());

                            if (serializable is EASBaseEvent)
                            {
                                return type.IsSubclassOf(typeof(EASBaseEvent));
                            }
                            else if (serializable is EASTrack)
                            {
                                return (type.IsSubclassOf(typeof(EASBaseEvent)) && HasSpaceForNewEvents(serializable as EASTrack, trackLength)) || type.IsSubclassOf(typeof(EASBaseTrack));
                            }
                            else if (serializable is EASTrackGroup)
                            {
                                return type.IsSubclassOf(typeof(EASBaseEvent)) || type.IsSubclassOf(typeof(EASBaseTrack));
                            }
                        }
                    }
                }
                catch { }
            }

            return false;
        }

        public static void OnPaste(IEASSerializable serializable, string json = "", bool allowCreationInAnyFrame = true)
        {
            JObject copyJSON = JObject.Parse(string.IsNullOrEmpty(json) ? GUIUtility.systemCopyBuffer : json);
            Assembly assembly = Assembly.Load(copyJSON.SelectToken("Assembly").ToString());
            System.Type type = assembly.GetType(copyJSON.SelectToken("Type").ToString());
            string valueJSON = copyJSON.SelectToken("Value").ToString();

            if (serializable is EASAnimationData)
            {
                FromJSON(serializable as EASAnimationData, valueJSON);
            }
            else if (serializable is EASBaseEvent)
            {
                EASBaseEvent baseEvent = serializable as EASBaseEvent;
                if (type.IsAssignableFrom(serializable.GetType()) || serializable.GetType().IsAssignableFrom(type))
                {
                    FromJSON(baseEvent, valueJSON);
                }
                else
                {
                    FromJSON(baseEvent.ParentTrack, valueJSON, type, allowCreationInAnyFrame);
                }
            }
            else if (serializable is EASTrack || (serializable == null && type == typeof(EASTrack)))
            {
                FromJSON(serializable as EASTrack, valueJSON, type, allowCreationInAnyFrame);
            }
            else if (serializable is EASTrackGroup || (serializable == null && type == typeof(EASTrackGroup)))
            {
                FromJSON(serializable as EASTrackGroup, valueJSON, type, allowCreationInAnyFrame);
            }
        }

        public static bool CanDuplicate(IEASSerializable serializable, int trackLength)
        {
            if (serializable is EASBaseEvent)
            {
                EASBaseEvent baseEvent = serializable as EASBaseEvent;
                return HasSpaceForNewEvents(baseEvent.ParentTrack, trackLength);
            }

            return true;
        }

        public static void OnDuplicate(IEASSerializable serializable)
        {
            OnCopy(serializable);
            if (serializable is EASBaseEvent)
            {
                EASBaseEvent baseEvent = serializable as EASBaseEvent;
                OnPaste(baseEvent.ParentTrack);
            }
            else if (serializable is EASTrack)
            {
                EASTrack track = serializable as EASTrack;
                OnPaste(track.ParentTrackGroup != null ? track.ParentTrackGroup : track);
            }
            else if (serializable is EASTrackGroup)
            {
                OnPaste(serializable);
            }
        }

        protected static JObject ConvertToJSON(IEASSerializable serializable, bool copyStartFrameAndDuration)
        {
            JObject outputJSON = new JObject();
            outputJSON.Add("Assembly", serializable.GetType().Assembly.FullName);
            outputJSON.Add("Type", serializable.GetType().FullName);

            if (serializable is EASAnimationData)
            {
                outputJSON.Add("Value", ToJSON(serializable as EASAnimationData));
            }
            else if (serializable is EASBaseEvent)
            {
                outputJSON.Add("Value", ToJSON(serializable as EASBaseEvent, copyStartFrameAndDuration));
            }
            else if (serializable is EASTrack)
            {
                outputJSON.Add("Value", ToJSON(serializable as EASTrack));
            }
            else if (serializable is EASTrackGroup)
            {
                outputJSON.Add("Value", ToJSON(serializable as EASTrackGroup));
            }

            return outputJSON;
        }

        protected static JToken ToJSON(EASAnimationData animationData)
        {
            JArray outputJSON = new JArray();

            for (int i = 0; i < animationData.TracksAndGroups.Count; ++i)
            {
                outputJSON.Add(ConvertToJSON(animationData.TracksAndGroups[i], true));
            }

            return outputJSON;
        }

        protected static JToken ToJSON(EASBaseEvent baseEvent, bool copyStartFrameAndDuration)
        {
            string unityJSON = JsonUtility.ToJson(baseEvent);
            JObject unityJSONObject = JObject.Parse(unityJSON);

            JObject outputJSON = new JObject();

            List<string> additionalFields = new List<string>{ "m_Name" };
            if (copyStartFrameAndDuration)
            {
                additionalFields.Add("m_StartFrame");
                additionalFields.Add("m_Duration");
            }

            List<FieldInfo> baseEventFields = GetFields(baseEvent.GetType(), additionalFields.ToArray());
            foreach (FieldInfo field in baseEventFields)
            {
                JToken fieldNode = unityJSONObject.SelectToken(field.Name);
                outputJSON.Add(field.Name, fieldNode);
            }

            return outputJSON;
        }

        protected static JToken ToJSON(EASTrack track)
        {
            JArray outputJSON = new JArray();

            for (int i = 0; i < track.Events.Count; ++i)
            {
                outputJSON.Add(ConvertToJSON(track.Events[i], copyStartFrameAndDuration: true));
            }

            return outputJSON;
        }

        protected static JToken ToJSON(EASTrackGroup trackGroup)
        {
            JArray outputJSON = new JArray();

            for (int i = 0; i < trackGroup.Tracks.Count; ++i)
            {
                outputJSON.Add(ConvertToJSON(trackGroup.Tracks[i], copyStartFrameAndDuration: true));
            }

            return outputJSON;
        }

        protected static void FromJSON(EASAnimationData animationData, string valueJSON)
        {
            JArray tracksAndTrackGroupsJArray = JArray.Parse(valueJSON);
            JToken[] tracksAndTrackGroups = tracksAndTrackGroupsJArray.ToArray();

            for (int i = 0; i < tracksAndTrackGroups.Length; ++i)
            {
                OnPaste(null, tracksAndTrackGroups[i].ToString());
            }
        }

        protected static void FromJSON(EASBaseEvent baseEvent, string valueJSON)
        {
            JsonUtility.FromJsonOverwrite(valueJSON, baseEvent);
        }

        protected static void FromJSON(EASTrack track, string valueJSON, System.Type jsonType, bool allowCreationInAnyFrame)
        {
            if (jsonType.IsSubclassOf(typeof(EASBaseEvent)))
            {
                EASBaseEvent baseEvent = EASEditor.Instance.AddEvent(jsonType, track, allowCreationInAnyFrame);
                if (baseEvent != null)
                {
                    FromJSON(baseEvent, valueJSON);
                }
            }
            else if (jsonType == typeof(EASTrack))
            {
                EASTrack pastedTrack = track != null && track.ParentTrackGroup != null ? track.ParentTrackGroup.AddTrack() : EASEditor.Instance.AddTrack();

                JArray eventsJArray = JArray.Parse(valueJSON);
                JToken[] events = eventsJArray.ToArray();
             
                for (int i = 0; i < events.Length; ++i)
                {
                    OnPaste(pastedTrack, events[i].ToString());
                }
            }
            else
            {
                EASTrackGroup pastedTrackGroup = EASEditor.Instance.AddTrackGroup();

                JArray tracksJArray = JArray.Parse(valueJSON);
                JToken[] tracks = tracksJArray.ToArray();

                for (int i = 0; i < tracks.Length; ++i)
                {
                    OnPaste(pastedTrackGroup, tracks[i].ToString());
                }
            }
        }

        protected static void FromJSON(EASTrackGroup trackGroup, string valueJSON, System.Type jsonType, bool allowCreationInAnyFrame)
        {
            if (jsonType.IsSubclassOf(typeof(EASBaseEvent)))
            {
                bool eventCreated = false;
                for (int i = 0; i < trackGroup.Tracks.Count && !eventCreated; ++i)
                {
                    EASBaseEvent baseEvent = EASEditor.Instance.AddEvent(jsonType, trackGroup.Tracks[i], allowCreationInAnyFrame);
                    if (baseEvent != null)
                    {
                        FromJSON(baseEvent, valueJSON);
                        eventCreated = true;
                    }
                }

                if (!eventCreated)
                {
                    EASTrack track = trackGroup.AddTrack();
                    FromJSON(track, valueJSON, jsonType, allowCreationInAnyFrame);
                }
            }
            else
            {
                EASBaseTrack pastedTrack = trackGroup != null && jsonType == typeof(EASTrack) ? trackGroup.AddTrack() : EASEditor.Instance.AddTrackGroup();

                JArray eventsJArray = JArray.Parse(valueJSON);
                JToken[] events = eventsJArray.ToArray();

                for (int i = 0; i < events.Length; ++i)
                {
                    OnPaste(pastedTrack, events[i].ToString());
                }
            }
        }

        public static void ImportEASAnimationData()
        {
            string filePath = EditorUtility.OpenFilePanel("Import EASAnimationData", "", "EASjson");
            if (!string.IsNullOrEmpty(filePath))
            {
                if (EditorUtility.DisplayDialog("Import EAS Animation", "By continuing with the import, you will overwrite the current EAS Animation", "Import", "Cancel"))
                {
                    EASAnimationData currentAnimationData = EASEditor.Instance.GetAnimationData();
                    currentAnimationData.TracksAndGroups.Clear();

                    OnPaste(currentAnimationData, File.ReadAllText(filePath));
                }
            }
        }

        public static void ExportEASAnimationData()
        {
            string path = EditorUtility.SaveFilePanel("Export EASAnimationData", "", EASEditor.Instance.SelectedAnimationName + ".EASjson", "EASjson");

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, OnCopyInternal(EASEditor.Instance.GetAnimationData()));
            }
        }
    }
}
