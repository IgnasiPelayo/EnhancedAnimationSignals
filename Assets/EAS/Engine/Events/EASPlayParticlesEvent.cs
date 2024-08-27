
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFA500"), EASEventCategory("Visual Effects"), EASEventTooltip("Triggers the playback of a specified Particle System")]
    public class EASPlayParticlesEvent : EASBaseParticlesEvent
    {
#if UNITY_EDITOR
        public override void OnUpdateTrackEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            ParticleSystem particleSystem = m_ParticleSystem.Resolve(editorBridge.Controller);
            if (particleSystem != null)
            {
                if (editorBridge.ShowParticleSystems)
                {
                    if (m_LastFrameUpdate != currentFrame)
                    {
                        float elapsedTime = (currentFrame - m_LastFrameUpdate) / editorBridge.FrameRate;
                        if (currentFrame >= StartFrame && elapsedTime > 0.0f)
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
                        else if (m_Previewing)
                        {
                            StopPreview(particleSystem);
                        }

                        m_LastFrameUpdate = currentFrame;
                    }
                }
                else if (m_Previewing)
                {
                    StopPreview(particleSystem);
                }
            }
        }
#endif // UNITY_EDITOR
    }
}
