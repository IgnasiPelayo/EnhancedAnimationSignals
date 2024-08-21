using UnityEngine;
using System.Collections.Generic;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFFFFF"), EASEventTooltip("This is a test event to show default property inspectors")]
    public class EASTestEvent : EASEvent
    {
        public AnimationCurve m_AnimationCurve;

        public string[] m_Array;

        public List<string> m_List;

        public bool m_Bool;

        public TestClass m_Class;

        public Color m_Color;

        public double m_Double;

        //public Directions m_Enum;

        public float m_Float;

        public int m_Int;

        public Transform m_Object;

        [TextArea]
        public string m_String;

        public Vector2 m_Vector2;

        public Vector3 m_Vector3;

        public Vector4 m_Vector4;

#if UNITY_EDITOR
        public override string GetErrorMessage()
        {
            return "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";
        }

        public override bool HasError(EASBaseController owner)
        {
            return true;
        }
#endif // UNITY_EDITOR
    }
}