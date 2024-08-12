using System;
using UnityEngine;

namespace EAS
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EASEventColorAttribute : Attribute
    {
        public Color Color { get; }

        public EASEventColorAttribute(float r, float g, float b, float a = 1.0f)
        {
            Color = new Color(r, g, b, a);
        }

        public EASEventColorAttribute(int r, int g, int b, int a = 255)
        {
            Color = new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        public EASEventColorAttribute(string hexColor)
        {
            Color = EASUtils.HexToColor(hexColor);
        }
    }
}

