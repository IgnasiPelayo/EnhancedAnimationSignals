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

        [SerializeField]
        protected EASEditorControls m_Controls = new EASEditorControls();

        [SerializeField]
        protected EASEditorHierarchy m_Hierarchy = new EASEditorHierarchy();

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
            Rect windowRect = position;
            windowRect.x = windowRect.y = 0;

            Rect toolbarRect = new Rect(0, 0, windowRect.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            m_Controls.OnGUI(toolbarRect);

            Rect hierarchyRect = new Rect(0, toolbarRect.yMax, windowRect.width / 4.0f, windowRect.height - toolbarRect.height);
            m_Hierarchy.OnGUI(hierarchyRect);
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

        public EASTrackGroup AddTrackGroup()
        {
            return Controller.Data.AddTrackGroup(m_Hierarchy.SelectedAnimationName);
        }

        public EASTrack AddTrack()
        {
            return Controller.Data.AddTrack(m_Hierarchy.SelectedAnimationName);
        }

        public bool RemoveTrackOrGroup(EASSerializable trackOrGroup)
        {
            return Controller.Data.RemoveTrackOrGroup(m_Hierarchy.SelectedAnimationName, trackOrGroup);
        }
    }
}
