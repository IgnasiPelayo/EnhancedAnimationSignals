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

        protected Dictionary<System.Type, List<int>> m_ActiveMarkups = new Dictionary<System.Type, List<int>>(4);
        protected Dictionary<System.Type, List<int>> m_SkippedMarkups = new Dictionary<System.Type, List<int>>(4);

        protected void Start()
        {
#if UNITY_EDITOR
            GenerateRuntimeData();
#endif // UNITY_EDITOR
        }

        protected void LateUpdate()
        {
            m_SkippedEvents.Clear();
            m_SkippedMarkups.Clear();

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

        public T GetActiveEvent<T>() where T : EASBaseEvent
        {
            for (int i = 0; i < m_ActiveEvents.Count; ++i)
            {
                if (m_ActiveEvents[i] is T)
                {
                    return m_ActiveEvents[i] as T;
                }
            }

            return null;
        }

        public void AddActiveMarkup(System.Type markupType, int markup)
        {
            if (m_ActiveMarkups.ContainsKey(markupType))
            {
                m_ActiveMarkups[markupType].Add(markup);
            }
            else
            {
                m_ActiveMarkups.Add(markupType, new List<int>(capacity: 8) { markup });
            }

#if UNITY_EDITOR
            Debug.Assert(m_ActiveMarkups[markupType].Count < 5, $"Reached markup limit of type {markupType} in EASBaseController {m_DataRoot.gameObject.name}. Maximum capacity is 4.", m_DataRoot.gameObject);
#endif // UNITY_EDITOR
        }

        public void RemoveActiveMarkup(System.Type markupType, int markup)
        {
            if (m_ActiveMarkups.ContainsKey(markupType))
            {
                List<int> activeMarkups = m_ActiveMarkups[markupType];
                for (int i = 0; i < activeMarkups.Count; ++i)
                {
                    if (activeMarkups[i] == markup)
                    {
                        m_ActiveMarkups[markupType].RemoveAt(i);
                        return;
                    }
                }
            }

            Debug.LogError($"Can't remove markup {markup} of type {markupType}. EASBaseController {m_DataRoot.gameObject.name} doesn't have this markup as active");
        }

        public void AddSkippedMarkup(System.Type markupType, int markup)
        {
            if (m_SkippedMarkups.ContainsKey(markupType))
            {
                m_SkippedMarkups[markupType].Add(markup);
            }
            else
            {
                m_SkippedMarkups.Add(markupType, new List<int>() { markup });
            }
        }

        public bool IsMarkupActive(System.Type markupType, int markup)
        {
            if (m_ActiveMarkups.ContainsKey(markupType))
            {
                List<int> activeMarkups = m_ActiveMarkups[markupType];
                for (int i = 0; i < activeMarkups.Count; ++i)
                {
                    if (activeMarkups[i] == markup)
                    {
                        return true;
                    }
                }
            }

            if (m_SkippedMarkups.ContainsKey(markupType))
            {
                List<int> skippedMarkups = m_SkippedMarkups[markupType];
                for (int i = 0; i < skippedMarkups.Count; ++i)
                {
                    if (skippedMarkups[i] == markup)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

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
                    List<EASBaseEvent> unmutedEvents = animationData.GetEvents(addMuted: false);
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
