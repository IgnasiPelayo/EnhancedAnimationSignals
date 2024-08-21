using UnityEngine;
using System.Collections.Generic;

namespace EAS
{
    public abstract class EASBaseController : MonoBehaviour
    {
        [SerializeField]
        protected EASData m_Data;
        public EASData Data { get => m_Data; }

        [SerializeField]
        protected Transform m_DataRoot;
        public Transform DataRoot { get => m_DataRoot; }
        public GameObject DataRootGameObject { get => m_DataRoot.gameObject; }

        public abstract string[] GetAnimationNames();

        public abstract object GetAnimation(string animationName);

        public abstract float GetLength(object animation);

        public abstract float GetFrameRate(object animation);

#if UNITY_EDITOR
        public abstract List<int> GetKeyFrames(object animation);
#endif // UNITY_EDITOR
    }
}
