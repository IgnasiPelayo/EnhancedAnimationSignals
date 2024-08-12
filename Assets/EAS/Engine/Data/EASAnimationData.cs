using UnityEngine;
using CustomAttributes;
using System.Collections.Generic;
using System.Linq;

namespace EAS
{
    [System.Serializable]
    public class EASAnimationData : EASID
    {
        [SerializeField, HideInInspector]
        protected int m_NameHash;
        public int NameHash { get => m_NameHash; }

        [SerializeReference, ArrayElementTitle("m_Name")]
        protected List<EASSerializable> m_TracksAndGroups;
        public List<EASSerializable> TracksAndGroups { get => m_TracksAndGroups; }

        public EASAnimationData(string name)
        {
            m_Name = name;
#if UNITY_EDITOR
            ID = GenerateID();
#endif // UNITY_EDITOR

            m_NameHash = Animator.StringToHash(m_Name);

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
        public override string DefaultName { get => "Track"; }

        public bool ParentTrackGroupMuted { get => m_ParentTrackGroup != null ? m_ParentTrackGroup.Muted : false; }
        public bool ParentTrackGroupLocked { get => m_ParentTrackGroup != null ? m_ParentTrackGroup.Locked : false; }

        [SerializeReference, HideInInspector]
        protected EASTrackGroup m_ParentTrackGroup;
        public EASTrackGroup ParentTrackGroup { get => m_ParentTrackGroup; set => m_ParentTrackGroup = value; }

        [SerializeReference, ArrayElementTitle("m_Name")]
        protected List<EASSerializable> m_Events;
        public List<EASSerializable> Events { get => m_Events; }

        public EASTrack()
        {
#if UNITY_EDITOR
            if (ID == INVALID_ID)
            {
                ID = GenerateID();
                m_Name = DefaultName;

                m_Events = new List<EASSerializable>();
            }
#endif // UNITY_EDITOR
        }

        public EASTrack(EASTrackGroup parentTrackGroup) : this()
        {
            m_ParentTrackGroup = parentTrackGroup;
        }

        protected EASBaseEvent Internal_AddEvent(System.Type type, int frame, int trackLength)
        {
            EASBaseEvent newEvent = EASBaseEvent.Create(type);
            newEvent.ParentTrack = this;

#if UNITY_EDITOR
            if (HasSpaceForNewEvents(trackLength))
            {
                List<Vector2Int> freeSpaces = GetFreeSpaces(trackLength);
                if (frame == -1)
                {
                    newEvent.StartFrame = freeSpaces[0].x;
                    newEvent.Duration = Mathf.Min(freeSpaces[0].y - freeSpaces[0].x, newEvent.DefaultDuration);
                }
                else
                {
                    for (int i = 0; i < freeSpaces.Count; ++i)
                    {
                        if (freeSpaces[i].x <= frame && freeSpaces[i].y >= frame)
                        {
                            newEvent.StartFrame = frame;
                            newEvent.Duration = Mathf.Min(freeSpaces[i].y - frame, newEvent.DefaultDuration);
                            break;
                        }
                    }
                }
            }
            else
            {
                return null;
            }
#endif // UNITY_EDITOR

            m_Events.Add(newEvent);
            ReorderEvents();

            return newEvent;
        }

        public EASBaseEvent AddEvent(System.Type type, int desiredFrame, int trackLength)
        {
            return Internal_AddEvent(type, desiredFrame, trackLength);
        }
        
        public EASBaseEvent AddEvent(System.Type type, int trackLength)
        {
            return Internal_AddEvent(type, -1, trackLength);
        }

        public EASBaseEvent AddEvent(EASBaseEvent baseEvent)
        {
            baseEvent.ParentTrack = this;
            m_Events.Add(baseEvent);

            return baseEvent;
        }

        public void ReorderEvents()
        {
#if UNITY_EDITOR
            m_Events = m_Events.OrderBy(e => (e as EASBaseEvent).StartFrame).ToList();
#endif // UNITY_EDITOR
        }

#if UNITY_EDITOR
        public bool HasSpaceForNewEvents(int trackLength)
        {
            int occupiedFrames = 0;
            for (int i = 0; i < m_Events.Count; ++i)
            {
                EASBaseEvent baseEvent = m_Events[i] as EASBaseEvent;
                occupiedFrames += baseEvent.Duration;
            }

            return (trackLength - 1) > occupiedFrames;
        }

        public List<Vector2Int> GetFreeSpaces(int trackLength)
        {
            List<Vector2Int> freeSpaces = new List<Vector2Int>();
            Vector2Int latestFreeSpace = Vector2Int.zero;

            for (int i = 0; i < m_Events.Count; ++i)
            {
                EASBaseEvent baseEvent = m_Events[i] as EASBaseEvent;
                if (baseEvent.StartFrame == latestFreeSpace.y)
                {
                    latestFreeSpace = Vector2Int.one * baseEvent.LastFrame;
                }
                else if (baseEvent.StartFrame > latestFreeSpace.y)
                {
                    latestFreeSpace.y = baseEvent.StartFrame;
                    freeSpaces.Add(latestFreeSpace);

                    latestFreeSpace = Vector2Int.one * baseEvent.LastFrame;
                }
            }

            if (latestFreeSpace.y < trackLength - 1)
            {
                latestFreeSpace.y = trackLength - 1;
                freeSpaces.Add(latestFreeSpace);
            }

            return freeSpaces;
        }
#endif // UNITY_EDITOR
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
    }
}

