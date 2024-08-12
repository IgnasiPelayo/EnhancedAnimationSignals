using System.Collections.Generic;

namespace EAS
{
    public class EASEditorUtils : EASUtils
    {
        public static System.Type GetEASCustomEventDrawerAttribute(System.Type type)
        {
            EASCustomEventDrawerAttribute attribute = GetAttribute<EASCustomEventDrawerAttribute>(type);
            if (attribute != null)
            {
                return attribute.Type;
            }

            return null;
        }

        public static Dictionary<System.Type, EASEventDrawer> GetAllEASCustomEventDrawers()
        {
            Dictionary<System.Type, EASEventDrawer> eventDrawers = new Dictionary<System.Type, EASEventDrawer>();

            List<System.Type> allEventDrawers = GetAllDerivedTypesOf<EASEventDrawer>();
            for (int i = 0; i < allEventDrawers.Count; ++i)
            {
                System.Type eventTypeOfEventDrawer = GetEASCustomEventDrawerAttribute(allEventDrawers[i]);
                if (!eventDrawers.ContainsKey(eventTypeOfEventDrawer))
                {
                    eventDrawers.Add(eventTypeOfEventDrawer, System.Runtime.Serialization.FormatterServices.GetUninitializedObject(allEventDrawers[i]) as EASEventDrawer);
                }
            }

            return eventDrawers;
        }
    }
}
