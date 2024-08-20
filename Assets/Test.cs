using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestClass
{
    public string m_Name;

    [SerializeField]
    protected int m_Health;
}

[System.Flags]
public enum Directions
{
    None = 0,
    North = 1 << 0,
    West = 1 << 1,
    South = 1 << 2,
    East = 1 << 3
}

public class Test : MonoBehaviour
{
    public AnimationCurve m_AnimationCurve;

    public string[] m_Array;

    public List<string> m_List;

    public bool m_Bool;

    public TestClass m_Class;

    public Color m_Color;

    public double m_Double;

    public Directions m_Enum;

    public float m_Float;

    public int m_Int;

    public Transform m_Object;

    [TextArea]
    public string m_String;

    public Vector2 m_Vector2;

    public Vector3 m_Vector3;

    public Vector4 m_Vector4;
}
