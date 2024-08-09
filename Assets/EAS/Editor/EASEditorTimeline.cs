using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    public class EASEditorTimeline
    {
        protected List<EASBaseGUIItem> m_TimelineTracksAndGroups = new List<EASBaseGUIItem>();

        [SerializeField]
        protected EASAnimationInformation m_AnimationInformation;

        [SerializeField]
        protected Vector2 m_ScrollPositionOffsets = Vector2.zero;

        [SerializeField]
        protected float m_PixelsPerFrame = 13;

        protected List<EASTimelineFrame> m_Frames = new List<EASTimelineFrame>();

        protected Rect m_WholeTimelineRect, m_FramesAreaRect;

        public void OnGUI(Rect rect)
        {
            m_WholeTimelineRect = rect;
            EditorGUI.DrawRect(rect, EASSkin.BackgroundColor);

            GUI.BeginClip(rect);

            m_FramesAreaRect = new Rect(0, 0, rect.width, EASSkin.ControlToolbarHeight);
            OnGUIFramesArea();

            Rect timelineWorkAreaRect = Rect.MinMaxRect(0, m_FramesAreaRect.yMax + 1, rect.width, rect.yMax - rect.y);
            OnGUITrackBackgrounds(timelineWorkAreaRect);
            OnGUIFrameLines(timelineWorkAreaRect);

            HandleRestrictedInput();

            GUI.EndClip();
        }

        protected void OnGUIFramesArea()
        {
            m_Frames.Clear();

            EditorGUI.DrawRect(m_FramesAreaRect, EASSkin.TimelineFramesBackgroundColor);

            Rect lineRect = new Rect(0, m_FramesAreaRect.y, 1f, m_FramesAreaRect.height - 1);
            for (int i = 0; i < m_AnimationInformation.Frames; ++i)
            {
                EASTimelineFrameType frameType = GetFrameType(i, m_AnimationInformation.Frames - 1);
                EASTimelineFrame timelineFrame = new EASTimelineFrame(frame: i, GetHorizontalPositionAtFrame(i), frameType, GetFrameColor(frameType));
                m_Frames.Add(timelineFrame);

                DrawFrame(lineRect, timelineFrame, isFrameLine: false);
            }
        }

        protected void OnGUITrackBackgrounds(Rect timelineWorkAreaRect)
        {
            m_TimelineTracksAndGroups.Clear();

            Rect timelineTrackRect = Rect.MinMaxRect(timelineWorkAreaRect.x, timelineWorkAreaRect.y + EASSkin.HierarchyUpperMargin, timelineWorkAreaRect.xMax, timelineWorkAreaRect.y + EASSkin.HierarchyTrackHeight);

            List<EASSerializable> tracksAndGroups = EASEditor.Instance.GetTracksAndGroups();
            for (int i = 0; i < tracksAndGroups.Count; ++i)
            {
                if (tracksAndGroups[i] is EASTrackGroup)
                {
                    EASTrackGroup trackGroup = tracksAndGroups[i] as EASTrackGroup;
                    if (trackGroup.Tracks.Count > 0 && !trackGroup.Collapsed)
                    {
                        Rect wholeTrackGroupRect = new Rect(timelineTrackRect.x, timelineTrackRect.y, timelineTrackRect.width, timelineTrackRect.height + (EASSkin.HierarchyTrackSpacing + timelineTrackRect.height) *
                            trackGroup.Tracks.Count + EASSkin.HierarchyTrackSpacingSingle);
                        EditorGUI.DrawRect(wholeTrackGroupRect, EASEditor.Instance.IsSelected(trackGroup) ? EASSkin.TimelineSelectedColor : EASSkin.TimelineTrackGroupColor);

                        Rect tracksRect = new Rect(wholeTrackGroupRect.x, timelineTrackRect.y + EASSkin.HierarchyTrackSpacing + timelineTrackRect.height, wholeTrackGroupRect.width, timelineTrackRect.height);
                        for (int j = 0; j < trackGroup.Tracks.Count; ++j)
                        {
                            EditorGUI.DrawRect(tracksRect, EASEditor.Instance.IsSelected(trackGroup.Tracks[j]) ? EASSkin.TimelineSelectedColor : EASSkin.TimelineTrackColor);

                            m_TimelineTracksAndGroups.Add(new EASBaseGUIItem(tracksRect, trackGroup.Tracks[j]));
                            tracksRect.y = tracksRect.yMax + EASSkin.HierarchyTrackSpacing;
                        }

                        m_TimelineTracksAndGroups.Add(new EASBaseGUIItem(wholeTrackGroupRect, trackGroup));
                        timelineTrackRect.y = wholeTrackGroupRect.yMax + EASSkin.HierarchyTrackSpacing;
                    }
                    else
                    {
                        EditorGUI.DrawRect(timelineTrackRect, EASEditor.Instance.IsSelected(trackGroup) ? EASSkin.TimelineSelectedColor : EASSkin.TimelineTrackGroupColor);

                        m_TimelineTracksAndGroups.Add(new EASBaseGUIItem(timelineTrackRect, trackGroup));
                        timelineTrackRect.y = timelineTrackRect.yMax + EASSkin.HierarchyTrackSpacing;
                    }
                }
                else if (tracksAndGroups[i] is EASTrack)
                {
                    EditorGUI.DrawRect(timelineTrackRect, EASEditor.Instance.IsSelected(tracksAndGroups[i]) ? EASSkin.TimelineSelectedColor : EASSkin.TimelineTrackColor);

                    m_TimelineTracksAndGroups.Add(new EASBaseGUIItem(timelineTrackRect, tracksAndGroups[i]));
                    timelineTrackRect.y = timelineTrackRect.yMax + EASSkin.HierarchyTrackSpacing;
                }
            }
        }

        protected void OnGUIFrameLines(Rect timelineWorkAreaRect)
        {
            Rect lineRect = new Rect(0, timelineWorkAreaRect.y, 1f, timelineWorkAreaRect.height);
            for (int i = 0; i < m_Frames.Count; ++i)
            {
                DrawFrame(lineRect, m_Frames[i], isFrameLine: true);
            }
        }

        protected void HandleRestrictedInput()
        {
            if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button == 0)
                {
                    OnLeftClickUpRestricted();
                }
                else if (Event.current.button == 1)
                {
                    OnRightClickUpRestricted();
                }
            }
        }

        protected void OnLeftClickUpRestricted()
        {
            EASSerializable timelineTrack = TimelineTrackAtRestrictedMousePosition();
            if (timelineTrack != null)
            {
                EASEditor.Instance.SelectObject(timelineTrack, Event.current.modifiers != EventModifiers.Shift);
            }
            else
            {
                EASEditor.Instance.SelectObject(null, true);
            }
        }

        protected void OnRightClickUpRestricted()
        {
            EASSerializable rightClickedTimelineTrack = TimelineTrackAtRestrictedMousePosition() as EASSerializable;
            if (rightClickedTimelineTrack != null)
            {
                if (rightClickedTimelineTrack is EASTrackGroup)
                {
                    EASEditor.Instance.ShowTrackGroupOptionsMenu(rightClickedTimelineTrack as EASTrackGroup);
                }
                else if (rightClickedTimelineTrack is EASTrack)
                {
                    EASEditor.Instance.ShowTrackOptionsMenu(rightClickedTimelineTrack as EASTrack);
                }
            }
            else
            {
                EASEditor.Instance.ShowOptionsMenu();
            }
        }

        protected EASSerializable TimelineTrackAtRestrictedMousePosition()
        {
            for (int i = 0; i < m_TimelineTracksAndGroups.Count; ++i)
            {
                if (m_TimelineTracksAndGroups[i].Rect.Contains(Event.current.mousePosition))
                {
                    return m_TimelineTracksAndGroups[i].EASSerializable;
                }
            }

            return null;
        }

        public void OnAnimationChanged()
        {
            m_AnimationInformation = EASEditor.Instance.GetAnimationInformation();

            float framesAreaWidth = m_FramesAreaRect.width - EASSkin.TimelineRightMargin;
            m_PixelsPerFrame = framesAreaWidth / (m_AnimationInformation.Frames - 1);
        }

        protected float GetInitialFramePosition()
        {
            return EASSkin.TimelineLeftMargin - m_ScrollPositionOffsets.x;
        }

        protected float GetHorizontalPositionAtFrame(float frame)
        {
            return GetInitialFramePosition() + frame * m_PixelsPerFrame;
        }

        protected EASTimelineFrameType GetFrameType(float frame, float lastFrame)
        {
            if (frame == lastFrame)
            {
                return EASTimelineFrameType.MainFrame;
            }

            int framesBetweenMainFrames = Mathf.Clamp(Mathf.RoundToInt(30.0f / (m_PixelsPerFrame / 2.0f)), 1, int.MaxValue);

            if (frame % framesBetweenMainFrames == 0)
            {
                return EASTimelineFrameType.MainFrame;
            }

            if (frame % (framesBetweenMainFrames / 2.0f) == 0 || frame % (framesBetweenMainFrames / 4.0f) == 0)
            {
                return EASTimelineFrameType.SecondaryFrame;
            }

            if (framesBetweenMainFrames >= 10)
            {
                return EASTimelineFrameType.InvisibleFrame;
            }

            if (framesBetweenMainFrames >= 5)
            {
                return EASTimelineFrameType.TertiaryFrame;
            }

            return EASTimelineFrameType.SecondaryFrame;
        }

        protected Color GetFrameColor(EASTimelineFrameType frameType)
        {
            if (frameType == EASTimelineFrameType.MainFrame) return EASSkin.TimelineFrameLineColor;
            if (frameType == EASTimelineFrameType.SecondaryFrame) return EASSkin.TimelineSecondaryFrameLineColor;

            return EASSkin.TimelineTertiaryFrameLineColor;
        }

        protected void DrawFrame(Rect rect, EASTimelineFrame frame, bool isFrameLine)
        {
            if (frame.FrameType != EASTimelineFrameType.InvisibleFrame)
            {
                rect.x = frame.HorizontalPosition;

                if (isFrameLine)
                {
                    EditorGUI.DrawRect(rect, frame.FrameColor);
                }
                else
                {
                    if (frame.FrameType == EASTimelineFrameType.MainFrame)
                    {
                        GUIContent frameGUIContent = new GUIContent(frame.Frame.ToString());
                        Vector2 size = EditorStyles.whiteMiniLabel.CalcSize(frameGUIContent);

                        Rect frameLabelRect = new Rect(rect.x + EASSkin.TimelineMainFrameLabelLeftMargin, rect.y + EASSkin.TimelineMainFrameLabelUpperMargin, size.x, size.y);
                        GUI.Label(frameLabelRect, frameGUIContent, EditorStyles.whiteMiniLabel);

                        rect = Rect.MinMaxRect(rect.x, rect.yMax - EASSkin.TimelineMainFrameHeight, rect.xMax, rect.yMax);
                    }
                    else
                    {
                        rect = Rect.MinMaxRect(rect.x, rect.yMax - (frame.FrameType == EASTimelineFrameType.SecondaryFrame ? EASSkin.TimelineSecondaryFrameHeight : EASSkin.TimelineTertiaryFrameHeight), rect.xMax, rect.yMax);
                    }

                    EditorGUI.DrawRect(rect, EASSkin.TimelineFrameColor);
                }
            }
        }
    }
}


