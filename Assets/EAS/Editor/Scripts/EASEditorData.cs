using UnityEngine;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    public class EASAnimationInformation
    {
        [SerializeField]
        protected object m_Animation;
        public object Animation { get => m_Animation; }

        [SerializeField]
        protected float m_Length;
        public float Length { get => m_Length; }
        
        [SerializeField]
        protected float m_FrameRate;
        public float FrameRate { get => m_FrameRate; }

        public float Frames { get => Mathf.Clamp(m_Length * m_FrameRate, 1.0f, float.MaxValue); }

        [SerializeField]
        protected List<int> m_KeyFrames;
        public List<int> KeyFrames { get => m_KeyFrames; }

        public EASAnimationInformation(object animation, float length, float frameRate, List<int> keyFrames)
        {
            m_Animation = animation;

            m_Length = length;
            m_FrameRate = frameRate;

            m_KeyFrames = new List<int>(keyFrames);
        }
    }

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
}
