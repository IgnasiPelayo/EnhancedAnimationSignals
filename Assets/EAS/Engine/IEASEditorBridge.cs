using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    public interface IEASEditorBridge
    {
        public EASBaseController Controller { get; }

        public bool Mute { get; set; }
        public bool ShowParticleSystems { get; set; }
        public float FrameRate { get; }

        public void SetTimeScale(float timeScale);
        public void CancelTimeScale(float timeScale);

        public void AddSecondaryPreviewAnimation(AnimationClip animationClip, int startFrame, GameObject gameObject);

        public List<T> GetEvents<T>(bool addMuted) where T : EASBaseEvent;

        public void BlockPreviewObject(Object previewObject, IEASSerializable serializable);
        public void FreePreviewObject(Object previewObject, IEASSerializable serializable);
        public List<IEASSerializable> GetPreviewObjectConflicts(Object previewObject);
    }
}
