using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    public abstract class EASBaseController : MonoBehaviour
    {
        [SerializeField]
        protected EASData m_Data;

        public EASData Data { get => m_Data; }
    }
}
