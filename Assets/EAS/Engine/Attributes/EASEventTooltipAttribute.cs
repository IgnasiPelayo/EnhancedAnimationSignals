using System;

namespace EAS
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EASEventTooltipAttribute : Attribute
    {
        public EASEventTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }

        public string Tooltip { get; }
    }
}
