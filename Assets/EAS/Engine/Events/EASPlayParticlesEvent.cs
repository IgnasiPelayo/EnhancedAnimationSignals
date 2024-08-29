using UnityEngine;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFA500"), EASEventCategory("General"), EASEventTooltip("Triggers the playback of a specified Particle System")]
    public class EASPlayParticlesEvent : EASBaseParticlesEvent
    {
        public override void OnStart(float currentFrame)
        {
            if (m_ParticleSystem != null && m_ParticleSystem.Instance != null)
            {
                m_ParticleSystem.Instance.Play();
            }
        }

#if UNITY_EDITOR
        protected override void OnUpdateTrackEditorInternal(int currentFrame, float elapsedTime, ParticleSystem particleSystem, IEASEditorBridge editorBridge)
        {
            if (currentFrame >= StartFrame && elapsedTime > 0.0f)
            {
                editorBridge.FreePreviewObject(particleSystem, this);

                if (ResolvePreviewConflicts(editorBridge.GetPreviewObjectConflicts(particleSystem)))
                {
                    if (!m_Previewing)
                    {
                        elapsedTime = (currentFrame - StartFrame) / editorBridge.FrameRate;
                        StartPreview(particleSystem, elapsedTime);
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
                    StopPreview(particleSystem);
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
                if (conflicts[i] is EASPlayParticlesEvent)
                {
                    return false;
                }

                if (conflicts[i] is EASStopParticlesEvent)
                {
                    EASStopParticlesEvent stopParticlesEvent = conflicts[i] as EASStopParticlesEvent;
                    if (stopParticlesEvent.StartFrame >= StartFrame)
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
