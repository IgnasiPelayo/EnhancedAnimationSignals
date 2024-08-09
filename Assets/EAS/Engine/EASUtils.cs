using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EAS
{
#if UNITY_EDITOR
    public class EASUtils
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

        public static string GetEASEventCategoryAttribute(System.Type type)
        {
            EASEventCategoryAttribute attribute = GetAttribute<EASEventCategoryAttribute>(type);
            if (attribute != null)
            {
                return attribute.Category.EndsWith('/') ? attribute.Category : attribute.Category + "/";
            }

            return string.Empty;
        }

        public static string GetReadableEventName(System.Type type, bool addExtension = true)
        {
            string eventName = type.Name.Replace("EAS", "").Replace("Event", "");
            eventName = FromCamelCase(eventName);

            if (addExtension)
            {
                return FromCamelCase(GetEASEventCategoryAttribute(type)) + eventName;
            }

            return eventName;
        }

        public static string FromCamelCase(string camelCase)
        {
            return Regex.Replace(camelCase, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
        }

        public static List<System.Type> GetAllDerivedTypesOf<T>()
        {
            return (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies() from assemblyType in domainAssembly.GetTypes()
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

    }
#endif // UNITY_EDITOR
}
