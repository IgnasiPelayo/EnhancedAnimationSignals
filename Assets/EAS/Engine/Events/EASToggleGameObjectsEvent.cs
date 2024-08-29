using UnityEngine;

namespace EAS
{
    [EASEventColor("#32CD32"), EASEventCategory("General"), EASEventTooltip("Activates or deactivates a list of GameObjects. Optionally resets them to their original state at the end of the event")]
    public class EASToggleGameObjectsEvent : EASEvent
    {
        [SerializeField, Tooltip("The array of GameObjects to activate or deactivate.")]
        protected EASReference<GameObject>[] m_GameObjects = System.Array.Empty<EASReference<GameObject>>();

        [Space(10)]

        [Header("Toggle Settings")]
        [SerializeField, Tooltip("If enabled, the GameObjects will be activated. If disabled, they will be deactivated.")]
        private bool m_Activate = true;

        [SerializeField, Tooltip("If enabled, the GameObjects will be reset to their original active state at the end of the event.")]
        private bool m_ResetOnFinish = false;

        public override void OnStart(float currentFrame)
        {
            ToggleGameObjects(m_Activate);
        }

        public override void OnEnd()
        {
            if (m_ResetOnFinish)
            {
                ToggleGameObjects(!m_Activate);
            }
        }

        protected void ToggleGameObjects(bool activate)
        {
            for (int i = 0; i < m_GameObjects.Length; ++i)
            {
                if (m_GameObjects[i] != null && m_GameObjects[i].Instance != null)
                {
                    m_GameObjects[i].Instance.SetActive(activate);
                }
            }
        }
    }
}
