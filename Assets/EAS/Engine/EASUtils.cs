using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EAS
{
    public class EASUtils
    {
        protected static int HexToInt(char hexChar)
        {
            string hexString = "" + hexChar;
            if (int.TryParse(hexString, out int hexValue))
            {
                return hexValue;
            }

            hexValue = hexString.ToUpper()[0] - 'A' + 10;
            return hexValue;
        }

        protected static string IntToHex(int value)
        {
            string hexValue = string.Empty;
            while (value > 0)
            {
                int remainder = value % 16;
                if (remainder < 10)
                {
                    hexValue = remainder + hexValue;
                }
                else
                {
                    hexValue = (char)(remainder - 10 + 'A') + hexValue;
                }

                value /= 16;
            }

            return hexValue;
        }

        public static string ColorToHex(Color color)
        {
            return IntToHex(Mathf.RoundToInt(color.r * 255)) + IntToHex(Mathf.RoundToInt(color.g * 255)) + IntToHex(Mathf.RoundToInt(color.b * 255));
        }

        public static Color HexToColor(string hexColor)
        {
            const int hex = 16;

            hexColor = hexColor.Replace("#", "");
            return new Color((HexToInt(hexColor[0]) * hex + HexToInt(hexColor[1])) / 255.0f, 
                (HexToInt(hexColor[2]) * hex + HexToInt(hexColor[3])) / 255.0f,
                (HexToInt(hexColor[4]) * hex + HexToInt(hexColor[5])) / 255.0f);
        }

#if UNITY_EDITOR
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

        public static Color GetEASEventColorAttribute(System.Type type)
        {
            EASEventColorAttribute attribute = GetAttribute<EASEventColorAttribute>(type);
            if (attribute != null)
            {
                return attribute.Color;
            }

            return HexToColor("#C0C0C0");
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
#endif // UNITY_EDITOR
    }
}
