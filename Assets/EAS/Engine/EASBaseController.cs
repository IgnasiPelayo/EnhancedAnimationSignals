using UnityEngine;
using System.Collections.Generic;
using static Unity.VisualScripting.Member;

namespace EAS
{
    public abstract class EASBaseController : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField]
        protected EASData m_Data;
        public EASData Data { get => m_Data; }
#endif // UNITY_EDITOR

        [SerializeField]
        protected Transform m_DataRoot;
        public Transform DataRoot { get => m_DataRoot; }
        public GameObject DataRootGameObject { get => m_DataRoot.gameObject; }

        [SerializeField, HideInInspector]
        protected EASRuntimeData m_RuntimeData = new EASRuntimeData();

        protected List<EASBaseEvent> m_ActiveEvents = new List<EASBaseEvent>(16);
        protected List<EASBaseEvent> m_SkippedEvents = new List<EASBaseEvent>(8);

        protected void Start()
        {
#if UNITY_EDITOR
            GenerateRuntimeData();
#endif // UNITY_EDITOR
        }

        protected void LateUpdate()
        {
            m_SkippedEvents.Clear();
            m_RuntimeData.Update(this);
        }

        public void AddActiveEvent(EASBaseEvent baseEvent) => m_ActiveEvents.Add(baseEvent);

        public void RemoveActiveEvent(EASBaseEvent baseEvent)
        {
            for (int i = 0; i < m_ActiveEvents.Count; ++i)
            {
                if (m_ActiveEvents[i] == baseEvent)
                {
                    m_ActiveEvents.RemoveAt(i);
                    return;
                }
            }
        }

        public void AddSkippedEvent(EASBaseEvent baseEvent) => m_SkippedEvents.Add(baseEvent);

        public abstract string[] GetAnimationNames();

        public abstract EASAnimationState GetCurrentAnimationState();

#if UNITY_EDITOR
        [ContextMenu("Generate Runtime Data")]
        public void GenerateRuntimeData()
        {
            List<EASRuntimeAnimationInformation> runtimeAnimationInformations = new List<EASRuntimeAnimationInformation>();

            for (int i = 0; i < m_Data.AnimationData.Count; ++i)
            {
                EASAnimationData animationData = m_Data.AnimationData[i];
                
                EASAnimationInformation animationInformation = GetAnimation(animationData.Name);
                if (animationInformation != null)
                {
                    List<EASBaseEvent> unmutedEvents = animationData.GetUnmutedEvents();
                    if (unmutedEvents.Count > 0)
                    {
                        List<EASBaseEvent> baseEvents = new List<EASBaseEvent>(unmutedEvents.Count);
                        for (int j = 0; j < unmutedEvents.Count; ++j)
                        {
                            EASBaseEvent unmutedEvent = unmutedEvents[j];

                            EASBaseEvent baseEvent = JsonUtility.FromJson(JsonUtility.ToJson(unmutedEvent), unmutedEvent.GetType()) as EASBaseEvent;
                            baseEvent.CreateRuntimeData(this);

                            baseEvents.Add(baseEvent);
                        }

                        runtimeAnimationInformations.Add(new EASRuntimeAnimationInformation(hash: Animator.StringToHash(animationData.Name), frames: Mathf.RoundToInt(animationInformation.Frames),
                            frameRate: animationInformation.FrameRate, baseEvents));
                    }
                }
            }

            m_RuntimeData = new EASRuntimeData() { Animations = runtimeAnimationInformations };
        }

        public abstract EASAnimationInformation GetAnimation(string animationName);

        public abstract EASAnimationInformation[] GetAnimations();

        public abstract void PreviewAnimations(float time, EASAnimationInformation animation, List<EASAdditionalAnimationInformation> animations);
#endif // UNITY_EDITOR
    }
}
