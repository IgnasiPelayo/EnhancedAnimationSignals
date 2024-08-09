using System;

namespace EAS
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EASEventCategoryAttribute : Attribute
    {
        public EASEventCategoryAttribute(string category)
        {
            Category = category;
        }

        public string Category { get; }
    }
}

