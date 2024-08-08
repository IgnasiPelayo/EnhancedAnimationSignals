using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using ExtendedGUI;

namespace EAS
{
    [System.Serializable]
    public class EASEditorHierarchy
    {
        [SerializeField]
        protected string m_SelectedAnimationName = null;
        public string SelectedAnimationName { get => m_SelectedAnimationName; }

        [SerializeField]
        protected int m_SelectedAnimationIndex = 0;

        protected List<Rect> m_HierarchyRects = new List<Rect>();

        public void OnUpdate()
        {
            if (m_SelectedAnimationName == null)
            {
                string[] animationNames = EASEditor.Instance.Controller.GetAnimationNames();

                m_SelectedAnimationName = animationNames.Length > 0 ? animationNames[0] : "No animations";
                m_SelectedAnimationIndex = 0;
            }
        }

        public void OnGUI(Rect rect)
        {
            Rect animationsRect = new Rect(rect.x, rect.y, rect.width, EASSkin.ControlToolbarHeight);
            if (GUI.Button(animationsRect, m_SelectedAnimationName, EditorStyles.toolbarDropDown))
            {
                string[] animationNames = EASEditor.Instance.Controller.GetAnimationNames();
                Action<int> onSelect = i =>
                {
                    m_SelectedAnimationName = animationNames[i];
                    m_SelectedAnimationIndex = i;
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

            m_HierarchyRects.Clear();
            Rect hierarchyTrackRect = Rect.MinMaxRect(hierarchyRect.x + EASSkin.HierarchyLeftMargin, hierarchyRect.y + EASSkin.HierarchyUpperMargin, hierarchyRect.xMax, hierarchyRect.y + EASSkin.HierarchyTrackHeight);

            List<EASSerializable> tracksAndGroups = EASEditor.Instance.Controller.Data.GetTracksAndGroups(m_SelectedAnimationName);
            for (int i = 0; i < tracksAndGroups.Count; ++i)
            {
                OnGUITrackAndGroup(ref hierarchyTrackRect, tracksAndGroups[i]);
            }

            HandleInput(hierarchyRect);
        }

        protected void OnGUITrackAndGroup(ref Rect hierarchyTrackRect, EASSerializable trackOrGroup)
        {
            if (trackOrGroup is EASTrackGroup)
            {
                OnGUITrackGroup(ref hierarchyTrackRect, trackOrGroup as EASTrackGroup);
            }
            else if (trackOrGroup is EASTrack)
            {
                OnGUITrack(ref hierarchyTrackRect, trackOrGroup as EASTrack, false, false);
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

                Rect dropdownIconRect = ExtendedGUI.ExtendedGUI.GetInnerRect(dropDownRect, 1.0f);
                if (trackGroup.Collapsed)
                {
                    GUIUtility.RotateAroundPivot(-90, dropdownIconRect.center);
                    GUI.DrawTexture(dropdownIconRect, EASSkin.Icon("icon dropdown"));
                    GUIUtility.RotateAroundPivot(90, dropdownIconRect.center);

                    EditorGUI.DrawRect(trackGroupRect, EASSkin.HierarchyTrackGroupColor);
                }
                else
                {
                    GUI.DrawTexture(dropdownIconRect, EASSkin.Icon("icon dropdown"));

                    trackGroupRect = new Rect(hierarchyTrackRect.x, hierarchyTrackRect.y, hierarchyTrackRect.width, hierarchyTrackRect.height + (EASSkin.HierarchyTrackSpacing + hierarchyTrackRect.height) * trackGroup.Tracks.Count +
                        EASSkin.HierarchyTrackSpacingSingle);
                    EditorGUI.DrawRect(trackGroupRect, EASSkin.HierarchyTrackGroupColor);

                    Rect tracksRects = new Rect(hierarchyTrackRect.x + EASSkin.HierarchyLeftMargin, trackGroupRect.y + EASSkin.HierarchyTrackSpacing + hierarchyTrackRect.height, hierarchyTrackRect.width - EASSkin.HierarchyLeftMargin,
                        hierarchyTrackRect.height);
                    for (int i = 0; i < trackGroup.Tracks.Count; ++i)
                    {
                        OnGUITrack(ref tracksRects, trackGroup.Tracks[i], trackGroup.Locked, trackGroup.Muted);
                    }
                }
            }
            else
            {
                EditorGUI.DrawRect(trackGroupRect, EASSkin.HierarchyTrackGroupColor);
            }

            if (OnGUITrackSpecialControls(hierarchyTrackRect, trackGroup, optionsButtonIcon: "Toolbar Plus", false, false))
            {
                GenericMenu trackOptionsMenu = new GenericMenu();
                ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Add Track"), !trackGroup.Locked, () => { trackGroup.AddTrack(); });
                ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Delete"), !trackGroup.Locked, () => { EASEditor.Instance.RemoveTrackOrGroup(trackGroup); });
                trackOptionsMenu.AddSeparator("");
                trackOptionsMenu.AddItem(new GUIContent($"{(trackGroup.Locked ? "Unl" : "L")}ock _L"), false, () => { trackGroup.Locked = !trackGroup.Locked; });
                trackOptionsMenu.AddItem(new GUIContent($"{(trackGroup.Muted ? "Unm" : "M")}ute _M"), false, () => { trackGroup.Muted = !trackGroup.Muted; });

                trackOptionsMenu.ShowAsContext();
            }

            m_HierarchyRects.Add(trackGroupRect);
            hierarchyTrackRect.y = trackGroupRect.yMax + EASSkin.HierarchyTrackSpacing;
        }

        protected void OnGUITrack(ref Rect hierarchyTrackRect, EASTrack track, bool externalLock, bool externalMute)
        {
            EditorGUI.DrawRect(hierarchyTrackRect, EASSkin.HierarchyTrackColor);

            Rect trackIconRect = ExtendedGUI.ExtendedGUI.GetInnerRect(new Rect(hierarchyTrackRect.x, hierarchyTrackRect.y, hierarchyTrackRect.height, hierarchyTrackRect.height),
                hierarchyTrackRect.height * EASSkin.HierarchyTrackIconMultiplier, hierarchyTrackRect.height * EASSkin.HierarchyTrackIconMultiplier);
            GUI.DrawTexture(trackIconRect, EASSkin.Icon("d_UnityEditor.Timeline.TimelineWindow"));

            if (OnGUITrackSpecialControls(hierarchyTrackRect, track, optionsButtonIcon: "_Menu", externalLock, externalMute))
            {
                GenericMenu trackOptionsMenu = new GenericMenu();
                ExtendedGUI.ExtendedGUI.GenericMenuAddItem(trackOptionsMenu, new GUIContent("Delete"), !track.Locked && !externalLock, () => { EASEditor.Instance.RemoveTrackOrGroup(track); });
                trackOptionsMenu.AddSeparator("");
                trackOptionsMenu.AddItem(new GUIContent($"{(track.Locked ? "Unl" : "L")}ock _L"), false, () => { track.Locked = !track.Locked; });
                trackOptionsMenu.AddItem(new GUIContent($"{(track.Muted ? "Unm" : "M")}ute _M"), false, () => { track.Muted = !track.Muted; });

                trackOptionsMenu.ShowAsContext();
            }

            m_HierarchyRects.Add(hierarchyTrackRect);
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
            }

            GUI.enabled = !externalMute;
            controlButtonRect.x = controlButtonRect.xMax;
            if (ExtendedGUI.ExtendedGUI.IconButton(controlButtonRect, GUIContent.none, GUIStyle.none, EASSkin.Icon(baseTrack.Muted || externalMute ? "animationvisibilitytoggleoff" : "animationvisibilitytoggleon"), 2.0f, iconIsOnTop: true, isSelected: false))
            {
                baseTrack.Muted = !baseTrack.Muted;
            }

            GUI.enabled = true;

            Rect trackGroupLabelRect = Rect.MinMaxRect(hierarchyTrackRect.x + EASSkin.HierarchyTrackLabelLeftMargin, hierarchyTrackRect.y, controlsRect.x, hierarchyTrackRect.yMax); 
            string baseTrackName = baseTrack.Name;
            EditorGUI.BeginChangeCheck();
            baseTrackName = EditorGUI.DelayedTextField(trackGroupLabelRect, baseTrack.Name, EditorStyles.label);
            if (EditorGUI.EndChangeCheck())
            {
                baseTrack.Name = baseTrackName;
                GUI.FocusControl(null);
            }

            return optionsButtonClicked;
        }

        protected void HandleInput(Rect hierarchyRect)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 1)
                {
                    if (hierarchyRect.Contains(Event.current.mousePosition))
                    {
                        OnHierarchyRightClick();
                    }
                }
            }
        }

        protected void OnHierarchyRightClick()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Track Group"), false, () => { EASEditor.Instance.AddTrackGroup(); });
            menu.AddItem(new GUIContent("Add Track"), false, () => { EASEditor.Instance.AddTrack(); });

            menu.ShowAsContext();
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
