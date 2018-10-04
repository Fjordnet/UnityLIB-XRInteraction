using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.XRInput
{
    /// <summary>
    /// Encapsulates a list of XRInputNames, primarily to create a PropertyDrawer for this type.
    /// You cannot create a PropertyDrawer for a List XRInputName type directly.
    /// </summary>
    [System.Serializable]
    public class XRInputNameList
    {
        [SerializeField]
        private List<XRInputName> _inputNames = new List<XRInputName>();

        public int Count
        {
            get { return _inputNames.Count; }
        }
        
        public XRInputName this[int i]
        {
            get { return _inputNames[i]; }
        }
        
        public void Add(XRInputName inputName)
        {
            if (!_inputNames.Contains(inputName))
            {
                _inputNames.Add(inputName);
            }
        }

        public bool Contains(XRInputName inputName)
        {
            return _inputNames.Contains(inputName);
        }
    }
}