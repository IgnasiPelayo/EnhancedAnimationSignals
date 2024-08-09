using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

namespace EAS
{
    [CreateAssetMenu(fileName = "EAS Data", menuName = "EAS Data", order = 459)]
    public class EASData : ScriptableObject
    {
        [SerializeField, ReadOnly]
        protected long m_TimeStamp = DateTime.MinValue.Ticks;
        public long TimeStamp { get => m_TimeStamp; set => m_TimeStamp = value; }

        [SerializeField, ArrayElementTitle("m_Name")]
        protected List<EASAnimationData> m_AnimationsData = new List<EASAnimationData>();

        public EASTrackGroup AddTrackGroup(string animationName)
        {
            EASAnimationData animationData = GetAnimationData(animationName);

            EASTrackGroup trackGroup = new EASTrackGroup();
            animationData.AddTrackOrGroup(trackGroup);

            m_TimeStamp = DateTime.Now.Ticks;

            return trackGroup;
        }

        public EASTrack AddTrack(string animationName)
        {
            EASAnimationData animationData = GetAnimationData(animationName);

            EASTrack track = new EASTrack();
            animationData.AddTrackOrGroup(track);

            m_TimeStamp = DateTime.Now.Ticks;

            return track;
        }

        public bool RemoveTrackOrGroup(string animationName, EASSerializable trackOrGroup)
        {
            EASAnimationData animationData = GetAnimationData(animationName);
            bool wasRemoved = animationData.RemoveTrackOrGroup(trackOrGroup);

            m_TimeStamp = DateTime.Now.Ticks;

            return wasRemoved;
        }

        protected EASAnimationData GetAnimationData(string animationName)
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

            m_TimeStamp = DateTime.Now.Ticks;

            return animationData;
        }

        public List<EASSerializable> GetTracksAndGroups(string animationName)
        {
            EASAnimationData animationData = GetAnimationData(animationName);
            if (animationName != null)
            {
                return animationData.TracksAndGroups;
            }

            return new List<EASSerializable>();
        }
    }
}
