using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public EASAnimationInformation(object animation, float length, float frameRate)
        {
            m_Animation = animation;

            m_Length = length;
            m_FrameRate = frameRate;
        }
    }

    public class EASBaseGUIItem
    {
        protected Rect m_Rect;
        public Rect Rect { get => m_Rect; }

        protected EASSerializable m_EASSerializable;
        public EASSerializable EASSerializable { get => m_EASSerializable; }

        public EASBaseGUIItem(Rect rect, EASSerializable serializable)
        {
            m_Rect = rect;
            m_EASSerializable = serializable;
        }
    }

    public class EASEventGUIItem : EASBaseGUIItem
    {
        public EASEventGUIItem(Rect rect, EASBaseEvent baseEvent) : base(rect, baseEvent)
        {

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

    public class EASDragInformation
    {
        protected Vector2 m_InitialPosition;
        public Vector2 InitialPosition { get => m_InitialPosition; }

        protected List<EASBaseGUIItem> m_Items = new List<EASBaseGUIItem>();
        public List<EASBaseGUIItem> Items { get => m_Items; }

        protected bool m_DragPerformed = false;
        public bool DragPerformed { get => m_DragPerformed; }

        protected bool m_CanEndDrag = true;
        public bool CanEndDrag { get => m_CanEndDrag; }

        public EASDragInformation(Vector2 initialPosition, List<EASBaseGUIItem> items)
        {
            m_InitialPosition = initialPosition;
            m_Items = items;

            GUIUtility.hotControl = GetControlId();
        }

        public void OnDragPerformed(Vector2 position, bool canEndDrag)
        {
            if (!m_DragPerformed)
            {
                m_DragPerformed = position != m_InitialPosition;
            }

            m_CanEndDrag = canEndDrag;
        }

        protected static int GetControlId() => GUIUtility.GetControlID(FocusType.Passive);

        public EventType GetEventType() => Event.current.GetTypeForControl(GetControlId());

        public bool IsBeingDragged(EASSerializable serializable)
        {
            int serializableID = EASUtils.GetSerializableID(serializable);
            for (int i = 0; i < m_Items.Count; ++i)
            {
                if (EASUtils.GetSerializableID(m_Items[i].EASSerializable) == serializableID)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
