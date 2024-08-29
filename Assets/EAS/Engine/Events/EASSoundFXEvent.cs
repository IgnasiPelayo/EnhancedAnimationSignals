using UnityEngine;

namespace EAS
{
    [EASEventColor("#9D387F"), EASEventCategory("General"), EASEventTooltip("Plays a sound effect using a specified AudioClip")]
    public class EASSoundFXEvent : EASBaseSoundEvent
    {
        [Header("Audio Settings")]
        [SerializeField, Tooltip("The AudioClip to play.")]
        protected EASGlobalReference<AudioClip> m_AudioClip = new EASGlobalReference<AudioClip>();

        [SerializeField, Tooltip("If enabled, the AudioClip will loop continuously until stopped.")]
        protected bool m_Loop = false;

        [SerializeField, Tooltip("The volume at which the AudioClip will be played.")]
        [Range(0f, 1f)]
        protected float m_Volume = 1.0f;

        [SerializeField, Tooltip("The pitch at which the AudioClip will be played.")]
        [Range(-3f, 3f)]
        protected float m_Pitch = 1.0f;

        public override void OnStart(float currentFrame)
        {
            if (m_AudioClip.Instance != null)
            {
                PlaySound(m_AudioClip.Instance, m_Volume, m_Pitch, m_Loop);
            }
        }

#if UNITY_EDITOR
        public override string GetErrorMessage(EASBaseController controller)
        {
            if (m_AudioClip == null || m_AudioClip.Resolve(controller) == null)
            {
                return "AudioClip is not set";
            }

            return string.Empty;
        }

        public override void OnStartEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            AudioClip audioClip = m_AudioClip.Resolve(editorBridge.Controller);
            if (audioClip != null)
            {
                Initialize();

                PreviewSound(audioClip);
            }
        }
#endif // UNITY_EDITOR
    }
}
