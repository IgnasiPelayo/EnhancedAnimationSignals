using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#008080"), EASEventCategory("General"), EASEventTooltip("Stops the playback of a specified Particle System")]
    public class EASStopParticlesEvent : EASBaseParticlesEvent
    {
        [Space(10)]

        [SerializeField]
        protected ParticleSystemStopBehavior m_StopBehaviour = ParticleSystemStopBehavior.StopEmittingAndClear;

        public override void OnStart(float currentFrame)
        {
            if (m_ParticleSystem != null && m_ParticleSystem.Instance != null)
            {
                if (m_ParticleSystem.Instance.isPaused)
                {
                    m_ParticleSystem.Instance.Play(withChildren: true);
                }
                m_ParticleSystem.Instance.Stop(withChildren: true, m_StopBehaviour);
            }
        }

#if UNITY_EDITOR
        protected override void OnUpdateTrackEditorInternal(int currentFrame, float elapsedTime, ParticleSystem particleSystem, IEASEditorBridge editorBridge)
        {
            if (currentFrame < StartFrame)
            {
                editorBridge.FreePreviewObject(particleSystem, this);

                if (ResolvePreviewConflicts(editorBridge.GetPreviewObjectConflicts(particleSystem)))
                {
                    if (!m_Previewing)
                    {
                        StartPreview(particleSystem, elapsedTime + 10);
                    }
                    else
                    {
                        UpdatePreview(particleSystem, elapsedTime);
                    }
                }
            }
            else
            {
                editorBridge.BlockPreviewObject(particleSystem, this);

                if (m_Previewing)
                {
                    StopPreview(particleSystem, m_StopBehaviour);
                }
            }
        }

        protected override bool ResolvePreviewConflicts(List<IEASSerializable> conflicts)
        {
            if (conflicts.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < conflicts.Count; ++i)
            {
                if (conflicts[i] is EASStopParticlesEvent)
                {
                    return false;
                }

                if (conflicts[i] is EASPlayParticlesEvent)
                {
                    EASPlayParticlesEvent playParticlesEvent = conflicts[i] as EASPlayParticlesEvent;
                    if (playParticlesEvent.StartFrame <= StartFrame)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
#endif // UNITY_EDITOR
    }
}
