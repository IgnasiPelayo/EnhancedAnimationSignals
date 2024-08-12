using System;

namespace EAS
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EASCustomEventDrawerAttribute : Attribute
    {
        public Type Type { get; }

        public EASCustomEventDrawerAttribute(Type type)
        {
            Type = type;
        }
    }
}
