using System;
using UnityEngine;
using UnityEditor;
using ExtendedGUI;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    public class EASHierarchy
    {
        [SerializeField]
        protected string m_SelectedAnimationName = null;
        public string SelectedAnimationName { get => m_SelectedAnimationName; }

        [SerializeField]
        protected int m_SelectedAnimationIndex = 0;

        protected List<EASBaseGUIItem> m_HierarchyGUIItems = new List<EASBaseGUIItem>();

        protected IEASSerializable m_RightClickedEASSerializable = null;

        public void OnUpdate()
        {
            if (m_SelectedAnimationName == null)
            {
                string[] animationNames = EASEditor.Instance.GetAnimationNames();

                if (animationNames.Length > 0)
                {
                    m_SelectedAnimationName = animationNames[0];
                    m_SelectedAnimationIndex = 0;

                    EASEditor.Instance.OnAnimationChanged();
                }
                else
                {
                    m_SelectedAnimationName = "No animations";
                }
            }
        }

        public void OnSelectionChanged()
        {
            m_SelectedAnimationName = null;
        }

        public void OnGUI(Rect rect)
        {
            Rect upperControlsRect = new Rect(rect.x, rect.y, rect.width, EASSkin.ControlToolbarHeight);
            Rect arrowsRect = Rect.MinMaxRect(upperControlsRect.xMax - 2.0f * upperControlsRect.height, upperControlsRect.y, upperControlsRect.xMax, upperControlsRect.yMax);

            bool hasAnimations = EASEditor.Instance.Controller.HasAnimations();

            Rect upArrowRect = new Rect(arrowsRect.x, arrowsRect.y, arrowsRect.width / 2.0f, arrowsRect.height);
            if (ExtendedGUI.ExtendedGUI.IconButton(upArrowRect, GUIContent.none, EditorStyles.toolbarButton, EASSkin.Icon("UpArrow"), iconPadding: 3, 
                iconIsOnTop: true, isSelected: false, enabled: hasAnimations))
            {
                string[] animationNames = EASEditor.Instance.Controller.GetAnimationNames();

                m_SelectedAnimationIndex = (++m_SelectedAnimationIndex) % animationNames.Length;
                m_SelectedAnimationName = animationNames[m_SelectedAnimationIndex];

                EASEditor.Instance.OnAnimationChanged();
            }

            Rect downArrowRect = new Rect(upArrowRect.xMax - 1, arrowsRect.y, upArrowRect.width + 1, arrowsRect.height);
            ExtendedGUI.ExtendedGUI.PreviewRect(downArrowRect);

            GUIUtility.RotateAroundPivot(180.0f, downArrowRect.center);
            if (ExtendedGUI.ExtendedGUI.IconButton(downArrowRect, GUIContent.none, EditorStyles.toolbarButton, EASSkin.Icon("UpArrow"), iconPadding: 3, 
                iconIsOnTop: true, isSelected: false, enabled: hasAnimations))
            {
                string[] animationNames = EASEditor.Instance.Controller.GetAnimationNames();

                m_SelectedAnimationIndex = (--m_SelectedAnimationIndex + animationNames.Length) % animationNames.Length;
                m_SelectedAnimationName = animationNames[m_SelectedAnimationIndex];

                EASEditor.Instance.OnAnimationChanged();
            }
            GUIUtility.RotateAroundPivot(-180.0f, downArrowRect.center);

            Rect animationsRect = Rect.MinMaxRect(upperControlsRect.x, upperControlsRect.y, arrowsRect.x, upperControlsRect.yMax);
            if (GUI.Button(animationsRect, m_SelectedAnimationName, EditorStyles.toolbarDropDown))
            {
                string[] animationNames = EASEditor.Instance.Controller.GetAnimationNames();
                Action<int> onSelect = i =>
                {
                    m_SelectedAnimationName = animationNames[i];
                    m_SelectedAnimationIndex = i;

                    EASEditor.Instance.OnAnimationChanged();
                };

                SearchablePopup.Show(animationsRect, animationNames, m_SelectedAnimationIndex, onSelect);
            }
            Rect verticalSeparatorRect = new Rect(rect.x, animationsRect.yMax, rect.width, 1);
            EditorGUI.DrawRect(verticalSeparatorRect, EASSkin.SeparatorColor);

            Rect hierarchyRect = Rect.MinMaxRect(rect.x, verticalSeparatorRect.yMax, rect.xMax, rect.yMax - animationsRect.height - 1);
            OnGUIHierarchy(hierarchyRect);

            verticalSeparatorRect.y = rect.yMax - animationsRect.height - 1;
            EditorGUI.DrawRect(verticalSeparatorRect, EASSkin.SeparatorColor);

            Rect bottomControlsRect = new Rect(rect.x, verticalSeparatorRect.yMax, rect.width, animationsRect.height);
            OnGUIBottomControls(bottomControlsRect);
            
            Rect horizontalSeparatorRect = new Rect(rect.xMax - 1, rect.y, 1, rect.height);
            EditorGUI.DrawRect(horizontalSeparatorRect, EASSkin.SeparatorColor);
        }

        protected void OnGUIHierarchy(Rect hierarchyRect)
        {
            EditorGUI.DrawRect(hierarchyRect, EASSkin.BackgroundColor);

            m_HierarchyGUIItems.Clear();
            Rect hierarchyTrackRect = Rect.MinMaxRect(hierarchyRect.x + EASSkin.HierarchyLeftMargin, hierarchyRect.y + EASSkin.HierarchyUpperMargin, hierarchyRect.xMax, hierarchyRect.y + EASSkin.HierarchyTrackHeight);

            List<IEASSerializable> tracksAndGroups = EASEditor.Instance.GetTracksAndGroups();
            for (int i = 0; i < tracksAndGroups.Count; ++i)
            {
                OnGUITrackAndGroup(ref hierarchyTrackRect, tracksAndGroups[i]);
            }

            HandleInput(hierarchyRect);
        }

        protected void OnGUITrackAndGroup(ref Rect hierarchyTrackRect, IEASSerializable trackOrGroup)
        {
            if (trackOrGroup is EASTrackGroup)
            {
                OnGUITrackGroup(ref hierarchyTrackRect, trackOrGroup as EASTrackGroup);
            }
            else if (trackOrGroup is EASTrack)
            {
                OnGUITrack(ref hierarchyTrackRect, trackOrGroup as EASTrack);
            }
        }

        protected void OnGUITrackGroup(ref Rect hierarchyTrackRect, EASTrackGroup trackGroup)
        {
            Rect trackGroupRect = hierarchyTrackRect;
            if (trackGroup.Tracks.Count > 0)
            {
                Rect dropDownRect = Rect.MinMaxRect(hierarchyTrackRect.x - EASSkin.HierarchyLeftMargin, hierarchyTrackRect.y, hierarchyTrackRect.x, hierarchyTrackRect.yMax);
                if (GUI.Button(dropDownRect, GUIContent.none, GUIStyle.none))
                {
                    trackGroup.Collapsed = !trackGroup.Collapsed;
                }

                Rect dropdownIconRect = ExtendedGUI.ExtendedGUI.GetInnerRect(dropDownRect, EASSkin.HierarchyTrackGroupDropdownSize, EASSkin.HierarchyTrackGroupDropdownSize);
                if (trackGroup.Collapsed)
                {
                    GUIUtility.RotateAroundPivot(-90, dropdownIconRect.center);
                    GUI.DrawTexture(dropdownIconRect, EASSkin.Icon("icon dropdown"));
                    GUIUtility.RotateAroundPivot(90, dropdownIconRect.center);

                    EditorGUI.DrawRect(trackGroupRect, EASEditor.Instance.IsSelected(trackGroup) ? EASSkin.HierarchySelectedColor : EASSkin.HierarchyTrackGroupColor);
                }
                else
                {
                    GUI.DrawTexture(dropdownIconRect, EASSkin.Icon("icon dropdown"));

                    trackGroupRect = new Rect(hierarchyTrackRect.x, hierarchyTrackRect.y, hierarchyTrackRect.width, hierarchyTrackRect.height + (EASSkin.HierarchyTrackSpacing + hierarchyTrackRect.height) * trackGroup.Tracks.Count +
                        EASSkin.HierarchyTrackSpacingSingle);
                    EditorGUI.DrawRect(trackGroupRect, EASEditor.Instance.IsSelected(trackGroup) ? EASSkin.HierarchySelectedColor : EASSkin.HierarchyTrackGroupColor);

                    Rect tracksRects = new Rect(hierarchyTrackRect.x + EASSkin.HierarchyLeftMargin, trackGroupRect.y + EASSkin.HierarchyTrackSpacing + hierarchyTrackRect.height, hierarchyTrackRect.width - EASSkin.HierarchyLeftMargin,
                        hierarchyTrackRect.height);
                    for (int i = 0; i < trackGroup.Tracks.Count; ++i)
                    {
                        OnGUITrack(ref tracksRects, trackGroup.Tracks[i]);
                    }
                }
            }
            else
            {
                EditorGUI.DrawRect(trackGroupRect, EASEditor.Instance.IsSelected(trackGroup) ? EASSkin.HierarchySelectedColor : EASSkin.HierarchyTrackGroupColor);
            }

            if (OnGUITrackSpecialControls(hierarchyTrackRect, trackGroup, optionsButtonIcon: "Toolbar Plus", false, false))
            {
                Event.current.Use();

                EASEditor.Instance.ShowTrackGroupOptionsMenu(trackGroup);
            }

            m_HierarchyGUIItems.Add(new EASBaseGUIItem(trackGroupRect, trackGroup));
            hierarchyTrackRect.y = trackGroupRect.yMax + EASSkin.HierarchyTrackSpacing;
        }

        protected void OnGUITrack(ref Rect hierarchyTrackRect, EASTrack track)
        {
            EditorGUI.DrawRect(hierarchyTrackRect, EASEditor.Instance.IsSelected(track) ? EASSkin.HierarchySelectedColor : EASSkin.HierarchyTrackColor);

            Rect trackIconRect = ExtendedGUI.ExtendedGUI.GetInnerRect(new Rect(hierarchyTrackRect.x, hierarchyTrackRect.y, hierarchyTrackRect.height, hierarchyTrackRect.height),
                hierarchyTrackRect.height * EASSkin.HierarchyTrackIconMultiplier, hierarchyTrackRect.height * EASSkin.HierarchyTrackIconMultiplier);
            GUI.DrawTexture(trackIconRect, EASSkin.Icon("d_UnityEditor.Timeline.TimelineWindow"));

            if (OnGUITrackSpecialControls(hierarchyTrackRect, track, optionsButtonIcon: "_Menu", track.ParentTrackGroupLocked, track.ParentTrackGroupMuted))
            {
                Event.current.Use();

                EASEditor.Instance.ShowTrackOptionsMenu(track);
            }

            m_HierarchyGUIItems.Add(new EASBaseGUIItem(hierarchyTrackRect, track));
            hierarchyTrackRect.y = hierarchyTrackRect.yMax + EASSkin.HierarchyTrackSpacing;
        }

        protected bool OnGUITrackSpecialControls(Rect hierarchyTrackRect, EASBaseTrack baseTrack, string optionsButtonIcon, bool externalLock, bool externalMute)
        {
            Rect optionsButtonRect = Rect.MinMaxRect(hierarchyTrackRect.xMax - hierarchyTrackRect.height, hierarchyTrackRect.y, hierarchyTrackRect.xMax, hierarchyTrackRect.yMax);
            bool optionsButtonClicked = ExtendedGUI.ExtendedGUI.IconButton(optionsButtonRect, GUIContent.none, GUIStyle.none, EASSkin.Icon(optionsButtonIcon), EASSkin.HierarchyTrackOptionsIconMargin, true, isSelected: false);

            Rect controlsRect = Rect.MinMaxRect(optionsButtonRect.x - EASSkin.HierarchyTrackControlsWidthMultiplier * optionsButtonRect.width, 
                hierarchyTrackRect.y + EASSkin.HierarchyTrackControlsVerticalPadding, optionsButtonRect.x, hierarchyTrackRect.yMax - EASSkin.HierarchyTrackControlsVerticalPadding);
            EditorGUI.DrawRect(controlsRect, EASSkin.HierarchyTrackControlsColor);

            Rect controlButtonRect = new Rect(controlsRect.x, controlsRect.y, controlsRect.width / 2.0f, controlsRect.height);
            GUI.enabled = !externalLock;
            if (ExtendedGUI.ExtendedGUI.IconButton(controlButtonRect, GUIContent.none, GUIStyle.none, EASSkin.Icon(baseTrack.Locked || externalLock ? "IN LockButton on" : "IN LockButton"), 1.0f, iconIsOnTop: true, isSelected: false))
            {
                baseTrack.Locked = !baseTrack.Locked;
                Event.current.Use();
                GUI.FocusControl(null);
            }

            GUI.enabled = !externalMute;
            controlButtonRect.x = controlButtonRect.xMax;
            if (ExtendedGUI.ExtendedGUI.IconButton(controlButtonRect, GUIContent.none, GUIStyle.none, EASSkin.Icon(baseTrack.Muted || externalMute ? "animationvisibilitytoggleoff" : "animationvisibilitytoggleon"), 2.0f, iconIsOnTop: true, isSelected: false))
            {
                baseTrack.Muted = !baseTrack.Muted;
                Event.current.Use();
                GUI.FocusControl(null);
            }

            GUI.enabled = true;

            Rect trackGroupLabelRect = Rect.MinMaxRect(hierarchyTrackRect.x + EASSkin.HierarchyTrackLabelLeftMargin, hierarchyTrackRect.y, controlsRect.x, hierarchyTrackRect.yMax);
            string baseTrackName = baseTrack.Name;

            trackGroupLabelRect.width = Mathf.Min(EditorStyles.label.CalcSize(new GUIContent(baseTrackName)).x, trackGroupLabelRect.width);

            EditorGUI.BeginChangeCheck();
            baseTrackName = EditorGUI.DelayedTextField(trackGroupLabelRect, baseTrack.Name, EditorStyles.label);
            if (EditorGUI.EndChangeCheck())
            {
                baseTrack.Name = string.IsNullOrEmpty(baseTrackName) || string.IsNullOrWhiteSpace(baseTrackName) ? baseTrack.DefaultName : baseTrackName;

                GUI.FocusControl(null);

                if (EASInspector.HasInstance)
                {
                    EASInspector.Instance.Repaint();
                }
            }

            return optionsButtonClicked;
        }

        protected void HandleInput(Rect hierarchyRect)
        {
            if (m_RightClickedEASSerializable != null)
            {
                if (m_RightClickedEASSerializable is EASTrackGroup)
                {
                    EASEditor.Instance.ShowTrackGroupOptionsMenu(m_RightClickedEASSerializable as EASTrackGroup);
                }
                else if (m_RightClickedEASSerializable is EASTrack)
                {
                    EASEditor.Instance.ShowTrackOptionsMenu(m_RightClickedEASSerializable as EASTrack);
                }

                m_RightClickedEASSerializable = null;
                return;
            }

            if (Event.current.type == EventType.MouseUp)
            {
                if (hierarchyRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                {
                    OnHierarchyLeftClick();
                }
            }
            else if (Event.current.type == EventType.MouseDown)
            {
                if (hierarchyRect.Contains(Event.current.mousePosition) && Event.current.button == 1)
                {
                    OnHierarchyRightClick();
                }
            }
        }

        protected void OnHierarchyLeftClick()
        {
            IEASSerializable hierarchyTrack = HierarchyTrackAtMousePosition();
            if (hierarchyTrack != null)
            {
                EASEditor.Instance.SelectObject(hierarchyTrack, Event.current.modifiers != EventModifiers.Shift);
            }
            else
            {
                EASEditor.Instance.SelectObject(null, true);
            }
        }

        protected void OnHierarchyRightClick()
        {
            IEASSerializable rightClickedHierarchyTrack = HierarchyTrackAtMousePosition();
            if (rightClickedHierarchyTrack != null)
            {
                EASEditor.Instance.SelectObject(rightClickedHierarchyTrack, singleSelection: true);

                m_RightClickedEASSerializable = rightClickedHierarchyTrack;
            }
            else
            {
                EASEditor.Instance.ShowOptionsMenu();
            }
        }

        protected IEASSerializable HierarchyTrackAtMousePosition()
        {
            for (int i = 0; i < m_HierarchyGUIItems.Count; ++i)
            {
                if (m_HierarchyGUIItems[i].Rect.Contains(Event.current.mousePosition))
                {
                    return m_HierarchyGUIItems[i].EASSerializable;
                }
            }

            return null;
        }

        protected void OnGUIBottomControls(Rect bottomControlsRect)
        {
            EditorGUI.DrawRect(bottomControlsRect, EASSkin.ToolbarBackgroundColor);

            Rect buttonRect = new Rect(0, bottomControlsRect.y, bottomControlsRect.height * 1.4f, bottomControlsRect.height);
            buttonRect.x = bottomControlsRect.xMax - buttonRect.width;

            if (ControlRect(ref buttonRect, EASSkin.Icon("d_Folder Icon")))
            {
                EASEditor.Instance.AddTrackGroup();
            }

            if (ControlRect(ref buttonRect, EASSkin.Icon("d_UnityEditor.Timeline.TimelineWindow")))
            {
                EASEditor.Instance.AddTrack();
            }
        }

        protected bool ControlRect(ref Rect rect, Texture2D icon = null, float iconPadding = 2)
        {
            bool buttonPressed = ExtendedGUI.ExtendedGUI.IconButton(rect, GUIContent.none, EditorStyles.toolbarButton, icon, iconPadding, iconIsOnTop: true, isSelected: false);
            rect.x -= rect.width;

            return buttonPressed;
        }
    }
}
