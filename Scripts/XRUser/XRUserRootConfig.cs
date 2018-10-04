using System;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRHaptics;
using Fjord.XRInteraction.XRInput;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fjord.XRInteraction.XRUser
{
    [CreateAssetMenu(fileName = "XRUserRootConfig", menuName = "XR User Root Config", order = 100)]
    [Serializable]
    public class XRUserRootConfig : ScriptableObject
    {
        [SerializeField]
        private string _deviceName;
        
        [SerializeField]
        private XRUserController _leftUserControllerPrefab;
        
        [SerializeField]
        private XRControllerModel _leftControllerModelPrefab;
        
        [SerializeField]
        private XRInputMap _leftControllerInputMap;

        [SerializeField]
        private XRUserController _rightUserControllerPrefab;
        
        [SerializeField]
        private XRControllerModel _rightControllerModelPrefab;
        
        [SerializeField]
        private XRInputMap _rightControllerInputMap;
        
        [SerializeField]
        private XRHapticEngine _hapticEngine;
        
        [SerializeField]
        private XRHapticMap _hapticMap;

        public string DeviceName
        {
            get { return _deviceName; }
        }

        public XRHapticEngine HapticEngine
        {
            get { return _hapticEngine; }
        }

        public XRHapticMap HapticMap
        {
            get { return _hapticMap; }
        }

        public XRUserController UserControllerPrefab(Chirality chirality)
        {
            if (chirality == Chirality.Left)
            {
                return _leftUserControllerPrefab;
            }
            else if (chirality == Chirality.Right)
            {
                return _rightUserControllerPrefab;
            }
            return null;
        }
        
        public XRControllerModel ControllerModelPrefab(Chirality chirality)
        {
            if (chirality == Chirality.Left)
            {
                return _leftControllerModelPrefab;
            }
            else if (chirality == Chirality.Right)
            {
                return _rightControllerModelPrefab;
            }
            return null;
        }
        
        public XRInputMap InputMapping(Chirality chirality)
        {
            if (chirality == Chirality.Left)
            {
                return _leftControllerInputMap;
            }
            else if (chirality == Chirality.Right)
            {
                return _rightControllerInputMap;
            }
            return null;
        }
    }        
}