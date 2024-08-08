using UnityEngine;

namespace EAS
{
    public abstract class EASBaseController : MonoBehaviour
    {
        [SerializeField]
        protected EASData m_Data;

        public EASData Data { get => m_Data; }

        public abstract string[] GetAnimationNames();
    }
}
