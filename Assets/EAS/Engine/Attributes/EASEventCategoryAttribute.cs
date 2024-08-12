using System;

namespace EAS
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EASEventCategoryAttribute : Attribute
    {
        public string Category { get; }

        public EASEventCategoryAttribute(string category)
        {
            Category = category;
        }
    }
}

