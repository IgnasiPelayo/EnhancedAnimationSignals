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
}
