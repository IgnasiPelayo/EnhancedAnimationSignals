using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [CreateAssetMenu(fileName = "EAS Data", menuName = "EAS Data", order = 459)]
    public class EASData : ScriptableObject
    {
        [SerializeField]
        protected long m_TimeStamp = DateTime.MinValue.Ticks;
        public long TimeStamp { get => m_TimeStamp; set => m_TimeStamp = value; }

        [SerializeField]
        protected List<EASAnimationData> m_Data;
    }
}
