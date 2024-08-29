using UnityEngine;

namespace EAS
{
    public abstract class EASBaseSoundEvent : EASCustomOwnerEvent<AudioSource>
    {
        protected void PlaySound(AudioClip audioClip, float volume, float pitch, bool loop)
        {
            Owner.clip = audioClip;
            Owner.volume = volume;
            Owner.pitch = pitch;
            Owner.loop = loop;

            Owner.Play();
        }

#if UNITY_EDITOR
        protected static System.Reflection.MethodInfo s_PlayPreviewClipMethod;
        protected static System.Reflection.MethodInfo s_StopAllPreviewClipsMethod;

        public override bool CanPreviewInEditor(IEASEditorBridge editorBridge)
        {
            return !editorBridge.Mute;
        }

        public override void OnAnimationEndEditor(IEASEditorBridge editorBridge)
        {
            Initialize();
            StopAllPreviewSounds();
        }

        public override void OnAnimationModified(IEASEditorBridge editorBridge)
        {
            Initialize();
            StopAllPreviewSounds();
        }

        protected void Initialize()
        {
            if (s_PlayPreviewClipMethod == null || s_StopAllPreviewClipsMethod == null)
            {
                System.Reflection.Assembly unityEditorAssembly = typeof(UnityEditor.AudioImporter).Assembly;
                System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

                s_PlayPreviewClipMethod = audioUtilClass.GetMethod("PlayPreviewClip", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                    null, new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);

                s_StopAllPreviewClipsMethod = audioUtilClass.GetMethod("StopAllPreviewClips", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                System.Reflection.MethodInfo[] methods = audioUtilClass.GetMethods();
            }
        }

        protected void PreviewSound(AudioClip audioClip, float elapsedTime = 0)
        {
            int startSample = Mathf.FloorToInt((elapsedTime * audioClip.samples) / audioClip.length);
            s_PlayPreviewClipMethod.Invoke(null, new object[] { audioClip, startSample, false });
        }

        protected void StopAllPreviewSounds()
        {
            s_StopAllPreviewClipsMethod.Invoke(null, null);
        }
#endif // UNITY_EDITOR
    }
}
