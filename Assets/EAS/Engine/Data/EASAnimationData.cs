using UnityEngine;
using CustomAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;

namespace EAS
{
    [System.Serializable]
    public class EASAnimationData : EASID
    {
        [SerializeField, HideInInspector]
        protected int m_NameHash;
        public int NameHash { get => m_NameHash; }

        [SerializeReference, ArrayElementTitle("m_Name")]
        protected List<IEASSerializable> m_TracksAndGroups;
        public List<IEASSerializable> TracksAndGroups { get => m_TracksAndGroups; }

        public EASAnimationData(string name)
        {
            m_Name = name;
#if UNITY_EDITOR
            ID = GenerateID();
#endif // UNITY_EDITOR

            m_NameHash = Animator.StringToHash(m_Name);

            m_TracksAndGroups = new List<IEASSerializable>();
        }

        public void AddTrackOrGroup(EASID trackOrGroup)
        {
            m_TracksAndGroups.Add(trackOrGroup);
        }

        public bool RemoveTrackOrGroup(IEASSerializable trackOrGroup)
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

#if UNITY_EDITOR
        public List<EASBaseEvent> GetEvents(bool addMuted)
        {
            List<EASBaseEvent> unmutedEvents = new List<EASBaseEvent>();

            for (int i = 0; i < m_TracksAndGroups.Count; ++i)
            {
                if (m_TracksAndGroups[i] is EASTrackGroup)
                {
                    EASTrackGroup trackGroup = m_TracksAndGroups[i] as EASTrackGroup;
                    if (!trackGroup.Muted || addMuted)
                    {
                        for (int j = 0; j < trackGroup.Tracks.Count; ++j)
                        {
                            EASTrack track = trackGroup.Tracks[j];
                            if (!track.Muted || addMuted)
                            {
                                for (int k = 0; k < track.Events.Count; ++k)
                                {
                                    unmutedEvents.Add(track.Events[k] as EASBaseEvent);
                                }
                            }
                        }
                    }
                }
                else if (m_TracksAndGroups[i] is EASTrack)
                {
                    EASTrack track = m_TracksAndGroups[i] as EASTrack;
                    if (!track.Muted || addMuted)
                    {
                        for (int j = 0; j < track.Events.Count; ++j)
                        {
                            unmutedEvents.Add(track.Events[j] as EASBaseEvent);
                        }
                    }
                }
            }

            return unmutedEvents;
        }
#endif // UNITY_EDITOR
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
        public override string DefaultName { get => "Track"; }

        public bool ParentTrackGroupMuted { get => m_ParentTrackGroup != null ? m_ParentTrackGroup.Muted : false; }
        public bool ParentTrackGroupLocked { get => m_ParentTrackGroup != null ? m_ParentTrackGroup.Locked : false; }

        [SerializeReference, HideInInspector]
        protected EASTrackGroup m_ParentTrackGroup;
        public EASTrackGroup ParentTrackGroup { get => m_ParentTrackGroup; set => m_ParentTrackGroup = value; }

        [SerializeReference, ArrayElementTitle("m_Name")]
        protected List<IEASSerializable> m_Events;
        public List<IEASSerializable> Events { get => m_Events; }

        public EASTrack()
        {
#if UNITY_EDITOR
            if (ID == INVALID_ID)
            {
                ID = GenerateID();
                m_Name = DefaultName;

                m_Events = new List<IEASSerializable>();
            }
#endif // UNITY_EDITOR
        }

        public EASTrack(EASTrackGroup parentTrackGroup) : this()
        {
            m_ParentTrackGroup = parentTrackGroup;
        }

        public EASBaseEvent CreateEvent(System.Type type)
        {
#if UNITY_EDITOR
            EASBaseEvent newEvent = EASBaseEvent.Create(type);
            newEvent.ParentTrack = this;

            return newEvent;
#else // UNITY_EDITOR
            return null;
#endif 
        }

        public EASBaseEvent AddEvent(EASBaseEvent baseEvent)
        {
            if (baseEvent.ParentTrack != this)
            {
                baseEvent.ParentTrack = this;
            }

            m_Events.Add(baseEvent);
            ReorderEvents();

            return baseEvent;
        }

        public void ReorderEvents()
        {
#if UNITY_EDITOR
            m_Events = m_Events.OrderBy(e => (e as EASBaseEvent).StartFrame).ToList();
#endif // UNITY_EDITOR
        }

        public override IEASSerializable GetSerializableFromID(int serializableID)
        {
            if (ID == serializableID)
            {
                return this;
            }    

            foreach (EASBaseEvent baseEvent in Events)
            {
                if (baseEvent.ID == serializableID)
                {
                    return baseEvent;
                }
            }

            return null;
        }
    }

    [System.Serializable]
    public class EASTrackGroup : EASBaseTrack
    {
        public override string DefaultName { get => "Track Group"; }

        [SerializeField, HideInInspector]
        protected bool m_Collapsed;
        public bool Collapsed { get => m_Collapsed; set => m_Collapsed = value; }

        [SerializeField, ArrayElementTitle("m_Name")]
        protected List<EASTrack> m_Tracks;
        public List<EASTrack> Tracks { get => m_Tracks; }

        public EASTrackGroup()
        {
#if UNITY_EDITOR
            if (ID == INVALID_ID)
            {
                ID = GenerateID();
                m_Name = DefaultName;

                m_Tracks = new List<EASTrack>();
            }
#endif // UNITY_EDITOR
        }

        public EASTrack AddTrack()
        {
            EASTrack track = new EASTrack(parentTrackGroup: this);
            m_Tracks.Add(track);

            return track;
        }

        public EASTrack AddTrack(EASTrack track)
        {
            if (track.ParentTrackGroup != null)
            {
                track.ParentTrackGroup.RemoveTrack(track);
            }

            m_Tracks.Add(track);
            track.ParentTrackGroup = this;

            return track;
        }

        public bool RemoveTrack(EASTrack track)
        {
            return m_Tracks.Remove(track);
        }

        public override IEASSerializable GetSerializableFromID(int serializableID)
        {
            if (ID == serializableID)
            {
                return this;
            }

            foreach (EASTrack track in Tracks)
            {
                IEASSerializable serializable = track.GetSerializableFromID(serializableID);
                if (serializable != null)
                {
                    return serializable;
                }
            }

            return null;
        }
    }
}

