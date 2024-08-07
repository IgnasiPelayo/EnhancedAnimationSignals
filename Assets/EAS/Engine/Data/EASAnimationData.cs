using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    public class EASAnimationData : EASID
    {
        [SerializeField]
        protected string m_Name = "Undefined";
        public string Name { get => m_Name; set => m_Name = value; }

        public List<EASID> m_TracksAndGroups;
    }

    [System.Serializable]
    public class EASTrack : EASID
    {
        [SerializeField]
        protected List<EASBaseEvent> m_Events;
    }

    [System.Serializable]
    public class EASTrackGroup : EASID
    {
        [SerializeField]
        protected List<EASTrack> m_Tracks;
    }
}

