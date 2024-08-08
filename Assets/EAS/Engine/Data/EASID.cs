using UnityEngine;
using CustomAttributes;

namespace EAS
{
    public interface EASSerializable { }

    [System.Serializable]
    public class EASID : EASSerializable
    {
#if UNITY_EDITOR
        public const int INVALID_ID = 0;

        [SerializeField, ReadOnly]
        protected int m_ID;
        public int ID { get => m_ID; set => m_ID = value; }
#endif // UNITY_EDITOR

        [HideInInspector]
        public string m_Name;
        public string Name { get => m_Name; set => m_Name = value; }

        public static int GenerateID() => System.Guid.NewGuid().GetHashCode();
    }
}
