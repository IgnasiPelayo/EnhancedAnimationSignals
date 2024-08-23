using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFD700"), EASEventCategory("Debug"), EASEventTooltip("Log a message to the Console using Unity's Debug system")]
    public class EASLogEvent : EASEvent
    {
        [Header("Message Settings")]
        [SerializeField, Tooltip("The message to log.")]
        protected string m_Message;

        [Space(10)]

        [Header("Log Settings")]
        [SerializeField, Tooltip("If enabled, the log will be triggered only once on the first frame. If disabled, the log will be triggered on every frame for the duration of the event.")]
        protected bool m_LogOnce = false;

        [SerializeField, Tooltip("The type of log to generate.")]
        protected LogType m_LogType = LogType.Log;

        protected void Log()
        {
            if (m_LogType == LogType.Log)
            {
                Debug.Log(m_Message);
            }
            else if (m_LogType == LogType.Warning)
            {
                Debug.LogWarning(m_Message);
            }
            else if (m_LogType == LogType.Error)
            {
                Debug.LogError(m_Message);
            }
        }

#if UNITY_EDITOR
        public override string GetErrorMessage(EASBaseController owner)
        {
            string errorMessage = string.Empty;
            if (string.IsNullOrEmpty(m_Message))
            {
                errorMessage = $"Message is empty";
            }

            if (m_LogType == LogType.Assert || m_LogType == LogType.Exception)
            {
                errorMessage += (string.IsNullOrEmpty(errorMessage) ? "" : " and ") + $"LogType can't be Assert or Exception";
            }

            return errorMessage;
        }

        public override void OnStartEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            Log();
        }

        public override void OnUpdateEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            if (!m_LogOnce)
            {
                Log();
            }
        }
#endif // UNITY_EDITOR
    }
}
