using UnityEngine;
using CustomAttributes;
using System.Collections.Generic;

namespace EAS
{
    [CreateAssetMenu(fileName = "EAS Data", menuName = "EAS Data", order = 459)]
    public class EASData : ScriptableObject
    {
        [SerializeField, ArrayElementTitle("m_Name")]
        protected List<EASAnimationData> m_AnimationsData = new List<EASAnimationData>();
        public List<EASAnimationData> AnimationData { get => m_AnimationsData; }

        public EASTrackGroup AddTrackGroup(string animationName)
        {
            EASAnimationData animationData = GetAnimationData(animationName);

            EASTrackGroup trackGroup = new EASTrackGroup();
            animationData.AddTrackOrGroup(trackGroup);

            return trackGroup;
        }

        public EASTrack AddTrack(string animationName)
        {
            EASAnimationData animationData = GetAnimationData(animationName);

            EASTrack track = new EASTrack();
            animationData.AddTrackOrGroup(track);

            return track;
        }

        public bool RemoveTrackOrGroup(string animationName, IEASSerializable trackOrGroup)
        {
            EASAnimationData animationData = GetAnimationData(animationName);
            bool wasRemoved = animationData.RemoveTrackOrGroup(trackOrGroup);

            return wasRemoved;
        }

        public EASAnimationData GetAnimationData(string animationName)
        {
            int animationNameHash = Animator.StringToHash(animationName);
            for (int i = 0; i < m_AnimationsData.Count; ++i)
            {
                if (m_AnimationsData[i].NameHash == animationNameHash)
                {
                    return m_AnimationsData[i];
                }
            }

            EASAnimationData animationData = new EASAnimationData(animationName);
            m_AnimationsData.Add(animationData);

            return animationData;
        }

        public List<IEASSerializable> GetTracksAndGroups(string animationName)
        {
            EASAnimationData animationData = GetAnimationData(animationName);
            if (animationName != null)
            {
                return animationData.TracksAndGroups;
            }

            return new List<IEASSerializable>();
        }

#if UNITY_EDITOR
        public List<EASBaseEvent> GetEvents(string animationName, bool addMuted)
        {
            return GetAnimationData(animationName).GetEvents(addMuted);
        }
#endif // UNITY_EDITOR
    }
}
