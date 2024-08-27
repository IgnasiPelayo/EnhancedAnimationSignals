using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#4682B4"), EASEventCategory("Visual Effects"), EASEventTooltip("Stops the playback of a specified Particle System")]
    public class EASStopParticlesEvent : EASBaseParticlesEvent
    {
        [Space(10)]

        [SerializeField]
        protected ParticleSystemStopBehavior m_StopBehaviour = ParticleSystemStopBehavior.StopEmittingAndClear;
    }
}
