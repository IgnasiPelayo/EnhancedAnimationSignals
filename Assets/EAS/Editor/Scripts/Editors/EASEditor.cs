using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExtendedGUI;

namespace EAS
{
    public class EASEditor : EditorWindow
    {
        protected static EASEditor m_Instance;
        public static EASEditor Instance { get { if (m_Instance == null) { m_Instance = GetWindow<EASEditor>(); } return m_Instance; } }

        protected int m_InstanceId = 0;
        protected const int kControllerMaxParentDepth = 2;
        public EASBaseController Controller { get => GetComponent<EASBaseController>(); }

        [SerializeField]
        protected bool m_LockSelection = false;
        public bool LockSelection { get => m_LockSelection; set { m_LockSelection = value; if (!m_LockSelection) { OnSelectionChange(); } } }

        [SerializeField]
        protected bool m_Playing;
        public bool Playing { get => m_Playing; set { if (m_Playing != value) { m_Playing = value; OnPlayModeChanged(); } } }

        [SerializeField]
        protected bool m_Loop;
        public bool Loop { get => m_Loop; set { if (m_Loop != value) { m_Loop = value; } } }

        public int CurrentFrame { get; set; }

        [SerializeField]
        protected bool m_Mute = true;
        public bool Mute { get => m_Mute; set { if (m_Mute != value) { m_Mute = value; } } }

        [SerializeField]
        protected bool m_ShowParticleSystems;
        public bool ShowParticleSystems { get => m_ShowParticleSystems; set { if (m_ShowParticleSystems != value) { m_ShowParticleSystems = value; } } }

        public string SelectedAnimationName { get => m_Hierarchy.SelectedAnimationName; }

        [SerializeReference]
        protected List<EASSerializable> m_SelectedObjects = new List<EASSerializable>();

        [SerializeField]
        protected EASEditorControls m_Controls = new EASEditorControls();

        [SerializeField]
        protected EASEditorHierarchy m_Hierarchy = new EASEditorHierarchy();

        [SerializeField]
        protected EASEditorTimeline m_Timeline = new EASEditorTimeline();
        public EASEditorTimeline Timeline { get => m_Timeline; }

        [SerializeField]
        protected Vector2 m_MousePosition;

        [SerializeField]
        protected Dictionary<System.Type, EASEventDrawer> m_EventDrawers = new Dictionary<System.Type, EASEventDrawer>();

        internal T GetComponent<T>() where T : Component
        {
            Object obj = EditorUtility.InstanceIDToObject(m_InstanceId);
            if (obj != null)
            {
                T selectedComponent = ((GameObject)obj).GetComponentInChildren<T>();

                if (selectedComponent != null)
                {
                    int parentDepth = 0;
                    GameObject selectedGameObject = obj as GameObject;
                    GameObject selectedControllerGameObject = selectedComponent.gameObject;
                    while (selectedControllerGameObject != selectedGameObject)
                    {
                        selectedControllerGameObject = selectedControllerGameObject.transform.parent.gameObject;
                        ++parentDepth;
                    }

                    return parentDepth > kControllerMaxParentDepth ? null : selectedComponent;
                }
            }

            return null;
        }

        [MenuItem("Window/Animation/Enhanced Animation Signals &a")]
        public static void OpenWindow()
        {
            EASEditor window = EditorWindow.GetWindow<EASEditor>(false, "EAS");
            window.titleContent = new GUIContent(window.titleContent.text, EASSkin.CustomIcon("EAS"), window.titleContent.tooltip);
            window.Show();

            m_Instance = window;
        }

        protected virtual void ShowButton(Rect rect)
        {
            bool newLock = GUI.Toggle(rect, LockSelection, GUIContent.none, EASSkin.LockStyle);
            if (LockSelection != newLock)
            {
                LockSelection = newLock;
            }
        }

        protected void OnEnable()
        {
            OnSelectionChange();

            if (Controller != null)
            {

            }

            m_EventDrawers = EASEditorUtils.GetAllEASCustomEventDrawers();
            SelectObject(null, singleSelection: true);
        }

        protected void Update()
        {
            if (Controller != null)
            {
                m_Hierarchy.OnUpdate();

                if (m_Playing)
                {
                    Repaint();
                }
            }
        }

        protected void OnGUI()
        {
            if (Controller == null)
            {
                NoControllerGUI();
                return;
            }

            RenderOnGUI();
        }

        protected void OnDestroy()
        {
            m_SelectedObjects.Clear();
            EditorUtility.SetDirty(this);
        }

        protected void NoControllerGUI()
        {
            GameObject activeGameObject = m_LockSelection ? EditorUtility.InstanceIDToObject(m_InstanceId) as GameObject : Selection.activeGameObject;

            if (activeGameObject != null)
            {
                GUILayout.FlexibleSpace();

                ExtendedGUILayout.CenterLabelHorizontally(new GUIContent($"To begin a new Enhanced Animation Signal Preview with {activeGameObject.name}, create a EASBaseController"));

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                Animator selectedAnimator = GetComponent<Animator>();
                if (selectedAnimator != null)
                {
                    if (ExtendedGUILayout.Button(new GUIContent("Create EASController"), GUI.skin.button, fixedContentSize: true, isSelected: false, enabled: selectedAnimator != null))
                    {
                        activeGameObject.AddComponent<EASController>();
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (selectedAnimator == null)
                {
                    GUILayout.Space(5);
                    ExtendedGUILayout.CenterLabelHorizontally(new GUIContent("Can't find a valid Animator in the selected gameobject"), new GUIStyle("CN StatusError"));
                }

                GUILayout.FlexibleSpace();
            }
            else
            {
                ExtendedGUILayout.CenterLabel("No GameObject selected");
            }
        }

        protected void RenderOnGUI()
        {
            m_MousePosition = Event.current.mousePosition;

            Rect windowRect = position;
            windowRect.x = windowRect.y = 0;

            Rect toolbarRect = new Rect(0, 0, windowRect.width, EASSkin.ControlToolbarHeight);
            Rect hierarchyRect = new Rect(0, toolbarRect.yMax, windowRect.width / 4.0f, windowRect.height - toolbarRect.height);
            Rect timelineRect = Rect.MinMaxRect(hierarchyRect.xMax, hierarchyRect.y, windowRect.xMax, hierarchyRect.yMax);

            m_Hierarchy.OnGUI(hierarchyRect);
            m_Timeline.OnGUI(timelineRect);
            m_Controls.OnGUI(toolbarRect);
        }

        protected void OnSelectionChange()
        {
            if (!LockSelection)
            {
                if (AnimationMode.InAnimationMode())
                {
                    AnimationMode.StopAnimationMode();
                }

                if (Selection.activeGameObject != null)
                {
                    int oldInstanceId = m_InstanceId;
                    m_InstanceId = Selection.activeGameObject.GetInstanceID();

                    if (oldInstanceId != m_InstanceId)
                    {
                        if (Controller != null)
                        {
                            Playing = false;
                        }

                        SelectObject(null, singleSelection: true);
                    }
                }
            }
            else if (EditorUtility.InstanceIDToObject(m_InstanceId) == null)
            {
                LockSelection = false;
            }

            Repaint();
        }

        protected void OnPlayModeChanged()
        {

        }

        public void OnAnimationChanged()
        {
            m_SelectedObjects.Clear();

            m_Timeline.OnAnimationChanged();
        }

        public string[] GetAnimationNames()
        {
            return Controller.GetAnimationNames();
        }

        public EASAnimationInformation GetAnimationInformation()
        {
            object animation = Controller.GetAnimation(SelectedAnimationName);

            return new EASAnimationInformation(animation, Controller.GetLength(animation), Controller.GetFrameRate(animation));
        }

        public int GetCurrentAnimationFrames()
        {
            return Mathf.RoundToInt(GetAnimationInformation().Frames);
        }

        public List<EASSerializable> GetTracksAndGroups()
        {
            return Controller.Data.GetTracksAndGroups(SelectedAnimationName);
        }

        public EASTrackGroup AddTrackGroup()
        {
            EASTrackGroup trackGroup = Controller.Data.AddTrackGroup(SelectedAnimationName);
            EditorUtility.SetDirty(Controller.Data);

            return trackGroup;
        }

        public EASTrack AddTrack()
        {
            EASTrack track = Controller.Data.AddTrack(SelectedAnimationName);
            EditorUtility.SetDirty(Controller.Data);

            return track;
        }

        public bool RemoveTrackOrGroup(EASSerializable trackOrGroup)
        {
            bool success = Controller.Data.RemoveTrackOrGroup(SelectedAnimationName, trackOrGroup);
            EditorUtility.SetDirty(Controller.Data);

            return success;
        }

        public bool RemoveEvent(EASBaseEvent baseEvent)
        {
            bool success = baseEvent.ParentTrack.Events.Remove(baseEvent);
            EditorUtility.SetDirty(Controller.Data);

            return success;
        }

        public void MoveEvent(EASBaseEvent baseEvent, EASTrack track)
        {
            RemoveEvent(baseEvent);
            track.AddEvent(baseEvent);

            EditorUtility.SetDirty(Controller.Data);
        }

        public EASBaseEvent AddEvent(System.Type eventType, EASBaseTrack baseTrack)
        {
            EASTrack track = null;
            if (baseTrack == null)
            {
                track = AddTrack();
            }
            else if (baseTrack is EASTrackGroup)
            {
                track = (baseTrack as EASTrackGroup).AddTrack();
            }
            else if (baseTrack is EASTrack)
            {
                track = baseTrack as EASTrack;
            }

            int trackLength = GetCurrentAnimationFrames();

            EASBaseEvent baseEvent = track.CreateEvent(eventType);
            if (EASEditorUtils.HasSpaceForNewEvents(track, GetCurrentAnimationFrames()) && m_Timeline.IsMouseOnTimeline(m_MousePosition) ? 
                    EASEditorUtils.SetStartFrameAndDurationForNewEvent(baseEvent, m_Timeline.GetSafeFrameAtPosition(m_MousePosition.x), trackLength) :
                    EASEditorUtils.SetStartFrameAndDurationForNewEvent(baseEvent, trackLength))
            {
                baseEvent = track.AddEvent(baseEvent);

                EditorUtility.SetDirty(Controller.Data);
                return baseEvent;
            }

            return null;
        }

        public bool IsSelected(EASSerializable selectedObject)
        {
            return m_SelectedObjects.Contains(selectedObject);
        }

        public bool HasMultipleSelectionModifier()
        {
            return Event.current.modifiers == EventModifiers.Shift || Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command;
        }

        public void SelectObject(EASSerializable selectedObject, bool singleSelection)
        {
            if (selectedObject == null)
            {
                m_SelectedObjects.Clear();
            }
            else
            {
                if (singleSelection)
                {
                    m_SelectedObjects.Clear();
                    m_SelectedObjects.Add(selectedObject);
                }
                else
                {
                    if (IsSelected(selectedObject))
                    {
                        m_SelectedObjects.Remove(selectedObject);
                    }
                    else
                    {
                        m_SelectedObjects.Add(selectedObject);
                    }
                }
            }

            GUI.FocusControl(null);
            Repaint();

            if (EASInspectorEditor.Instance != null)
            {
                EASInspectorEditor.Instance.Repaint();
            }
        }

        public List<EASSerializable> GetSelected<T>()
        {
            List<EASSerializable> selectedObjects = new List<EASSerializable>();
            for (int i = 0; i < m_SelectedObjects.Count; ++i)
            {
                if (m_SelectedObjects[i] is T)
                {
                    selectedObjects.Add(m_SelectedObjects[i]);
                }
            }

            return selectedObjects;
        }

        public bool HasEventsSelected()
        {
            for (int i = 0; i < m_SelectedObjects.Count; ++i)
            {
                if (m_SelectedObjects[i] is EASBaseEvent)
                {
                    return true;
                }
            }

            return false;
        }

        public void ShowTrackGroupOptionsMenu(EASTrackGroup trackGroup)
        {
            GenericMenu trackOptionsMenu = new GenericMenu();

            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Copy"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Paste"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Duplicate"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Delete"), !trackGroup.Locked, () => { RemoveTrackOrGroup(trackGroup); });
            trackOptionsMenu.AddSeparator("");
            trackOptionsMenu.AddItem(new GUIContent($"{(trackGroup.Locked ? "Unl" : "L")}ock _L"), false, () => { trackGroup.Locked = !trackGroup.Locked; });
            trackOptionsMenu.AddItem(new GUIContent($"{(trackGroup.Muted ? "Unm" : "M")}ute _M"), false, () => { trackGroup.Muted = !trackGroup.Muted; });
            trackOptionsMenu.AddSeparator("");
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Add Track"), !trackGroup.Locked, () => { trackGroup.AddTrack(); });
            AddEventMenuOption(trackOptionsMenu, trackGroup);

            trackOptionsMenu.ShowAsContext();
        }

        public void ShowTrackOptionsMenu(EASTrack track)
        {
            GenericMenu trackOptionsMenu = new GenericMenu();

            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Copy"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Paste"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Duplicate"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Delete"), !track.Locked && !track.ParentTrackGroupLocked, () => { RemoveTrackOrGroup(track); });
            trackOptionsMenu.AddSeparator("");
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent($"{(track.Locked ? "Unl" : "L")}ock _L"), !track.ParentTrackGroupLocked, () => { track.Locked = !track.Locked; });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent($"{(track.Muted ? "Unm" : "M")}ute _M"), !track.ParentTrackGroupMuted, () => { track.Muted = !track.Muted; });
            trackOptionsMenu.AddSeparator("");

            if (track.ParentTrackGroup == null)
            {
                ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Move to Track Group"), true, () => { EASTrackGroup trackGroup = AddTrackGroup(); trackGroup.AddTrack(track); RemoveTrackOrGroup(track); });
            }
            AddEventMenuOption(trackOptionsMenu, track);

            trackOptionsMenu.ShowAsContext();
        }

        public void ShowOptionsMenu()
        {
            GenericMenu optionsMenu = new GenericMenu();

            optionsMenu.AddItem(new GUIContent("Add Track"), false, () => { AddTrack(); });
            optionsMenu.AddItem(new GUIContent("Add Track Group"), false, () => { AddTrackGroup(); });
            optionsMenu.AddSeparator("");
            AddEventMenuOption(optionsMenu, null);

            optionsMenu.ShowAsContext();
        }

        public void AddEventMenuOption(GenericMenu menu, EASBaseTrack baseTrack)
        {
            bool canAddEvents = true;
            if (baseTrack != null)
            {
                canAddEvents = baseTrack is EASTrack ? !baseTrack.Locked && !(baseTrack as EASTrack).ParentTrackGroupLocked && EASEditorUtils.HasSpaceForNewEvents(baseTrack as EASTrack, GetCurrentAnimationFrames()) : !baseTrack.Locked;
            }

            List<System.Type> eventTypes = EASEditorUtils.GetValidEventsForTrack(Controller.DataRootGameObject, Controller);
            for (int i = 0; i < eventTypes.Count; ++i)
            {
                System.Type type = eventTypes[i];
                ExtendedGUI.ExtendedGUI.GenericMenuAddItem(menu, new GUIContent($"Add Event/{EASEditorUtils.GetReadableEventNameWithCategory(type)}"),
                    canAddEvents, () => { AddEvent(type, baseTrack); });
            }
        }

        public void ShowEventOptionsMenu(EASBaseEvent baseEvent)
        {
            GenericMenu eventOptionsMenu = new GenericMenu();

            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(eventOptionsMenu, new GUIContent("Copy"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(eventOptionsMenu, new GUIContent("Paste"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(eventOptionsMenu, new GUIContent("Duplicate"), false, () => { });
            ExtendedGUI.ExtendedGUI.GenericMenuAddItem(eventOptionsMenu, new GUIContent("Delete"), true, () => { RemoveEvent(baseEvent); });

            eventOptionsMenu.ShowAsContext();
        }

        public EASEventDrawer GetEventDrawer(System.Type type)
        {
            if (m_EventDrawers.ContainsKey(type))
            {
                return m_EventDrawers[type];
            }

            return null;
        }
    }
}