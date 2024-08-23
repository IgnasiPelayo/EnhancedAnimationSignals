using UnityEngine;

namespace EAS
{
    public interface IEASEditorBridge
    {
        public EASBaseController Controller { get; }

        public void SetTimeScale(float timeScale);
        public void CancelTimeScale(float timeScale);

        public void AddSecondaryPreviewAnimation(AnimationClip animationClip, int startFrame, GameObject gameObject);
    }
}
