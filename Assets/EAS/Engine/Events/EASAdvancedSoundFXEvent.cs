using UnityEngine;

namespace EAS
{
    [EASEventColor("#9D3862"), EASEventCategory("General"), EASEventTooltip("Plays a randomly selected sound effect from a list of AudioClips with customizable volume and pitch ranges")]
    public class EASAdvancedSoundFXEvent : EASBaseSoundEvent
    {
        [SerializeField, Tooltip("The array of AudioClips to choose from. A random clip will be selected to play.")]
        protected EASGlobalReference<AudioClip>[] m_AudioClips = System.Array.Empty<EASGlobalReference<AudioClip>>();
        protected AudioClip m_LastAudioClip;

        [Header("Settings")]
        [SerializeField, Tooltip("If enabled, the AudioClip will loop continuously until stopped.")]
        protected bool m_Loop = false;

        [SerializeField, Tooltip("The range for the volume. A random value between the x (minimum) and y (maximum) components will be used.")]
        protected Vector2 m_Volume = new Vector2(1.0f, 1.0f);

        [SerializeField, Tooltip("The range for the pitch. A random value between the x (minimum) and y (maximum) components will be used.")]
        protected Vector2 m_Pitch = new Vector2(1.0f, 1.0f);

        public override void OnStart(float currentFrame)
        {
            AudioClip audioClip = GetRandomAudioClip(null);
            if (audioClip != null)
            {
                PlaySound(audioClip, Random.Range(m_Volume.x, m_Volume.y), Random.Range(m_Pitch.x, m_Pitch.y), m_Loop);
            }
        }

        protected AudioClip GetRandomAudioClip(EASBaseController controller)
        {
            if (m_AudioClips.Length > 0)
            {
                int randomNumber = Random.Range(0, m_AudioClips.Length - 1);
                m_LastAudioClip = controller != null ? m_AudioClips[randomNumber].Resolve(controller) : m_AudioClips[randomNumber].Instance;
                return m_LastAudioClip;
            }
            m_LastAudioClip = null;
            return m_LastAudioClip;
        }

#if UNITY_EDITOR
        public override void OnValidate()
        {
            m_Volume.x = Mathf.Clamp01(m_Volume.x);
            m_Volume.y = Mathf.Clamp01(m_Volume.y);

            m_Pitch.x = Mathf.Clamp(m_Pitch.x, -3.0f, 3.0f);
            m_Pitch.y = Mathf.Clamp(m_Pitch.y, -3.0f, 3.0f);
        }

        public override string GetErrorMessage(EASBaseController controller)
        {
            string errorMessage = string.Empty;

            if (m_AudioClips != null)
            {
                for (int i = 0; i < m_AudioClips.Length; ++i)
                {
                    if (m_AudioClips[i].Resolve(controller) == null)
                    {
                        errorMessage += (string.IsNullOrEmpty(errorMessage) ? "" : " and ") + $"AudioClip at index {i} is not set"; 
                    }
                }
            }

            return errorMessage;
        }

        public override void OnStartEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            AudioClip previewAudioClip = GetRandomAudioClip(editorBridge.Controller);
            if (previewAudioClip != null)
            {
                Initialize();

                PreviewSound(previewAudioClip);
            }
        }
#endif // UNITY_EDITOR
    }
}
