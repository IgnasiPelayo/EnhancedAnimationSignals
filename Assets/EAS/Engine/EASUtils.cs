using UnityEngine;
using System.Text.RegularExpressions;

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
            if (value == 0)
            {
                return "00";
            }

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

        public static string ColorToHex(Color color, bool addAlpha = false)
        {
            return IntToHex(Mathf.RoundToInt(color.r * 255)) + IntToHex(Mathf.RoundToInt(color.g * 255)) + IntToHex(Mathf.RoundToInt(color.b * 255)) + (addAlpha ? IntToHex(Mathf.RoundToInt(color.a * 255)) : "");
        }

        public static Color HexToColor(string hexColor)
        {
            const int hex = 16;

            hexColor = hexColor.Replace("#", "");
            if (hexColor.Length == 8)
            {
                return HexToColorWithAlpha(hexColor);
            }

            return new Color((HexToInt(hexColor[0]) * hex + HexToInt(hexColor[1])) / 255.0f,
                (HexToInt(hexColor[2]) * hex + HexToInt(hexColor[3])) / 255.0f,
                (HexToInt(hexColor[4]) * hex + HexToInt(hexColor[5])) / 255.0f);
        }

        public static Color HexToColorWithAlpha(string hexColor)
        {
            const int hex = 16;

            return new Color((HexToInt(hexColor[0]) * hex + HexToInt(hexColor[1])) / 255.0f,
                (HexToInt(hexColor[2]) * hex + HexToInt(hexColor[3])) / 255.0f,
                (HexToInt(hexColor[4]) * hex + HexToInt(hexColor[5])) / 255.0f,
                (HexToInt(hexColor[6]) * hex + HexToInt(hexColor[7])) / 255.0f);
        }

        public static string GetReadableEventName(System.Type type, bool replaceEvent = true)
        {
            string eventName = type.Name.Replace("EAS", "");
            if (replaceEvent)
            {
                eventName = eventName.Replace("Event", "");
            }

            return FromCamelCase(eventName);
        }

        public static string FromCamelCase(string camelCase)
        {
            return Regex.Replace(camelCase, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
        }
    }
}
