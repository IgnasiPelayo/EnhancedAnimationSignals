
using System;

namespace EAS
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class EASCustomPropertyInspectorDrawerAttribute : Attribute
    {
        public Type Type { get; }

        public EASCustomPropertyInspectorDrawerAttribute(Type type)
        {
            Type = type;
        }
    }
}


