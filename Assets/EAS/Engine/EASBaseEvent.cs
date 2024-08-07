using CustomAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    public abstract class EASBaseEvent : EASID
    {
        [SerializeField, HideInInspector]
        protected uint m_StartFrame = 0;
        public uint StartFrame
        {
            get => m_StartFrame;
#if UNITY_EDITOR
            set { m_StartFrame = value; OnDurationChanged(); }
#endif // UNITY_EDITOR
        }

        [SerializeField, HideInInspector]
        protected uint m_Duration = 1;
        public uint Duration
        {
            get => m_Duration;
#if UNITY_EDITOR
            set { m_Duration = value; OnDurationChanged(); }
#endif // UNITY_EDITOR
        }

        public uint LastFrame => m_StartFrame + m_Duration;

        protected bool m_IsTriggered;
        public bool IsTriggered { get => m_IsTriggered; set => m_IsTriggered = value; }

        // ------------------------------------------------------------------------------------------------------------------

        public virtual void OnStart(float currentFrame) { }
        public virtual void OnUpdate(float progress) { }
        public virtual void OnAnimationEnd() { }
        public virtual void OnEnd() { }
        public virtual void OnSkip(float currentFrame)
        {
            OnStart(currentFrame);
            OnUpdate(1.0f);
            OnEnd();
        }

        protected virtual void OnDurationChanged() { }
    }
}


