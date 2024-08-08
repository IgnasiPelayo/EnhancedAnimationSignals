using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAttributes
{
    public class ArrayElementTitleAttribute : PropertyAttribute
    {
        protected string m_ElementTitleVariableName;
        public string ElementTitleVariableName { get => m_ElementTitleVariableName; }

        public ArrayElementTitleAttribute(string elementTitleVar)
        {
            m_ElementTitleVariableName = elementTitleVar;
        }
    }
}