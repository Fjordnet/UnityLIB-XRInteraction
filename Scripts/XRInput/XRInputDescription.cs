using System;
using Fjord.Common.Types;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fjord.XRInteraction.XRInput
{
    /// <summary>
    /// Describes how to route the Unity Input API to an XR Input abstraction.
    /// </summary>
    [Serializable]
    public class XRInputDescription
    {        
        [SerializeField]
        private XRInputName _inputName;

        [SerializeField]
        private XRInputType _inputType;

        [SerializeField]
        private Chirality _chirality;

        [SerializeField]
        private KeyCode _buttonKeycode;

        [SerializeField]
        private XRAxisName _axis0Name;
        
        [SerializeField]
        private XRAxisName _axis1Name;

        [SerializeField]
        private float _downThreshold = .85f;

        [SerializeField]
        private float _upThreshold = .8f;

        public XRInputType InputType
        {
            get { return _inputType; }
        }

        public Chirality Chirality
        {
            get { return _chirality; }
        }

        public KeyCode ButtonKeycode
        {
            get { return _buttonKeycode; }
        }

        public float DownThreshold
        {
            get { return _downThreshold; }
        }

        public float UpThreshold
        {
            get { return _upThreshold; }
        }

        public XRInputName InputName
        {
            get { return _inputName; }
        }

        public XRAxisName Axis0Name
        {
            get { return _axis0Name; }
        }

        public XRAxisName Axis1Name
        {
            get { return _axis1Name; }
        }
    }
}