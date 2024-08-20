using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace EAS
{
    public class EASEditorUtils : EASUtils
    {
        public static T GetAttribute<T>(System.Type type) where T : System.Attribute
        {
            MemberInfo memberInfo = type;
            object[] allAttributes = memberInfo.GetCustomAttributes(inherit: true);

            for (int i = 0; i < allAttributes.Length; ++i)
            {
                if (allAttributes[i] is T)
                {
                    return allAttributes[i] as T;
                }
            }

            return null;
        }

        public static T GetAttribute<T>(object[] attributes) where T : System.Attribute
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; ++i)
                {
                    if (attributes[i] is T)
                    {
                        return attributes[i] as T;
                    }
                }
            }

            return null;
        }

        public static string GetEASEventCategoryAttribute(System.Type type)
        {
            EASEventCategoryAttribute attribute = GetAttribute<EASEventCategoryAttribute>(type);
            if (attribute != null)
            {
                return attribute.Category.EndsWith('/') ? attribute.Category : attribute.Category + "/";
            }

            return string.Empty;
        }

        public static Color GetEASEventColorAttribute(System.Type type)
        {
            EASEventColorAttribute attribute = GetAttribute<EASEventColorAttribute>(type);
            if (attribute != null)
            {
                return attribute.Color;
            }

            return HexToColor("#C0C0C0");
        }

        public static string GetEASEventTooltipAttribute(System.Type type)
        {
            EASEventTooltipAttribute attribute = GetAttribute<EASEventTooltipAttribute>(type);
            if (attribute != null)
            {
                return attribute.Tooltip;
            }

            return string.Empty;
        }

        public static System.Type GetEASCustomEventDrawerAttribute(System.Type type)
        {
            EASCustomEventDrawerAttribute attribute = GetAttribute<EASCustomEventDrawerAttribute>(type);
            if (attribute != null)
            {
                return attribute.Type;
            }

            return null;
        }

        public static Dictionary<System.Type, EASEventDrawer> GetAllEASCustomEventDrawers()
        {
            Dictionary<System.Type, EASEventDrawer> eventDrawers = new Dictionary<System.Type, EASEventDrawer>();

            List<System.Type> allEventDrawers = GetAllDerivedTypesOf<EASEventDrawer>();
            for (int i = 0; i < allEventDrawers.Count; ++i)
            {
                System.Type eventTypeOfEventDrawer = GetEASCustomEventDrawerAttribute(allEventDrawers[i]);
                if (!eventDrawers.ContainsKey(eventTypeOfEventDrawer))
                {
                    eventDrawers.Add(eventTypeOfEventDrawer, System.Runtime.Serialization.FormatterServices.GetUninitializedObject(allEventDrawers[i]) as EASEventDrawer);
                }
            }

            return eventDrawers;
        }

        public static System.Type GetEASCustomEventInspectorDrawerAttribute(System.Type type)
        {
            EASCustomEventInspectorDrawerAttribute attribute = GetAttribute<EASCustomEventInspectorDrawerAttribute>(type);
            if (attribute != null)
            {
                return attribute.Type;
            }

            return null;
        }

        public static System.Type GetEASCustomPropertyInspectorDrawerAttribute(System.Type type)
        {
            EASCustomPropertyInspectorDrawerAttribute attribute = GetAttribute<EASCustomPropertyInspectorDrawerAttribute>(type);
            if (attribute != null)
            {
                return attribute.Type;
            }

            return null;
        }

        public static Dictionary<System.Type, EASPropertyInspectorDrawer> GetAllEASCustomPropertyInspectorDrawers()
        {
            Dictionary<System.Type, EASPropertyInspectorDrawer> propertyInspectorDrawers = new Dictionary<System.Type, EASPropertyInspectorDrawer>();

            List<System.Type> allPropertyInspectorDrawers = GetAllDerivedTypesOf<EASPropertyInspectorDrawer>();
            for (int i = 0; i < allPropertyInspectorDrawers.Count; ++i)
            {
                System.Type fieldTypeOfPropertyInspectorDrawer = GetEASCustomPropertyInspectorDrawerAttribute(allPropertyInspectorDrawers[i]);
                if (fieldTypeOfPropertyInspectorDrawer != null && !propertyInspectorDrawers.ContainsKey(fieldTypeOfPropertyInspectorDrawer))
                {
                    propertyInspectorDrawers.Add(fieldTypeOfPropertyInspectorDrawer, System.Runtime.Serialization.FormatterServices.GetUninitializedObject(allPropertyInspectorDrawers[i]) as EASPropertyInspectorDrawer);
                }
            }

            return propertyInspectorDrawers;
        }

        public static string GetReadableEventNameWithCategory(System.Type type)
        {
            return FromCamelCase(GetEASEventCategoryAttribute(type)) + GetReadableEventName(type);
        }

        public static List<System.Type> GetAllDerivedTypesOf<T>()
        {
            return (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
                    from assemblyType in domainAssembly.GetTypes()
                    where typeof(T).IsAssignableFrom(assemblyType) && !assemblyType.IsAbstract
                    select assemblyType).ToList();
        }

        public static List<System.Type> GetValidEventsForTrack(GameObject root, EASBaseController owner)
        {
            List<System.Type> validEvents = new List<System.Type>();

            List<System.Type> allEvents = GetAllDerivedTypesOf<EASBaseEvent>();
            for (int i = 0; i < allEvents.Count; ++i)
            {
                EASBaseEvent baseEvent = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(allEvents[i]) as EASBaseEvent;
                if (baseEvent.IsObjectCompatible(root) && baseEvent.HasOwnerType(owner))
                {
                    validEvents.Add(allEvents[i]);
                }
            }

            return validEvents.OrderBy(e => e.Name).ToList();
        }

        public static int GetSerializableID(EASSerializable serializable)
        {
            return (serializable as EASID).ID;
        }

        public static bool ValidateTrackGroupEventPositions(EASTrackGroup trackGroup, int trackLength)
        {
            for (int i = 0; i < trackGroup.Tracks.Count; ++i)
            {
                if (!ValidateTrackEventPositions(trackGroup.Tracks[i], trackLength))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateTrackEventPositions(EASTrack track, int trackLength)
        {
            bool[] occupiedFrames = new bool[trackLength - 1];
            for (int i = 0; i < track.Events.Count; ++i)
            {
                EASBaseEvent baseEvent = track.Events[i] as EASBaseEvent;
                for (int j = 0; j < baseEvent.Duration; ++j)
                {
                    if (occupiedFrames[baseEvent.StartFrame + j])
                    {
                        return false;
                    }
                    occupiedFrames[baseEvent.StartFrame + j] = true;
                }
            }

            return true;
        }

        public static bool HasSpaceForNewEvents(EASTrack track, int trackLength)
        {
            int occupiedFramesCount = 0;
            for (int i = 0; i < track.Events.Count; ++i)
            {
                EASBaseEvent baseEvent = track.Events[i] as EASBaseEvent;
                occupiedFramesCount += baseEvent.Duration;
            }

            return (trackLength - 1) > occupiedFramesCount;
        }

        public static bool SetStartFrameAndDurationForNewEvent(EASBaseEvent baseEvent, int trackLength)
        {
            List<Vector2Int> freeEventSpaces = GetFreeEventSpaces(baseEvent.ParentTrack, trackLength);
            if (freeEventSpaces.Count > 0)
            {
                baseEvent.StartFrame = freeEventSpaces[0].x;
                baseEvent.Duration = Mathf.Min(freeEventSpaces[0].y - freeEventSpaces[0].x, baseEvent.DefaultDuration);

                return true;
            }

            return false;
        }

        public static bool SetStartFrameAndDurationForNewEvent(EASBaseEvent baseEvent, int frameAtMousePosition, int trackLength)
        {
            List<Vector2Int> freeEventSpaces = GetFreeEventSpaces(baseEvent.ParentTrack, trackLength);
            for (int i = 0; i < freeEventSpaces.Count; ++i)
            {
                if (freeEventSpaces[i].x <= frameAtMousePosition && freeEventSpaces[i].y >= frameAtMousePosition)
                {
                    baseEvent.StartFrame = frameAtMousePosition;
                    baseEvent.Duration = Mathf.Min(freeEventSpaces[i].y - frameAtMousePosition, baseEvent.DefaultDuration);
                  
                    return true;
                }
            }

            return false;
        }

        protected static List<Vector2Int> GetFreeEventSpaces(EASTrack track, int trackLength)
        {
            List<Vector2Int> freeSpaces = new List<Vector2Int>();
            Vector2Int latestFreeSpace = Vector2Int.zero;

            for (int i = 0; i < track.Events.Count; ++i)
            {
                EASBaseEvent baseEvent = track.Events[i] as EASBaseEvent;
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
    }
}
