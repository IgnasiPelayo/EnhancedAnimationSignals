using CustomAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    public abstract class EASBaseEvent : EASID
    {
        public override string DefaultName => EASUtils.GetReadableEventName(GetType(), replaceEvent: true);

        [SerializeField, HideInInspector]
        protected EASBaseController m_Controller;

        [SerializeReference, HideInInspector]
        protected IEASSerializable m_ParentTrack;
        public EASTrack ParentTrack { get => m_ParentTrack as EASTrack; set => m_ParentTrack = value; }

        [SerializeField, HideInInspector]
        protected int m_StartFrame = 0;
        public int StartFrame
        {
            get => m_StartFrame;
#if UNITY_EDITOR
            set { m_StartFrame = value; OnDurationChanged(); }
#endif // UNITY_EDITOR
        }

        [SerializeField, HideInInspector]
        protected int m_Duration = 1;
        public int Duration
        {
            get => m_Duration;
#if UNITY_EDITOR
            set { m_Duration = value; OnDurationChanged(); }
#endif // UNITY_EDITOR
        }

        public virtual int DefaultDuration { get => 3; }

        public int LastFrame => m_StartFrame + m_Duration;

        protected bool m_IsTriggered;
        public bool IsTriggered { get => m_IsTriggered; set => m_IsTriggered = value; }

        // ------------------------------------------------------------------------------------------------------------------

        public EASBaseEvent()
        {
#if UNITY_EDITOR
            if (m_ID == INVALID_ID)
            {
                m_ID = GenerateID();
                m_Name = EASUtils.GetReadableEventName(GetType());
            }
#endif // UNITY_EDITOR
        }

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

#if UNITY_EDITOR
        public virtual string GetLabel() => m_Name;

        public virtual bool HasOwnerType(EASBaseController owner) => true;
        public virtual bool IsObjectCompatible(GameObject root) => true;

        public abstract bool HasError(EASBaseController owner);
        public abstract string GetErrorMessage();

        public static EASBaseEvent Create(System.Type type)
        {
            EASBaseEvent newEvent = System.Activator.CreateInstance(type) as EASBaseEvent;


            return newEvent;
        }
#endif // UNITY_EDITOR
    }

    [System.Serializable]
    public abstract class EASCustomOwnerEvent<T> : EASBaseEvent where T : Behaviour
    {
        [SerializeField, HideInInspector]
        protected T m_Owner;
        public T Owner { get => m_Owner; }

#if UNITY_EDITOR
        public override bool HasOwnerType(EASBaseController owner)
        {
            if (typeof(T) == typeof(Animator))
            {
                return owner.gameObject.GetComponent<Animator>() != null;
            }

            if (owner.DataRoot != null)
            {
                T[] componentsInChildren = owner.DataRoot.GetComponentsInChildren<T>(includeInactive: true);
                return componentsInChildren.Length > 0;
            }

            return false;
        }

        public virtual T GetOwner(EASBaseController owner)
        {
            if (typeof(T) == typeof(Animator))
            {
                return owner.gameObject.GetComponent<T>();
            }

            if (owner.DataRoot != null)
            {
                T[] componentsInChildren = owner.DataRoot.GetComponentsInChildren<T>(includeInactive: true);
                return componentsInChildren.Length > 0 ? componentsInChildren[0] : null;
            }

            return null;
        }
#endif // UNITY_EDITOR
    }

    [System.Serializable]
    public abstract class EASEvent : EASCustomOwnerEvent<EASBaseController>
    {

    }
}


