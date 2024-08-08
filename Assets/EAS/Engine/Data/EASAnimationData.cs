using UnityEngine;
using CustomAttributes;
using System.Collections;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    public class EASAnimationData : EASID
    {
        [SerializeReference, ArrayElementTitle("m_Name")]
        protected List<EASSerializable> m_TracksAndGroups;
        public List<EASSerializable> TracksAndGroups { get => m_TracksAndGroups; }

        public EASAnimationData(string name)
        {
            m_Name = name;
            ID = GenerateID();

            m_TracksAndGroups = new List<EASSerializable>();
        }

        public void AddTrackOrGroup(EASID trackOrGroup)
        {
            m_TracksAndGroups.Add(trackOrGroup);
        }

        public bool RemoveTrackOrGroup(EASSerializable trackOrGroup)
        {
            if (!m_TracksAndGroups.Remove(trackOrGroup) && trackOrGroup is EASTrack)
            {
                EASTrack track = trackOrGroup as EASTrack;
                for (int i = 0; i < m_TracksAndGroups.Count; ++i)
                {
                    EASTrackGroup trackGroup = m_TracksAndGroups[i] as EASTrackGroup;
                    if (trackGroup != null)
                    {
                        if (trackGroup.RemoveTrack(track))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return true;
        }
    }

    [System.Serializable]
    public class EASBaseTrack : EASID
    {
        [SerializeField, HideInInspector]
        protected bool m_Muted;
        public bool Muted { get => m_Muted; set => m_Muted = value; }

        [SerializeField, HideInInspector]
        protected bool m_Locked;
        public bool Locked { get => m_Locked; set => m_Locked = value; }
    }

    [System.Serializable]
    public class EASTrack : EASBaseTrack
    {
        [SerializeReference, ArrayElementTitle("m_Name")]
        protected List<EASSerializable> m_Events;

        public EASTrack()
        {
            if (ID == INVALID_ID)
            {
                ID = GenerateID();
                m_Name = "Track";

                m_Events = new List<EASSerializable>();
            }
        }
    }

    [System.Serializable]
    public class EASTrackGroup : EASBaseTrack
    {
        [SerializeField, HideInInspector]
        protected bool m_Collapsed;
        public bool Collapsed { get => m_Collapsed; set => m_Collapsed = value; }

        [SerializeField, ArrayElementTitle("m_Name")]
        protected List<EASTrack> m_Tracks;
        public List<EASTrack> Tracks { get => m_Tracks; }

        public EASTrackGroup()
        {
            if (ID == INVALID_ID)
            {
                ID = GenerateID();
                m_Name = "Track Group";

                m_Tracks = new List<EASTrack>();
            }
        }

        public EASTrack AddTrack()
        {
            EASTrack track = new EASTrack();
            m_Tracks.Add(track);

            return track;
        }

        public bool RemoveTrack(EASTrack track)
        {
            return m_Tracks.Remove(track);
        }
    }
}

