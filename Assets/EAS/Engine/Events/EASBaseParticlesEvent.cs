using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    public abstract class EASBaseParticlesEvent : EASBaseEvent
    {
        [Header("Particle System Settings")]
        [SerializeField, Tooltip("The Particle System to play/stop.")]
        protected EASReference<ParticleSystem> m_ParticleSystem = new EASReference<ParticleSystem>();

#if UNITY_EDITOR
        protected bool m_Previewing;
        protected int m_LastFrameUpdate = -1;

        public override string GetErrorMessage(EASBaseController controller)
        {
            if (m_ParticleSystem.Resolve(controller) == null)
            {
                return "Particle System is not set";
            }

            return base.GetErrorMessage(controller);
        }

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
                        OnUpdateTrackEditorInternal(currentFrame, elapsedTime, particleSystem, editorBridge);

                        m_LastFrameUpdate = currentFrame;
                    }
                }
                else if (m_Previewing)
                {
                    StopPreview(particleSystem);
                }
            }
        }

        protected abstract void OnUpdateTrackEditorInternal(int currentFrame, float elapsedTime, ParticleSystem particleSystem, IEASEditorBridge editorBridge);

        protected abstract bool ResolvePreviewConflicts(List<IEASSerializable> conflicts);

        public override void OnDisableEditor(IEASEditorBridge editorBridge)
        {
            ParticleSystem particleSystem = m_ParticleSystem.Resolve(editorBridge.Controller);
            if (particleSystem != null && m_Previewing)
            {
                StopPreview(particleSystem);
            }
        }

        public override void OnAnimationEndEditor(IEASEditorBridge editorBridge)
        {
            ParticleSystem particleSystem = m_ParticleSystem.Resolve(editorBridge.Controller);
            if (particleSystem != null && m_Previewing)
            {
                StopPreview(particleSystem);
            }
        }

        protected void StartPreview(ParticleSystem particleSystem, float elapsedTime)
        {
            particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Simulate(elapsedTime, withChildren: true, restart: true);
            particleSystem.time = elapsedTime;

            m_Previewing = true;
        }

        protected void UpdatePreview(ParticleSystem particleSystem, float elapsedTime)
        {
            particleSystem.Simulate(elapsedTime, withChildren: true, restart: false);
            particleSystem.time = elapsedTime;
        }

        protected void StopPreview(ParticleSystem particleSystem, ParticleSystemStopBehavior stopBehaviour = ParticleSystemStopBehavior.StopEmittingAndClear)
        {
            particleSystem.Stop(withChildren: true, stopBehaviour);

            m_Previewing = false;
        }
#endif // UNITY_EDITOR
    }
}
