using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    public struct TestClass
    {
        [SerializeField]
        public int m_Health;

        [SerializeField]
        public string m_Name;

        public List<TestClass2> m_Stats;
    }

    [System.Serializable]
    public struct TestClass2
    {
        [SerializeField]
        public int m_Health;

        [SerializeField]
        public string m_Name;
    }

    [System.Serializable]
    [EASEventColor("#FFFFFF"), EASEventTooltip("This is a test event to show default property inspectors")]
    public class EASTestEvent : EASEvent
    {
        public List<Transform> List;

        [SerializeField]
        public int m_Health;

        public TestClass testClass;

        [Space(10)]
        public Transform m_Transform;

#if UNITY_EDITOR
        public override string GetErrorMessage()
        {
            return string.Empty;
        }

        public override bool HasError(EASBaseController owner)
        {
            return false;
        }
#endif // UNITY_EDITOR
    }
}