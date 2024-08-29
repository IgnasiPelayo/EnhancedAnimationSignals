using UnityEngine;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    public class EASRuntimeAnimationInformation
    {
        [SerializeField]
        protected int m_Hash;
        public int Hash { get { return m_Hash; } }

        [SerializeField]
        protected int m_Frames;
        public int Frames { get { return m_Frames; } }

        [SerializeField]
        protected float m_FrameRate;
        public float FrameRate { get { return m_FrameRate; } }

        [SerializeReference]
        protected List<EASBaseEvent> m_Events;

        public List<EASBaseEvent> Events { get { return m_Events; } }
        
        public void ResetEvents()
        {
            for (int i = 0; i < m_Events.Count; ++i)
            {
                m_Events[i].IsTriggered = false;
            }
        }

        public EASRuntimeAnimationInformation(int hash, int frames, float frameRate, List<EASBaseEvent> events)
        {
            m_Hash = hash;
            m_Frames = frames;
            m_FrameRate = frameRate;
            m_Events = events;
        }
    }

    [System.Serializable]
    public class EASPreviousAnimationInformation
    {
        [SerializeField]
        protected int m_Index = -1;
        public int Index { get => m_Index; set => m_Index = value; }

        [SerializeField]
        protected int m_Frame;
        public int Frame { get => m_Frame; set => m_Frame = value; }
    }

    [System.Serializable]
    public class EASRuntimeData
    {
        [SerializeField]
        protected List<EASRuntimeAnimationInformation> m_Animations = new List<EASRuntimeAnimationInformation>();
        public List<EASRuntimeAnimationInformation> Animations { get => m_Animations; set => m_Animations = value; }

        protected EASPreviousAnimationInformation m_PreviousAnimation;


        public int GetIndexOfAnimationHash(int hash)
        {
            for (int i = 0; i < m_Animations.Count; ++i)
            {
                if (m_Animations[i].Hash == hash)
                {
                    return i;
                }
            }

            return -1;
        }

        protected float GetFrame(float normalizedTime, bool isLoop, float frames)
        {
            normalizedTime = isLoop ? normalizedTime % 1.0f : Mathf.Clamp01(normalizedTime);
            frames = Mathf.Max(1.0f, frames);

            return Mathf.Min(Mathf.Abs(normalizedTime) * frames, frames - 0.001f);
        }

        public void Update(EASBaseController controller)
        {
            EASAnimationState animationState = controller.GetCurrentAnimationState();
            
            Debug.Assert(animationState.AnimationHash != 0, $"Animation Hash can't be 0, as it is considered an invalid hash", controller.gameObject);

            bool animationChanged = false;

            int currentAnimationIndex = GetIndexOfAnimationHash(animationState.AnimationHash);
            if (m_PreviousAnimation == null)
            {
                m_PreviousAnimation = new EASPreviousAnimationInformation() { Frame = -1 };
                animationChanged = true;
            }
            else if (m_PreviousAnimation.Index != currentAnimationIndex)
            {
                m_PreviousAnimation.Frame = -1;
                animationChanged = true;
            }

            float currentFrame = currentAnimationIndex != -1 ? GetFrame(animationState.NormalizedTime, animationState.IsLoop, m_Animations[currentAnimationIndex].Frames) : -1.0f;
            int currentFrameInt = Mathf.FloorToInt(currentFrame);

            bool isLoop = !animationChanged && currentFrameInt < m_PreviousAnimation.Frame;

            if (m_PreviousAnimation.Index != -1)
            {
                EASRuntimeAnimationInformation previousRuntimeAnimationInformation = m_Animations[m_PreviousAnimation.Index];
                bool shouldStopAllEvents = isLoop || animationChanged;

                for (int i = 0; i < previousRuntimeAnimationInformation.Events.Count; ++i)
                {
                    EASBaseEvent baseEvent = previousRuntimeAnimationInformation.Events[i];
                    if (baseEvent.IsTriggered && (shouldStopAllEvents || baseEvent.LastFrame <= currentFrame))
                    {
                        EndEvent(baseEvent, controller);
                    }
                }

                if (animationChanged)
                {
                    for (int i = 0; i < previousRuntimeAnimationInformation.Events.Count; ++i)
                    {
                        EASBaseEvent baseEvent = previousRuntimeAnimationInformation.Events[i];
                        AnimationEndEvent(baseEvent);
                    }
                }
            }

            if (currentAnimationIndex != -1)
            {
                EASRuntimeAnimationInformation runtimeAnimationInformation = m_Animations[currentAnimationIndex];

                for (int i = 0; i < runtimeAnimationInformation.Events.Count; ++i)
                {
                    EASBaseEvent baseEvent = runtimeAnimationInformation.Events[i];
                    if (baseEvent.StartFrame > currentFrameInt)
                    {
                        continue;
                    }

                    float eventProgress = Mathf.Clamp01((currentFrame - (float)baseEvent.StartFrame) / (float)baseEvent.Duration);
                    if (baseEvent.IsTriggered)
                    {
                        if (EASUtils.IsFrameInsideEvent(baseEvent, currentFrameInt))
                        {
                            baseEvent.OnUpdate(eventProgress);
                        }
                        continue;
                    }

                    if (EASUtils.IsFrameInsideEvent(baseEvent, currentFrameInt))
                    {
                        StartEvent(baseEvent, currentFrame, eventProgress, controller);
                    }

                    if (isLoop)
                    {
                        // The animation is looping and the event was skipped
                        if (baseEvent.LastFrame <= currentFrameInt)
                        {
                            StartEvent(baseEvent, currentFrame, eventProgress, controller);
                        }
                    }
                    else if (!animationChanged)
                    {
                        // The event was skipped while playing the same animation in the previous Update
                        if (baseEvent.StartFrame > m_PreviousAnimation.Frame && baseEvent.LastFrame <= currentFrameInt)
                        {
                            SkipEvent(baseEvent, currentFrame, controller);
                        }
                    }
                }
            }

            m_PreviousAnimation.Index = currentAnimationIndex;
            m_PreviousAnimation.Frame = currentFrameInt;
        }

        protected void AnimationEndEvent(EASBaseEvent baseEvent)
        {
            baseEvent.OnAnimationEnd();
        }

        protected void StartEvent(EASBaseEvent baseEvent, float currentFrame, float eventProgress, EASBaseController controller)
        {
            baseEvent.OnStart(currentFrame);
            baseEvent.IsTriggered = true;
            baseEvent.OnUpdate(eventProgress);

            controller.AddActiveEvent(baseEvent);
        }

        protected void EndEvent(EASBaseEvent baseEvent, EASBaseController controller)
        {
            baseEvent.OnEnd();
            baseEvent.IsTriggered = false;

            controller.RemoveActiveEvent(baseEvent);
        }

        protected void SkipEvent(EASBaseEvent baseEvent, float currentFrame, EASBaseController controller)
        {
            baseEvent.OnSkip(currentFrame);

            controller.AddSkippedEvent(baseEvent);
        }
    }
}
