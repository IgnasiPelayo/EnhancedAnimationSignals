using UnityEngine;
using CustomAttributes;

namespace EAS
{
    public class EASID
    {
#if UNITY_EDITOR
        public const int INVALID_ID = 0;

        [SerializeField, ReadOnly]
        protected int m_ID;
        public int ID { get => m_ID; }
#endif // UNITY_EDITOR

        public static int GenerateID() => System.Guid.NewGuid().GetHashCode();
    }
}
