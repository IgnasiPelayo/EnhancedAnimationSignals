using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TestClass
{
    [SerializeField]
    public int m_Health;

    [SerializeField]
    public string m_Name;

    public List<float> m_Stats;
}

public class Test : MonoBehaviour
{
    public AnimationCurve curve;

    public List<List<bool>> boolList;

    public TestClass m_TestClass;
}
