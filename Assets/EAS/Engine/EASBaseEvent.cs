using System;
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
        public virtual void OnValidate() { }
        public virtual void OnAnimationModified(IEASEditorBridge editorBridge) { }

        public virtual string GetLabel() => m_Name;

        public virtual bool HasOwnerType(EASBaseController owner) => true;
        public virtual bool IsObjectCompatible(GameObject root) => true;

        public bool HasError(string errorMessage) => !string.IsNullOrEmpty(errorMessage);
        public bool HasError(EASBaseController controller) => HasError(GetErrorMessage(controller));
        public virtual string GetErrorMessage(EASBaseController controller) => string.Empty;

        public static EASBaseEvent Create(System.Type type)
        {
            EASBaseEvent newEvent = System.Activator.CreateInstance(type) as EASBaseEvent;
            return newEvent;
        }

        public EASBaseEvent Duplicate()
        {
            return MemberwiseClone() as EASBaseEvent;
        }

        public virtual bool CanPreviewInEditor(IEASEditorBridge editorBridge) => true;

        public virtual void OnStartEditor(int currentFrame, IEASEditorBridge editorBridge) { }
        public virtual void OnUpdateEditor(int currentFrame, IEASEditorBridge editorBridge) { }
        public virtual void OnUpdateTrackEditor(int currentFrame, IEASEditorBridge editorBridge) { }
        public virtual void OnEndEditor(int currentFrame, IEASEditorBridge editorBridge) { }
        public virtual void OnAnimationEndEditor(IEASEditorBridge editorBridge) { }
        public virtual void OnResetEditor(IEASEditorBridge editorBridge) { }
        public virtual void OnDeleteEvent(IEASEditorBridge editorBridge) { }

        public virtual void CreateRuntimeData(EASBaseController controller) 
        {
            m_Controller = controller;

            System.Type baseReferenceType = typeof(EASBaseReference);

            System.Reflection.FieldInfo[] fields = GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                System.Reflection.FieldInfo field = fields[i];
                System.Type fieldType = field.FieldType;

                if (baseReferenceType.IsAssignableFrom(fieldType))
                {
                    EASBaseReference baseReference = field.GetValue(this) as EASBaseReference;
                    baseReference.GenerateRuntimeData(controller);
                }
                else if ((fieldType.IsArray && baseReferenceType.IsAssignableFrom(fieldType.GetElementType())) || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    System.Collections.IList iList = (System.Collections.IList)field.GetValue(this);
                    for (int j = 0; j < iList.Count; ++j)
                    {
                        EASBaseReference baseReference = iList[j] as EASBaseReference;
                        baseReference.GenerateRuntimeData(controller);
                    }
                }
            }
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
        public override bool HasOwnerType(EASBaseController controller)
        {
            if (typeof(T) == typeof(Animator))
            {
                return controller.gameObject.GetComponent<Animator>() != null;
            }

            if (controller.DataRoot != null)
            {
                T[] componentsInChildren = controller.DataRoot.GetComponentsInChildren<T>(includeInactive: true);
                return componentsInChildren.Length > 0;
            }

            return false;
        }

        public virtual T GetOwner(EASBaseController controller)
        {
            if (typeof(T) == typeof(Animator))
            {
                return controller.gameObject.GetComponent<T>();
            }

            if (controller.DataRoot != null)
            {
                T[] componentsInChildren = controller.DataRoot.GetComponentsInChildren<T>(includeInactive: true);
                return componentsInChildren.Length > 0 ? componentsInChildren[0] : null;
            }

            return null;
        }

        public override void CreateRuntimeData(EASBaseController controller)
        {
            m_Owner = GetOwner(controller);

            base.CreateRuntimeData(controller);
        }
#endif // UNITY_EDITOR
    }

    [System.Serializable]
    public abstract class EASEvent : EASCustomOwnerEvent<EASBaseController>
    {

    }
}


