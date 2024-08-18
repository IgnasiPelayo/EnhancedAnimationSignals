using System;

namespace EAS
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EASCustomEventInspectorDrawerAttribute : Attribute
    {
        public Type Type { get; }

        public EASCustomEventInspectorDrawerAttribute(Type type)
        {
            Type = type;
        }
    }
}