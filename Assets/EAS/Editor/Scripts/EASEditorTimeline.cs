using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    public class EASEditorTimeline
    {
        protected List<EASBaseGUIItem> m_TimelineTracksAndGroups = new List<EASBaseGUIItem>();
        protected List<EASEventGUIItem> m_TimelineEvents = new List<EASEventGUIItem>();

        protected EASDragInformation m_DragInformation;

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

            if (m_AnimationInformation == null)
            {
                return;
            }

            EditorGUI.DrawRect(rect, EASSkin.BackgroundColor);

            GUI.BeginClip(rect);

            m_FramesAreaRect = new Rect(0, 0, rect.width, EASSkin.ControlToolbarHeight);
            OnGUIFramesArea();

            Rect timelineWorkAreaRect = Rect.MinMaxRect(0, m_FramesAreaRect.yMax + 1, rect.width, rect.yMax - rect.y);
            OnGUITrackBackgrounds(timelineWorkAreaRect);
            OnGUIFrameLines(timelineWorkAreaRect);
            OnGUIEvents();

            HandleClippedInput();

            GUI.EndClip();

            HandleInput();
        }

        protected void OnGUIFramesArea()
        {
            m_Frames.Clear();

            EditorGUI.DrawRect(m_FramesAreaRect, EASSkin.TimelineFramesBackgroundColor);

            Rect lineRect = new Rect(0, m_FramesAreaRect.y, 1f, m_FramesAreaRect.height - 1);
            for (int i = 0; i < m_AnimationInformation.Frames; ++i)
            {
                EASTimelineFrameType frameType = GetFrameType(i, m_AnimationInformation.Frames - 1);
                EASTimelineFrame timelineFrame = new EASTimelineFrame(frame: i, GetHorizontalPositionAtFrame(i, clipped: true), frameType, GetFrameColor(frameType));
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

        protected void OnGUIEvents()
        {
            m_TimelineEvents.Clear();

            for (int i = 0; i < m_TimelineTracksAndGroups.Count; ++i)
            {
                EASTrack track = m_TimelineTracksAndGroups[i].EASSerializable as EASTrack;
                if (track != null)
                {
                    List<EASSerializable> events = new List<EASSerializable>(track.Events);
                    events = events.OrderBy(e => EASEditor.Instance.IsSelected(e)).ToList();

                    for (int j = 0; j < events.Count; ++j)
                    {
                        OnGUIEvent(m_TimelineTracksAndGroups[i], events[j] as EASBaseEvent);
                    }
                }
            }

            for (int i = 0; i < m_TimelineEvents.Count; ++i)
            {
                OnGUISelectedEvent(m_TimelineEvents[i]);
                OnGUIEventLabel(m_TimelineEvents[i]);
            }
        }

        protected void OnGUIEvent(EASBaseGUIItem trackGUIItem, EASBaseEvent baseEvent)
        {
            Rect eventRect = Rect.MinMaxRect(GetHorizontalPositionAtFrame(baseEvent.StartFrame, clipped: true), trackGUIItem.Rect.y, Mathf.Ceil(GetHorizontalPositionAtFrame(baseEvent.LastFrame, clipped: true)), trackGUIItem.Rect.yMax);

            if (EASEditor.Instance.IsSelected(baseEvent) && m_DragInformation != null && !m_DragInformation.CanEndDrag)
            {
                EditorGUI.DrawRect(eventRect, EASSkin.DraggedItemErrorColor);
            }
            else
            {
                EASEventDrawer eventDrawer = EASEditor.Instance.GetEventDrawer(baseEvent.GetType());
                if (eventDrawer != null)
                {
                    eventDrawer.OnGUIBackground(eventRect, baseEvent);
                }
                else
                {
                    EditorGUI.DrawRect(eventRect, EASUtils.GetEASEventColorAttribute(baseEvent.GetType()));
                }
            }

            Rect eventSeparatorRect = new Rect(eventRect.x, eventRect.y, 1, eventRect.height);
            EditorGUI.DrawRect(eventSeparatorRect, EASSkin.TimelineEventSeparatorColor);

            m_TimelineEvents.Add(new EASEventGUIItem(eventRect, baseEvent));
        }

        protected void OnGUISelectedEvent(EASEventGUIItem eventGUIItem)
        {
            if (EASEditor.Instance.IsSelected(eventGUIItem.EASSerializable))
            {
                if (m_DragInformation != null && !m_DragInformation.CanEndDrag)
                {
                    ExtendedGUI.ExtendedGUI.DrawOutlineRect(eventGUIItem.Rect, EASSkin.TimelineEventSelectedBorderColor, 1);
                }
                else
                {
                    EASEventDrawer eventDrawer = EASEditor.Instance.GetEventDrawer(eventGUIItem.EASSerializable.GetType());
                    if (eventDrawer != null)
                    {
                        eventDrawer.OnGUISelected(eventGUIItem.Rect, eventGUIItem.EASSerializable as EASBaseEvent);
                    }
                    else
                    {
                        EditorGUI.DrawRect(eventGUIItem.Rect, EASSkin.TimelineEventSelectedColor);
                        ExtendedGUI.ExtendedGUI.DrawOutlineRect(eventGUIItem.Rect, EASSkin.TimelineEventSelectedBorderColor, 1);
                    }
                }
            }
        }

        protected void OnGUIEventLabel(EASEventGUIItem eventGUIItem)
        {
            GUIStyle labelGUIStyle = new GUIStyle(EditorStyles.label);
            labelGUIStyle.alignment = TextAnchor.MiddleLeft;

            EASBaseEvent baseEvent = eventGUIItem.EASSerializable as EASBaseEvent;

            GUIContent eventLabelContent = new GUIContent(baseEvent.GetLabel());
            Vector2 eventLabelSize = labelGUIStyle.CalcSize(eventLabelContent);
            Rect eventLabelRect = ExtendedGUI.ExtendedGUI.GetInnerRect(eventGUIItem.Rect, Mathf.Min(eventLabelSize.x,
                eventGUIItem.Rect.width - (EASSkin.TimelineEventLabelLeftMargin + EASSkin.TimelineEventLabelRightMargin)), eventGUIItem.Rect.height);

            EASEventDrawer eventDrawer = EASEditor.Instance.GetEventDrawer(baseEvent.GetType());
            Color textColor = eventDrawer != null ? eventDrawer.LabelColor : ExtendedGUI.ExtendedGUI.GetContrastingLabelColor(EASUtils.GetEASEventColorAttribute(baseEvent.GetType()));

            labelGUIStyle.normal.textColor = labelGUIStyle.hover.textColor = labelGUIStyle.active.textColor = textColor;
            GUI.Label(eventLabelRect, eventLabelContent, labelGUIStyle);
        }

        protected void HandleClippedInput()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    OnLeftClickDownClipped();
                }
                else if (Event.current.button == 1)
                {
                    OnRightClickDownClipped();
                }
            }
        }

        protected void HandleInput()
        {
            EventType eventType = m_DragInformation != null ? m_DragInformation.GetEventType() : Event.current.type;
            if (eventType == EventType.MouseDrag)
            {
                if (Event.current.button == 0)
                {
                    OnLeftClickDrag();
                }
            }
            else if (eventType == EventType.MouseUp)
            {
                if (Event.current.button == 0)
                {
                    OnLeftClickUp();
                }
            }
        }

        protected void OnLeftClickDownClipped()
        {
            EASBaseGUIItem leftClickedGUIItem = TimelineGUIItemAtMousePosition(clipped: true);
            if (leftClickedGUIItem != null)
            {
                if (EASEditor.Instance.HasMultipleSelectionModifier())
                {
                    EASEditor.Instance.SelectObject(leftClickedGUIItem.EASSerializable, singleSelection: false);
                }
                else if (!EASEditor.Instance.IsSelected(leftClickedGUIItem.EASSerializable))
                {
                    EASEditor.Instance.SelectObject(leftClickedGUIItem.EASSerializable, singleSelection: true);
                }

                if (leftClickedGUIItem.EASSerializable is EASBaseEvent)
                {
                    List<EASBaseGUIItem> selectedGUIItemEvents = new List<EASBaseGUIItem>();
                    List<EASSerializable> selectedEvents = EASEditor.Instance.GetSelected<EASBaseEvent>();

                    for (int i = 0; i < selectedEvents.Count; ++i)
                    {
                        EASBaseGUIItem baseGUIItem = GetGUIItemOfEASSerializable(selectedEvents[i]);
                        selectedGUIItemEvents.Add(new EASBaseGUIItem(TransformClippedRect(baseGUIItem.Rect), baseGUIItem.EASSerializable));
                    }
                    m_DragInformation = new EASDragInformation(Event.current.mousePosition + m_WholeTimelineRect.position, selectedGUIItemEvents);
                }
            }
            else
            {
                EASEditor.Instance.SelectObject(null, singleSelection: true);
            }
        }

        protected void OnRightClickDownClipped()
        {
            EASBaseGUIItem rightClickedGUIItem = TimelineGUIItemAtMousePosition(clipped: true);
            if (rightClickedGUIItem != null)
            {
                if (rightClickedGUIItem.EASSerializable is EASBaseEvent)
                {
                    EASEditor.Instance.ShowEventOptionsMenu(rightClickedGUIItem.EASSerializable as EASBaseEvent);
                }
                else if (rightClickedGUIItem.EASSerializable is EASTrackGroup)
                {
                    EASEditor.Instance.ShowTrackGroupOptionsMenu(rightClickedGUIItem.EASSerializable as EASTrackGroup);
                }
                else if (rightClickedGUIItem.EASSerializable is EASTrack)
                {
                    EASEditor.Instance.ShowTrackOptionsMenu(rightClickedGUIItem.EASSerializable as EASTrack);
                }
            }
            else
            {
                EASEditor.Instance.ShowOptionsMenu();
            }
        }

        protected void OnLeftClickDrag()
        {
            if (m_DragInformation != null)
            {
                for (int i = 0; i < m_DragInformation.Items.Count; ++i)
                {
                    EASBaseGUIItem draggingItem = m_DragInformation.Items[i];
                    if (draggingItem.EASSerializable is EASBaseEvent)
                    {
                        EASBaseEvent baseEvent = draggingItem.EASSerializable as EASBaseEvent;
                        Vector2 distanceFromEventStart = draggingItem.Rect.position - m_DragInformation.InitialPosition;

                        float draggedEventStartHorizontalPosition = Event.current.mousePosition.x + distanceFromEventStart.x;
                        baseEvent.StartFrame = GetSafeFrameAtPosition(draggedEventStartHorizontalPosition, Mathf.RoundToInt(m_AnimationInformation.Frames - 1 - baseEvent.Duration));
                    }
                }

                m_DragInformation.OnDragPerformed(Event.current.mousePosition, AllEventsAreSafe());

                EditorWindow.focusedWindow.Repaint();
            }
        }

        protected void OnLeftClickUp()
        {
            if (m_DragInformation != null)
            {
                if (!m_DragInformation.DragPerformed && !EASEditor.Instance.HasMultipleSelectionModifier())
                {
                    EASBaseGUIItem leftClickedGUIItem = TimelineGUIItemAtMousePosition();
                    EASEditor.Instance.SelectObject(leftClickedGUIItem != null ? leftClickedGUIItem.EASSerializable : null, singleSelection: true);
                }
                else if (m_DragInformation.DragPerformed && !m_DragInformation.CanEndDrag)
                {
                    CancelDrag();
                }

                m_DragInformation = null;

                EditorWindow.focusedWindow.Repaint();
            }
        }

        protected void CancelDrag()
        {
            for (int i = 0; i < m_DragInformation.Items.Count; ++i)
            {
                if (m_DragInformation.Items[i].EASSerializable is EASBaseEvent)
                {
                    EASBaseEvent baseEvent = m_DragInformation.Items[i].EASSerializable as EASBaseEvent;
                    baseEvent.StartFrame = GetSafeFrameAtPosition(m_DragInformation.Items[i].Rect.x);
                }
            }
        }

        protected Rect TransformClippedRect(Rect clippedRect)
        {
            return new Rect(clippedRect.x + m_WholeTimelineRect.x, clippedRect.y + m_WholeTimelineRect.y, clippedRect.width, clippedRect.height);
        }

        protected EASBaseGUIItem TimelineTrackAtMousePosition(bool clipped = false)
        {
            for (int i = 0; i < m_TimelineTracksAndGroups.Count; ++i)
            {
                Rect rect = clipped ? m_TimelineTracksAndGroups[i].Rect : TransformClippedRect(m_TimelineTracksAndGroups[i].Rect);
                if (rect.Contains(Event.current.mousePosition))
                {
                    return m_TimelineTracksAndGroups[i];
                }
            }

            return null;
        }

        protected EASBaseGUIItem TimelineGUIEventAtMousePosition(bool clipped = false)
        {
            for (int i = 0; i < m_TimelineEvents.Count; ++i)
            {
                Rect rect = clipped ? m_TimelineEvents[i].Rect : TransformClippedRect(m_TimelineEvents[i].Rect);
                if (rect.Contains(Event.current.mousePosition))
                {
                    EASBaseEvent baseEvent = m_TimelineEvents[i].EASSerializable as EASBaseEvent;
                    return !baseEvent.ParentTrack.Locked && !baseEvent.ParentTrack.ParentTrackGroupLocked ? m_TimelineEvents[i] : null;
                }
            }

            return null;
        }

        protected EASBaseGUIItem TimelineGUIItemAtMousePosition(bool clipped = false)
        {
            EASBaseGUIItem eventAtClippedMousePosition = TimelineGUIEventAtMousePosition(clipped);
            if (eventAtClippedMousePosition != null)
            {
                return eventAtClippedMousePosition;
            }

            return TimelineTrackAtMousePosition(clipped);
        }

        public void OnAnimationChanged()
        {
            m_AnimationInformation = EASEditor.Instance.GetAnimationInformation();

            float framesAreaWidth = m_FramesAreaRect.width - EASSkin.TimelineRightMargin;
            m_PixelsPerFrame = framesAreaWidth / (m_AnimationInformation.Frames - 1);
        }

        protected float GetInitialFramePosition(bool clipped = false)
        {
            return (clipped ? 0.0f : m_WholeTimelineRect.x) + EASSkin.TimelineLeftMargin - m_ScrollPositionOffsets.x;
        }

        protected float GetHorizontalPositionAtFrame(float frame, bool clipped = false)
        {
            return GetInitialFramePosition(clipped) + frame * m_PixelsPerFrame;
        }

        protected float GetFrameAtPosition(float horizontalPosition, bool clipped = false)
        {
            return (horizontalPosition - GetInitialFramePosition(clipped)) / m_PixelsPerFrame;
        }

        protected int GetSafeFrameAtPosition(float horizontalPosition, int maxFrame, bool clipped = false)
        {
            return Mathf.RoundToInt(Mathf.Clamp(GetFrameAtPosition(horizontalPosition, clipped), 0, maxFrame));
        }

        public int GetSafeFrameAtPosition(float horizontalPosition, bool clipped = false)
        {
            return GetSafeFrameAtPosition(horizontalPosition, Mathf.RoundToInt(m_AnimationInformation.Frames - 1), clipped);
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

        public bool IsMouseOnTimeline(Vector2 mousePosition)
        {
            return m_WholeTimelineRect.Contains(mousePosition);
        }

        protected EASBaseGUIItem GetGUIItemOfEASSerializable(EASSerializable serializable)
        {
            int serializableID = (serializable as EASID).ID;
            if (serializable is EASBaseEvent)
            {
                for (int i = 0; i < m_TimelineEvents.Count; ++i)
                {
                    if ((m_TimelineEvents[i].EASSerializable as EASID).ID == serializableID)
                    {
                        return m_TimelineEvents[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_TimelineTracksAndGroups.Count; ++i)
                {
                    if ((m_TimelineTracksAndGroups[i].EASSerializable as EASID).ID == serializableID)
                    {
                        return m_TimelineTracksAndGroups[i];
                    }
                }
            }

            return null;
        }

        protected bool AllEventsAreSafe()
        {
            for (int i = 0; i < m_TimelineTracksAndGroups.Count; ++i)
            {
                EASTrack track = m_TimelineTracksAndGroups[i].EASSerializable as EASTrack;
                if (track != null)
                {
                    bool[] availableFrames = new bool[Mathf.RoundToInt(m_AnimationInformation.Frames)];
                    List<EASSerializable> trackEvents = track.Events;
                    for (int j = 0; j < trackEvents.Count; ++j)
                    {
                        EASBaseEvent baseEvent = trackEvents[j] as EASBaseEvent;
                        for (int k = 0; k < baseEvent.Duration; ++k)
                        {
                            if (availableFrames[baseEvent.StartFrame + k])
                            {
                                return false;
                            }
                            availableFrames[baseEvent.StartFrame + k] = true;
                        }
                    }
                }
            }

            return true;
        }
    }
}


