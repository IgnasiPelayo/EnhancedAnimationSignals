using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFD700"), EASEventCategory("Debug"), EASEventTooltip("Log a message to the Console using Unity's Debug system")]
    public class EASLogEvent : EASEvent
    {
        [SerializeField]
        protected string m_Message;
        public string Message { get => m_Message; set => m_Message = value; }

#if UNITY_EDITOR
        public override string GetErrorMessage()
        {
            return $"Variable {nameof(Message)} is empty";
        }

        public override bool HasError(EASBaseController owner)
        {
            return string.IsNullOrEmpty(m_Message);
        }
#endif // UNITY_EDITOR
    }
}
