using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EAS
{
    [System.Serializable]
    public class EASAnimationInformation
    {
        [SerializeField]
        protected object m_Animation;
        public object Animation { get => m_Animation; }

        [SerializeField]
        protected string m_Name;
        public string Name { get => m_Name; }

        [SerializeField]
        protected float m_Length;
        public float Length { get => m_Length; }

        [SerializeField]
        protected float m_FrameRate;
        public float FrameRate { get => m_FrameRate; }

        public float Frames { get => Mathf.Clamp(m_Length * m_FrameRate, 1.0f, float.MaxValue); }

        [SerializeField]
        protected List<int> m_KeyFrames;
        public List<int> KeyFrames { get => m_KeyFrames; }

        public EASAnimationInformation(object animation, string name, float length, float frameRate, List<int> keyFrames)
        {
            m_Animation = animation;

            m_Name = name;

            m_Length = length;
            m_FrameRate = frameRate;

            m_KeyFrames = new List<int>(keyFrames);
        }
    }

    [System.Serializable]
    public class EASAdditionalAnimationInformation : EASAnimationInformation
    {
        public AnimationClip AnimationClip { get => m_Animation as AnimationClip; }

        [SerializeField]
        protected float m_Time;
        public float Time { get => m_Time; set => m_Time = value; }

        [SerializeField]
        protected int m_StartFrame;
        public int StartFrame { get => m_StartFrame; }

        [SerializeField]
        public GameObject m_GameObject;
        public GameObject GameObject { get => m_GameObject; }

        public EASAdditionalAnimationInformation(object animation, string name, float length, float frameRate, int startFrame, GameObject gameObject)
            : base(animation, name, length, frameRate, new List<int>())
        {
            m_Time = 0.0f;
            m_StartFrame = startFrame;
            m_GameObject = gameObject;
        }
    }

    [System.Serializable]
    public class EASAnimationState
    {
        [SerializeField]
        protected int m_AnimationHash;
        public int AnimationHash { get => m_AnimationHash; }

        [SerializeField]
        protected float m_NormalizedTime;
        public float NormalizedTime { get => m_NormalizedTime; }

        [SerializeField]
        protected bool m_IsLoop;
        public bool IsLoop { get => m_IsLoop; }

        public EASAnimationState(int animationHash, float normalizedTime, bool isLoop)
        {
            m_AnimationHash = animationHash;
            m_NormalizedTime = normalizedTime;
            m_IsLoop = isLoop;
        }
    }

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

        public static bool IsFrameInsideEvent(EASBaseEvent baseEvent, float currentFrame)
        {
            return baseEvent.StartFrame <= currentFrame && baseEvent.LastFrame > currentFrame;
        }
    }
}
