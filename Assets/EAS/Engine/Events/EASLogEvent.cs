using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFD700"), EASEventCategory("Debug"), EASTooltip("Log a message using Unity's Debug system")]
    public class EASLogEvent : EASEvent
    {
        [SerializeField]
        protected string m_Message;
        public string Message { get => m_Message; set => m_Message = value; }

#if UNITY_EDITOR
        public override string GetErrorMessage()
        {
            return "No Log message";
        }

        public override bool HasError(EASBaseController owner)
        {
            return false;
        }
#endif // UNITY_EDITOR
    }
}
