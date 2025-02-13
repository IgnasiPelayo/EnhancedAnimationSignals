using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    public class EASTimeline
    {
        protected List<EASBaseGUIItem> m_TimelineTracksAndGroups = new List<EASBaseGUIItem>();
        protected List<EASEventGUIItem> m_TimelineEvents = new List<EASEventGUIItem>();

        protected List<EASTimelineFrame> m_Frames = new List<EASTimelineFrame>();

        protected EASDragInformation m_DragInformation;

        [SerializeField]
        protected EASAnimationInformation m_AnimationInformation;
        public EASAnimationInformation AnimationInformation { get => m_AnimationInformation; }

        [SerializeField]
        protected List<EASAdditionalAnimationInformation> m_AdditionalAnimationInformations = new List<EASAdditionalAnimationInformation>();
        public List<EASAdditionalAnimationInformation> AdditionalAnimationInformations { get => m_AdditionalAnimationInformations; }

        [SerializeField]
        protected Vector2 m_ScrollPositionOffsets = Vector2.zero;

        [SerializeField]
        protected float m_PixelsPerFrame = 13;

        protected EASEditorUtils.EASTimescaledTimer m_TimelineTimer = new EASEditorUtils.EASTimescaledTimer();

        protected Rect m_WholeTimelineRect, m_FramesAreaRect;

        protected IEASSerializable m_RightClickedEASSerializable = null;

        public float CurrentFrame { get => m_TimelineTimer.ElapsedTime * m_AnimationInformation.FrameRate; set { SetElapsedTimeAtFrame(value); EASEditor.Instance.Playing = false; UpdateEvents(); } }

        public void OnUpdate()
        {
            if (m_PixelsPerFrame < 0 || (m_AnimationInformation != null && m_AnimationInformation.Animation == null))
            {
                OnAnimationChanged();
                EASEditor.Instance.Repaint();
            }

            if (!Application.isPlaying)
            {
                if (EASEditor.Instance.Playing)
                {
                    m_TimelineTimer.Update(EASEditor.Instance.TimeScale);

                    UpdateEvents();

                    if (m_TimelineTimer.StopIfElapsed())
                    {
                        OnAnimationEnd();

                        if (EASEditor.Instance.Loop)
                        {
                            m_TimelineTimer.Start(m_AnimationInformation.Length);
                        }
                        else
                        {
                            EASEditor.Instance.Playing = false;
                            EASEditor.Instance.Repaint();
                        }

                        EASEditor.Instance.TimeScale = 1.0f;
                    }
                }
                else if (m_DragInformation != null && m_DragInformation.DragType == EASDragInformation.EASDragType.TimerLine)
                {
                    UpdateEvents();
                }
            }
        }

        public void OnPlayModeChanged()
        {
            if (EASEditor.Instance.Playing)
            {
                if (m_TimelineTimer.IsElapsed() || !m_TimelineTimer.IsPaused)
                {
                    m_TimelineTimer.Start(m_AnimationInformation.Length);
                }
                else
                {
                    m_TimelineTimer.Resume();
                }
            }
            else
            {
                ResetEventsTrigger();
                m_TimelineTimer.Pause();
            }
        }

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

            OnGUIMutedOrLockedTracks();

            OnGUIKeyFrames();
            OnGUITimerLine();

            HandleClippedInput();

            GUI.EndClip();

            HandleInput();
            OnGUIDrag();
        }

        protected void UpdateEvents()
        {
            float currentFrame = CurrentFrame;
            int currentFrameInt = Mathf.FloorToInt(currentFrame);
            IEASEditorBridge editorBridge = EASEditor.Instance;

            List<EASBaseEvent> unmutedEASBaseEvents = EASEditor.Instance.GetEvents(addMuted: false);

            foreach (EASBaseEvent baseEvent in unmutedEASBaseEvents)
            {
                if (baseEvent.IsTriggered)
                {
                    if (!EASUtils.IsFrameInsideEvent(baseEvent, currentFrame))
                    {
                        baseEvent.IsTriggered = false;
                        baseEvent.OnEndEditor(currentFrameInt, editorBridge);
                    }
                    else if (!baseEvent.CanPreviewInEditor(editorBridge))
                    {
                        baseEvent.IsTriggered = false;
                        baseEvent.OnResetEditor(editorBridge);
                    }
                }
            }

            foreach (EASBaseEvent baseEvent in unmutedEASBaseEvents)
            {
                if (baseEvent.CanPreviewInEditor(editorBridge))
                {
                    if (EASUtils.IsFrameInsideEvent(baseEvent, currentFrame))
                    {
                        if (!baseEvent.IsTriggered)
                        {
                            baseEvent.IsTriggered = true;
                            baseEvent.OnStartEditor(currentFrameInt, editorBridge);
                        }

                        baseEvent.OnUpdateEditor(currentFrameInt, editorBridge);
                    }

                    baseEvent.OnUpdateTrackEditor(currentFrameInt, editorBridge);
                }
            }

            if (!AnimationMode.InAnimationMode() && !PrefabUtility.IsPartOfPrefabAsset(EASEditor.Instance.Controller))
            {
                AnimationMode.StartAnimationMode();
            }

            if (AnimationMode.InAnimationMode())
            {
                for (int i = 0; i < m_AdditionalAnimationInformations.Count; ++i)
                {
                    m_AdditionalAnimationInformations[i].Time = m_AnimationInformation.Length * ((currentFrame - m_AdditionalAnimationInformations[i].StartFrame) / m_AnimationInformation.Frames);
                    m_AdditionalAnimationInformations[i].Time = AdjustAnimationTimeToActualFrame(m_AdditionalAnimationInformations[i].Time, m_AdditionalAnimationInformations[i].FrameRate);
                }

                float time = AdjustAnimationTimeToActualFrame(m_TimelineTimer.ElapsedTime, m_AnimationInformation.FrameRate);
                EASEditor.Instance.Controller.PreviewAnimations(time, m_AnimationInformation, m_AdditionalAnimationInformations);

                m_AdditionalAnimationInformations.Clear();
            }
        }

        protected void OnAnimationEnd()
        {
            float currentFrame = CurrentFrame;
            int currentFrameInt = Mathf.FloorToInt(currentFrame);
            IEASEditorBridge editorBridge = EASEditor.Instance;

            List<EASBaseEvent> unmutedEASBaseEvents = EASEditor.Instance.GetEvents(addMuted: false);
            foreach (EASBaseEvent baseEvent in unmutedEASBaseEvents)
            {
                if (baseEvent.CanPreviewInEditor(editorBridge))
                {
                    if (baseEvent.IsTriggered)
                    {
                        baseEvent.OnEndEditor(currentFrameInt, editorBridge);
                    }

                    baseEvent.IsTriggered = false;
                    baseEvent.OnAnimationEndEditor(editorBridge);
                }
                else
                {
                    baseEvent.OnResetEditor(editorBridge);
                }
            }
        }

        protected float AdjustAnimationTimeToActualFrame(float currentTime, float frameRate)
        {
            int frame = Mathf.RoundToInt(currentTime * frameRate);
            return frame / frameRate;
        }

        protected void ResetEventsTrigger()
        {
            int currentFrame = Mathf.FloorToInt(m_TimelineTimer.ElapsedTime * m_AnimationInformation.FrameRate);
            IEASEditorBridge editorBridge = EASEditor.Instance;

            List<EASBaseEvent> unmutedEASBaseEvents = EASEditor.Instance.GetEvents(addMuted: false);
            foreach (EASBaseEvent baseEvent in unmutedEASBaseEvents)
            {
                baseEvent.IsTriggered = false;
                baseEvent.OnEndEditor(currentFrame, editorBridge);
            }
        }

        protected void OnGUIFramesArea()
        {
            m_Frames.Clear();

            EditorGUI.DrawRect(m_FramesAreaRect, EASSkin.TimelineFramesBackgroundColor);

            Rect lineRect = new Rect(0, m_FramesAreaRect.y, 1f, m_FramesAreaRect.height - 1);
            for (int i = 0; i <= m_AnimationInformation.Frames; ++i)
            {
                EASTimelineFrameType frameType = GetFrameType(i, m_AnimationInformation.Frames);
                EASTimelineFrame timelineFrame = new EASTimelineFrame(frame: i, GetHorizontalPositionAtFrame(i, clipped: true), frameType, GetFrameColor(frameType));
                m_Frames.Add(timelineFrame);

                DrawFrame(lineRect, timelineFrame, isFrameLine: false);
            }
        }

        protected void OnGUITrackBackgrounds(Rect timelineWorkAreaRect)
        {
            m_TimelineTracksAndGroups.Clear();

            Rect timelineTrackRect = Rect.MinMaxRect(timelineWorkAreaRect.x, timelineWorkAreaRect.y + EASSkin.HierarchyUpperMargin, timelineWorkAreaRect.xMax, timelineWorkAreaRect.y + EASSkin.HierarchyTrackHeight);

            List<IEASSerializable> tracksAndGroups = EASEditor.Instance.GetTracksAndGroups();
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
                    List<IEASSerializable> events = new List<IEASSerializable>(track.Events);
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

            if (EASEditor.Instance.IsSelected(baseEvent) && m_DragInformation != null && !m_DragInformation.IsValidDrag)
            {
                EditorGUI.DrawRect(eventRect, EASSkin.DraggedItemErrorColor);
            }
            else
            {
                BaseOnGUIEventBackground(eventRect, baseEvent);
            }

            Rect eventSeparatorRect = new Rect(eventRect.x, eventRect.y, 1, eventRect.height);
            EditorGUI.DrawRect(eventSeparatorRect, EASSkin.TimelineEventSeparatorColor);

            if (eventRect.width >= 3.0f)
            {
                Rect leftResizeRect = new Rect(eventRect.x, eventRect.y, Mathf.Clamp(EASSkin.TimelineEventResizeRectsMaxWidth, 1.0f, eventRect.width / 3.0f), eventRect.height);
                Rect rightResizeRect = new Rect(eventRect.xMax - leftResizeRect.width, eventRect.y, leftResizeRect.width, eventRect.height);

                m_TimelineEvents.Add(new EASEventGUIItem(eventRect, leftResizeRect, rightResizeRect, baseEvent));
                EditorGUIUtility.AddCursorRect(leftResizeRect, MouseCursor.ResizeHorizontal);
                EditorGUIUtility.AddCursorRect(rightResizeRect, MouseCursor.ResizeHorizontal);
            }
            else
            {
                m_TimelineEvents.Add(new EASEventGUIItem(eventRect, baseEvent));
            }
        }

        public void BaseOnGUIEventBackground(Rect rect, EASBaseEvent baseEvent)
        {
            EditorGUI.DrawRect(rect, EASEditorUtils.GetEASEventColorAttribute(baseEvent.GetType()));

            if (baseEvent.HasError(EASEditor.Instance.Controller))
            {
                ExtendedGUI.ExtendedGUI.DrawOutlineRect(rect, EASSkin.TimelineEventErrorColor, Mathf.FloorToInt(Mathf.Min(rect.width / 2.0f, 4)));
            }
        }

        protected void OnGUISelectedEvent(EASEventGUIItem eventGUIItem)
        {
            if (EASEditor.Instance.IsSelected(eventGUIItem.EASSerializable))
            {
                if (m_DragInformation != null && !m_DragInformation.IsValidDrag)
                {
                    ExtendedGUI.ExtendedGUI.DrawOutlineRect(eventGUIItem.Rect, EASSkin.TimelineEventSelectedBorderColor, 1);
                }
                else
                {
                    BaseOnGUIEventSelected(eventGUIItem.Rect, eventGUIItem.EASSerializable as EASBaseEvent);
                }
            }
        }

        public void BaseOnGUIEventSelected(Rect rect, EASBaseEvent baseEvent)
        {
            EditorGUI.DrawRect(rect, EASSkin.TimelineEventSelectedColor);
            ExtendedGUI.ExtendedGUI.DrawOutlineRect(rect, EASSkin.TimelineEventSelectedBorderColor, 1);
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

            Color textColor = ExtendedGUI.ExtendedGUI.GetContrastingLabelColor(EASEditorUtils.GetEASEventColorAttribute(baseEvent.GetType()));

            labelGUIStyle.normal.textColor = labelGUIStyle.hover.textColor = labelGUIStyle.active.textColor = textColor;
            GUI.Label(eventLabelRect, eventLabelContent, labelGUIStyle);
        }

        protected void OnGUIMutedOrLockedTracks()
        {
            for (int i = 0; i < m_TimelineTracksAndGroups.Count; ++i)
            {
                EASBaseTrack baseTrack = m_TimelineTracksAndGroups[i].EASSerializable as EASBaseTrack;
                if (baseTrack is EASTrack)
                {
                    EASTrack track = baseTrack as EASTrack;
                    if (track.ParentTrackGroup != null && (track.ParentTrackGroupLocked || track.ParentTrackGroupMuted))
                    {
                        continue;
                    }
                }

                if (baseTrack.Muted || baseTrack.Locked)
                {
                    string trackStatus = (baseTrack.Locked ? "Locked" : string.Empty);
                    trackStatus += (baseTrack.Muted ? (string.IsNullOrEmpty(trackStatus) ? "Muted" : " and Muted") : string.Empty);
                    GUIContent trackStatusGUIContent = new GUIContent(trackStatus);

                    Rect trackStatusBackgroundRect = m_TimelineTracksAndGroups[i].Rect;
                    if (m_TimelineTracksAndGroups[i].EASSerializable is EASTrackGroup)
                    {
                        trackStatusBackgroundRect.height = EASSkin.HierarchyTrackHeight;
                    }

                    EditorGUI.DrawRect(m_TimelineTracksAndGroups[i].Rect, EASSkin.TimelineLockedMutedBackgroundColor);

                    Vector2 trackStatusSize = EditorStyles.label.CalcSize(trackStatusGUIContent);
                    Rect trackStatusRect = ExtendedGUI.ExtendedGUI.GetInnerRect(trackStatusBackgroundRect, trackStatusSize.x + 10, EASSkin.HierarchyTrackHeight - 10);

                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = false;
                    GUI.Button(trackStatusRect, trackStatusGUIContent);
                    GUI.enabled = wasEnabled;
                }
            }
        }

        protected void OnGUIKeyFrames()
        {
            float keyFrameSize = Mathf.Min(EASSkin.TimelineKeyFrameSize, m_PixelsPerFrame - 2);
            for (int i = 0; i < m_AnimationInformation.KeyFrames.Count; ++i)
            {
                Rect keyFrameRect = new Rect(GetHorizontalPositionAtFrame(m_AnimationInformation.KeyFrames[i], clipped: true) - EASSkin.TimelineHalfKeyFrameSize,
                    m_FramesAreaRect.yMax - EASSkin.TimelineHalfKeyFrameSize, EASSkin.TimelineKeyFrameSize, EASSkin.TimelineKeyFrameSize);

                GUI.DrawTexture(keyFrameRect, EASSkin.Icon("animationdopesheetkeyframe"));
            }
        }

        protected void OnGUITimerLine()
        {
            Rect timerLineRect = new Rect(GetHorizontalPositionAtTime(m_TimelineTimer.ElapsedTime, clipped: true) - 1, 0, 2, m_WholeTimelineRect.height);
            EditorGUI.DrawRect(timerLineRect, Color.white);
        }

        protected void OnGUIDrag()
        {
            if (m_DragInformation != null && m_DragInformation.DragPerformed)
            {
                if (m_DragInformation.DragType == EASDragInformation.EASDragType.NormalDrag)
                {
                    Vector2Int startEndFrame = new Vector2Int(int.MaxValue, 0);
                    for (int i = 0; i < m_DragInformation.Items.Count; ++i)
                    {
                        if (m_DragInformation.Items[i].EASSerializable is EASBaseEvent)
                        {
                            EASBaseEvent baseEvent = m_DragInformation.Items[i].EASSerializable as EASBaseEvent;
                            startEndFrame.x = Mathf.Min(baseEvent.StartFrame, startEndFrame.x);
                            startEndFrame.y = Mathf.Max(baseEvent.LastFrame, startEndFrame.y);
                        }
                    }

                    Rect timelineFramesAreaHighlightRect = Rect.MinMaxRect(GetHorizontalPositionAtFrame(startEndFrame.x), m_WholeTimelineRect.y, GetHorizontalPositionAtFrame(startEndFrame.y), m_WholeTimelineRect.y + m_FramesAreaRect.height);
                    EditorGUI.DrawRect(timelineFramesAreaHighlightRect, EASSkin.TimelineEventSelectedColor);

                    OnGUIDragVerticalLine(startEndFrame.x);
                    OnGUIDragVerticalLine(startEndFrame.y);
                }
                else if (m_DragInformation.DragType != EASDragInformation.EASDragType.TimerLine)
                {
                    Rect resizeRect = new Rect(Event.current.mousePosition - Vector2.one * 20, Vector2.one * 40);
                    EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);

                    EASBaseEvent resizingBaseEvent = m_DragInformation.Items[0].EASSerializable as EASBaseEvent;
                    OnGUIDragVerticalLine(m_DragInformation.DragType == EASDragInformation.EASDragType.ResizeLeft ? resizingBaseEvent.StartFrame : resizingBaseEvent.LastFrame);
                }
            }
        }

        protected void OnGUIDragVerticalLine(int frame)
        {
            Rect verticalLineRect = new Rect(GetHorizontalPositionAtFrame(frame), m_WholeTimelineRect.y, 1, m_WholeTimelineRect.height);
            ExtendedGUI.ExtendedGUI.DrawVerticalDottedLine(verticalLineRect, Color.black, 7, 3);

            Vector2 frameLabelSize = EASSkin.WhiteMiniLabelStyle.CalcSize(new GUIContent(frame.ToString()));
            Rect startFrameRect = new Rect(verticalLineRect.x - frameLabelSize.x / 2.0f, verticalLineRect.y, frameLabelSize.x, frameLabelSize.y);
            EditorGUI.DrawRect(startFrameRect, Color.black);
            GUI.Label(startFrameRect, frame.ToString(), EASSkin.WhiteMiniLabelStyle);
        }

        protected void HandleClippedInput()
        {
            if (m_RightClickedEASSerializable != null)
            {
                if (m_RightClickedEASSerializable is EASBaseEvent)
                {
                    EASEditor.Instance.ShowEventOptionsMenu(m_RightClickedEASSerializable as EASBaseEvent);
                }
                else if (m_RightClickedEASSerializable is EASTrackGroup)
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
                    CreateDragInformation(leftClickedGUIItem);
                }
            }
            else if (m_FramesAreaRect.Contains(Event.current.mousePosition))
            {
                MoveTimerLineToMousePosition(clipped: true);
                m_DragInformation = new EASDragInformation(Event.current.mousePosition + m_WholeTimelineRect.position, new List<EASDragGUIItem>(), EASDragInformation.EASDragType.TimerLine);
            }
            else
            {
                EASEditor.Instance.SelectObject(null, singleSelection: true);
            }
        }

        protected void MoveTimerLineToMousePosition(bool clipped)
        {
            EASEditor.Instance.Playing = false;

            float frameAtMousePosition = Mathf.Clamp(GetFrameAtPosition(Event.current.mousePosition.x, clipped), 0, m_AnimationInformation.Frames);
            SetElapsedTimeAtFrame(frameAtMousePosition);

            EASEditor.Instance.Repaint();
        }

        public void SetElapsedTimeAtFrame(float frame)
        {
            frame = Mathf.Clamp(frame, 0, m_AnimationInformation.Frames);
            float elapsedTime = frame * m_AnimationInformation.Length / m_AnimationInformation.Frames;

            m_TimelineTimer.Start(m_AnimationInformation.Length, elapsedTime);
            m_TimelineTimer.Pause();
        }

        protected void CreateDragInformation(EASBaseGUIItem clickedGUIItem)
        {
            EASEventGUIItem eventGUIItem = clickedGUIItem as EASEventGUIItem;
            EASDragInformation.EASDragType dragType = EASDragInformation.EASDragType.NormalDrag;
            if (eventGUIItem.HasResizeRects)
            {
                if (eventGUIItem.ResizeLeftRect.Contains(Event.current.mousePosition)) dragType = EASDragInformation.EASDragType.ResizeLeft;
                else if (eventGUIItem.ResizeRightRect.Contains(Event.current.mousePosition)) dragType = EASDragInformation.EASDragType.ResizeRight;
            }

            List<EASDragGUIItem> draggedGUIItems = new List<EASDragGUIItem>();
            if (dragType == EASDragInformation.EASDragType.NormalDrag)
            {
                draggedGUIItems.Add(new EASDragGUIItem(TransformClippedRect(clickedGUIItem.Rect), clickedGUIItem.EASSerializable));
            }
            else
            {
                draggedGUIItems.Add(new EASDragGUIItem(TransformClippedRect(clickedGUIItem.Rect), clickedGUIItem.EASSerializable, eventGUIItem.EventStartPositionAndDuration));
            }

            List<IEASSerializable> selectedEvents = EASEditor.Instance.GetSelected<EASBaseEvent>();

            for (int i = 0; i < selectedEvents.Count; ++i)
            {
                if (EASEditorUtils.GetSerializableID(selectedEvents[i]) != EASEditorUtils.GetSerializableID(clickedGUIItem.EASSerializable))
                {
                    EASBaseGUIItem baseGUIItem = GetGUIItemOfEASSerializable(selectedEvents[i]);
                    draggedGUIItems.Add(new EASDragGUIItem(TransformClippedRect(baseGUIItem.Rect), baseGUIItem.EASSerializable));
                }
            }

            m_DragInformation = new EASDragInformation(Event.current.mousePosition + m_WholeTimelineRect.position, draggedGUIItems, dragType);
        }

        protected void OnRightClickDownClipped()
        {
            EASBaseGUIItem rightClickedGUIItem = TimelineGUIItemAtMousePosition(clipped: true);
            if (rightClickedGUIItem != null)
            {
                EASEditor.Instance.SelectObject(rightClickedGUIItem.EASSerializable, singleSelection: true);

                m_RightClickedEASSerializable = rightClickedGUIItem.EASSerializable;
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
                if (m_DragInformation.DragType == EASDragInformation.EASDragType.NormalDrag)
                {
                    PerformNormalDrag();
                }
                else if (m_DragInformation.DragType == EASDragInformation.EASDragType.TimerLine)
                {
                    PerformTimerLineDrag();
                }
                else 
                {
                    if (m_DragInformation.DragType == EASDragInformation.EASDragType.ResizeLeft)
                    {
                        PerformResizeLeftDrag();
                    }
                    else
                    {
                        PerformResizeRightDrag();
                    }
                    Rect resizeRectAtMousePosition = new Rect(Event.current.mousePosition.x - 20, Event.current.mousePosition.y - 20, 40, 40);
                    EditorGUIUtility.AddCursorRect(resizeRectAtMousePosition, MouseCursor.ResizeHorizontal);
                }

                m_DragInformation.OnDragPerformed(Event.current.mousePosition, isValidDrag: ValidateAllEventsPositions(), m_PixelsPerFrame);
                EditorWindow.focusedWindow.Repaint();
            }
        }

        protected void PerformNormalDrag()
        {
            EASTrack trackAtMousePosition = null;
            EASBaseGUIItem trackGUIItemAtMousePosition = TimelineTrackAtPosition(Event.current.mousePosition);
            if (trackGUIItemAtMousePosition != null && trackGUIItemAtMousePosition.EASSerializable is EASTrack)
            {
                trackAtMousePosition = trackGUIItemAtMousePosition.EASSerializable as EASTrack;
            }

            bool allowVerticalDrag = m_DragInformation.AllowVerticalDrag() && trackAtMousePosition != null && !trackAtMousePosition.Locked && !trackAtMousePosition.ParentTrackGroupLocked;

            for (int i = 0; i < m_DragInformation.Items.Count; ++i)
            {
                EASBaseGUIItem draggingItem = m_DragInformation.Items[i];
                if (draggingItem.EASSerializable is EASBaseEvent)
                {
                    EASBaseEvent baseEvent = draggingItem.EASSerializable as EASBaseEvent;
                    if (allowVerticalDrag && EASEditorUtils.GetSerializableID(baseEvent.ParentTrack) != EASEditorUtils.GetSerializableID(trackGUIItemAtMousePosition.EASSerializable))
                    {
                        EASEditor.Instance.MoveEvent(baseEvent, trackAtMousePosition);
                    }

                    Vector2 distanceFromEventStart = draggingItem.Rect.position - m_DragInformation.InitialPosition;

                    float draggedEventStartHorizontalPosition = Event.current.mousePosition.x + distanceFromEventStart.x;
                    baseEvent.StartFrame = GetSafeFrameAtPosition(draggedEventStartHorizontalPosition, Mathf.RoundToInt(m_AnimationInformation.Frames - baseEvent.Duration));
                }
            }
        }

        protected void PerformTimerLineDrag()
        {
            MoveTimerLineToMousePosition(clipped: false);
        }

        protected void PerformResizeLeftDrag()
        {
            EASBaseGUIItem resizingGUIItem = m_DragInformation.Items[0];
            if (resizingGUIItem.EASSerializable is EASBaseEvent)
            {
                EASBaseEvent baseEvent = resizingGUIItem.EASSerializable as EASBaseEvent;

                Vector2 distanceFromEventStart = resizingGUIItem.Rect.position - m_DragInformation.InitialPosition;
                float resizingEventStartHorizontalPosition = Event.current.mousePosition.x + distanceFromEventStart.x;

                int previousLastFrame = baseEvent.LastFrame;
                baseEvent.StartFrame = Mathf.Clamp(GetSafeFrameAtPosition(resizingEventStartHorizontalPosition, Mathf.RoundToInt(m_AnimationInformation.Frames)), 0, baseEvent.LastFrame - 1);
                baseEvent.Duration = previousLastFrame - baseEvent.StartFrame;
            }
        }

        protected void PerformResizeRightDrag()
        {
            EASBaseGUIItem resizingGUIItem = m_DragInformation.Items[0];
            if (resizingGUIItem.EASSerializable is EASBaseEvent)
            {
                EASBaseEvent baseEvent = resizingGUIItem.EASSerializable as EASBaseEvent;

                Vector2 distanceFromEventEnd = new Vector2(resizingGUIItem.Rect.xMax, resizingGUIItem.Rect.y) - m_DragInformation.InitialPosition;
                float resizingEventEndHorizontalPosition = Event.current.mousePosition.x + distanceFromEventEnd.x;

                int frameAtMousePosition = GetSafeFrameAtPosition(resizingEventEndHorizontalPosition, Mathf.RoundToInt(m_AnimationInformation.Frames));
                baseEvent.Duration = Mathf.Clamp(frameAtMousePosition - baseEvent.StartFrame, 1, int.MaxValue);
            }
        }

        protected bool ValidateAllEventsPositions()
        {
            int trackLength = EASEditor.Instance.GetCurrentAnimationFrames();
            List<IEASSerializable> tracksAndGroups = EASEditor.Instance.GetTracksAndGroups();
            for (int i = 0; i < tracksAndGroups.Count; ++i)
            {
                if ((tracksAndGroups[i] is EASTrackGroup && !EASEditorUtils.ValidateTrackGroupEventPositions(tracksAndGroups[i] as EASTrackGroup, trackLength)) ||
                    (tracksAndGroups[i] is EASTrack && !EASEditorUtils.ValidateTrackEventPositions(tracksAndGroups[i] as EASTrack, trackLength)))
                {
                    return false;
                }
            }

            return true;
        }

        protected void OnLeftClickUp()
        {
            if (m_DragInformation != null)
            {
                if (m_DragInformation.DragType != EASDragInformation.EASDragType.TimerLine)
                {
                    if (!m_DragInformation.DragPerformed && !EASEditor.Instance.HasMultipleSelectionModifier())
                    {
                        EASBaseGUIItem leftClickedGUIItem = TimelineGUIItemAtMousePosition();
                        EASEditor.Instance.SelectObject(leftClickedGUIItem != null ? leftClickedGUIItem.EASSerializable : null, singleSelection: true);
                    }
                    else if (m_DragInformation.DragPerformed)
                    {
                        if (m_DragInformation.IsValidDrag)
                        {
                            FinishDrag();
                        }
                        else
                        {
                            CancelDrag();
                        }
                    }
                }

                m_DragInformation = null;

                EditorWindow.focusedWindow.Repaint();
            }
        }

        protected void FinishDrag()
        {
            List<IEASSerializable> tracksAndGroups = EASEditor.Instance.GetTracksAndGroups();
            for (int i = 0; i < tracksAndGroups.Count; ++i)
            {
                if (tracksAndGroups[i] is EASTrackGroup)
                {
                    EASTrackGroup trackGroup = tracksAndGroups[i] as EASTrackGroup;
                    for (int j = 0; j < trackGroup.Tracks.Count; ++j)
                    {
                        trackGroup.Tracks[j].ReorderEvents();
                    }
                }
                else if (tracksAndGroups[i] is EASTrack)
                {
                    EASTrack track = tracksAndGroups[i] as EASTrack;
                    track.ReorderEvents();
                }
            }

            EditorUtility.SetDirty(EASEditor.Instance.Controller.Data);
        }

        protected void CancelDrag()
        {
            if (m_DragInformation.DragType == EASDragInformation.EASDragType.NormalDrag)
            {
                for (int i = 0; i < m_DragInformation.Items.Count; ++i)
                {
                    if (m_DragInformation.Items[i].EASSerializable is EASBaseEvent)
                    {
                        EASBaseEvent baseEvent = m_DragInformation.Items[i].EASSerializable as EASBaseEvent;
                        baseEvent.StartFrame = GetSafeFrameAtPosition(m_DragInformation.Items[i].Rect.x);

                        EASTrack eventTrack = m_DragInformation.Items[i].Context as EASTrack;
                        if (baseEvent.ParentTrack != eventTrack)
                        {
                            EASEditor.Instance.MoveEvent(baseEvent, eventTrack);
                        }
                    }
                }
            }
            else
            {
                if (m_DragInformation.Items[0].EASSerializable is EASBaseEvent)
                {
                    EASBaseEvent baseEvent = m_DragInformation.Items[0].EASSerializable as EASBaseEvent;
                    Vector2Int eventStartPositionAndDuration = (Vector2Int)m_DragInformation.Items[0].Context;
                    baseEvent.StartFrame = eventStartPositionAndDuration.x;
                    baseEvent.Duration = eventStartPositionAndDuration.y;
                }
            }
        }

        protected Rect TransformClippedRect(Rect clippedRect)
        {
            return new Rect(clippedRect.x + m_WholeTimelineRect.x, clippedRect.y + m_WholeTimelineRect.y, clippedRect.width, clippedRect.height);
        }

        protected EASBaseGUIItem TimelineTrackAtPosition(Vector2 position, bool clipped = false)
        {
            for (int i = 0; i < m_TimelineTracksAndGroups.Count; ++i)
            {
                Rect rect = clipped ? m_TimelineTracksAndGroups[i].Rect : TransformClippedRect(m_TimelineTracksAndGroups[i].Rect);
                if (rect.Contains(position))
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

            return TimelineTrackAtPosition(Event.current.mousePosition, clipped);
        }

        public void OnAnimationChanged()
        {
            OnAnimationEnd();

            m_AnimationInformation = EASEditor.Instance.GetAnimationInformation();

            float framesAreaWidth = m_FramesAreaRect.width - EASSkin.TimelineRightMargin;
            m_PixelsPerFrame = framesAreaWidth / m_AnimationInformation.Frames;

            SetElapsedTimeAtFrame(0.0f);
        }

        protected float GetInitialFramePosition(bool clipped = false)
        {
            return (clipped ? 0.0f : m_WholeTimelineRect.x) + EASSkin.TimelineLeftMargin - m_ScrollPositionOffsets.x;
        }

        protected float GetHorizontalPositionAtFrame(float frame, bool clipped = false)
        {
            return GetInitialFramePosition(clipped) + frame * m_PixelsPerFrame;
        }

        protected float GetHorizontalPositionAtTime(float time, bool clipped = false)
        {
            return GetInitialFramePosition(clipped) + (time / m_AnimationInformation.Length * m_AnimationInformation.Frames) * m_PixelsPerFrame;
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
            return GetSafeFrameAtPosition(horizontalPosition, Mathf.RoundToInt(m_AnimationInformation.Frames), clipped);
        }

        protected EASTimelineFrameType GetFrameType(float frame, float lastFrame)
        {
            if (Mathf.RoundToInt(frame) == Mathf.RoundToInt(lastFrame))
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

        protected EASBaseGUIItem GetGUIItemOfEASSerializable(IEASSerializable serializable)
        {
            int serializableID = EASEditorUtils.GetSerializableID(serializable);
            if (serializable is EASBaseEvent)
            {
                for (int i = 0; i < m_TimelineEvents.Count; ++i)
                {
                    if (EASEditorUtils.GetSerializableID(m_TimelineEvents[i].EASSerializable) == serializableID)
                    {
                        return m_TimelineEvents[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_TimelineTracksAndGroups.Count; ++i)
                {
                    if (EASEditorUtils.GetSerializableID(m_TimelineTracksAndGroups[i].EASSerializable) == serializableID)
                    {
                        return m_TimelineTracksAndGroups[i];
                    }
                }
            }

            return null;
        }
    }
}


