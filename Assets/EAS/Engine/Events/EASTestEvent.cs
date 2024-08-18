using System.Collections.Generic;
using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFFFFF"), EASEventTooltip("This is a test event to show default property inspectors")]
    public class EASTestEvent : EASEvent
    {
        [SerializeField]
        protected bool[] m_Bools;

        [SerializeField]
        protected Vector4[] m_VectorsArray;

        //public List<bool> m_ListBool = new List<bool>();

        //[SerializeField]
        //protected string m_StringValue;

        [SerializeField]
        protected int m_IntValue;

        //[SerializeField]
        //protected bool m_BoolValue;

        //[SerializeField, Min(2.0f)]
        //protected float m_FloatValue;

        //public double DoubleValue;

        //[SerializeField, Header("Color")]
        //public Color m_ColorValue;

        ////[SerializeField]
        //public Vector2 m_Vector2Value;
        //public Vector3 m_Vector3Value;
        //public Vector4 m_Vector4Value;
        public Vector4 m_Vector4Value2;

        //public Transform m_Transform;

        //public GameObject m_GameObject;
        //public Animator animator;

        //[Space(10)]
        //public AnimationCurve animationCurve;

#if UNITY_EDITOR
        public override string GetErrorMessage()
        {
            return string.Empty;
        }

        public override bool HasError(EASBaseController owner)
        {
            return false;
        }
#endif // UNITY_EDITOR
    }
}