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

#if UNITY_EDITOR
        public abstract EASAnimationInformation GetAnimation(string animationName);

        public abstract void PreviewAnimations(float time, EASAnimationInformation animation, List<EASAdditionalAnimationInformation> animations);
#endif // UNITY_EDITOR
    }
}
