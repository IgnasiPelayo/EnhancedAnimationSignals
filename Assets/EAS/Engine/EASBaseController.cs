using UnityEngine;

namespace EAS
{
    public abstract class EASBaseController : MonoBehaviour
    {
        [SerializeField]
        protected EASData m_Data;

        public EASData Data { get => m_Data; }

        public abstract string[] GetAnimationNames();

        public abstract object GetAnimation(string animationName);

        public abstract float GetLength(object animation);

        public abstract float GetFrameRate(object animation);
    }
}
