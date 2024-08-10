using System;

namespace EAS
{
    [AttributeUsage(AttributeTargets.All)]
    public class EASTooltipAttribute : Attribute
    {
        public EASTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }

        public string Tooltip { get; }
    }
}
