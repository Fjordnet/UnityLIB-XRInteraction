using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fjord.Common.Data;
using Fjord.Common.Extensions;
using Fjord.Common.Tweens;
using Fjord.Common.Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Valve.VR;

namespace Fjord.XRInteraction.XRUser
{
    /// <summary>
    /// Main entry point to all things related to XR and the User.
    /// This component will construct the entire XRUser on Awake. Go through this
    /// component to get any body part, to subscribe to any input, or other.
    /// </summary>
    [RequireComponent(typeof(TransformTweener))]
    public class XRUserRoot : MonoBehaviour
    {
        [SerializeField]
        private List<XRUserRootConfig> _configs = new List<XRUserRootConfig>();

        [SerializeField]
        private XRUserHead _userHeadPrefab;

        [SerializeField]
        private TrackingSpaceType _trackingSpaceType = TrackingSpaceType.RoomScale;

        private XRUserRootConfig configuration = null;
        private readonly List<XRUserController> _userControllers = new List<XRUserController>();
        private readonly List<XRNode> _acquiredXRNodes = new List<XRNode>();

        public XRUserHead UserHead { get; private set; }

        private void Awake()
        {
            try
            {
                Input.GetAxisRaw("Axis1");
            }
            catch (Exception e)
            {
                Debug.LogError("Input Manager for XRInteraction Library has not been setup." +
                               " This can be corrected by selecting Fjord > XRInteraction > Setup Input Manager. " +
                               e.Message);
                return;
            }
            
            XRDevice.SetTrackingSpaceType(_trackingSpaceType);
            
            configuration = null;
            if (XRSettings.loadedDeviceName == "OpenVR")
            {
                string openVRDevice = "OpenVR-" + OpenVRTrackingSystemName();
                Debug.Log("Trying to find XRUserConfiguration " + openVRDevice + " on " + name);
                configuration = _configs.Find(
                    c => c.DeviceName == openVRDevice
                );
            }
            else
            {
                Debug.Log("Trying to find XRUserConfiguration " + XRSettings.loadedDeviceName + " on " + name);
                configuration = _configs.Find(
                    c => c.DeviceName == XRSettings.loadedDeviceName
                );
            }

            if (null != configuration)
            {
                Debug.Log("Loading XRUserConfiguration: " + configuration.DeviceName);
                InputTracking.nodeAdded += InputTracking_nodeAdded;
                InputTracking.nodeRemoved += InputTracking_nodeRemoved;
                InputTracking.trackingAcquired += InputTracking_trackingAcquired;
                InputTracking.trackingLost += InputTracking_trackingLost;
            }
            else
            {
                Debug.LogError("XR User configuration not found for " + XRSettings.loadedDeviceName);
            }

            UserHead = Instantiate(_userHeadPrefab, Vector3.zero, Quaternion.identity);
            UserHead.gameObject.RemoveCloneSuffix();
            UserHead.transform.SetParent(transform);
        }

        private void InputTracking_trackingLost(XRNodeState nodeState)
        {
//            Debug.Log("InputTracking_trackingLost " + nodeState.nodeType);
            /*
            Debug.Log("InputTracking_trackingLost for UniqueID :" + nodeState.uniqueID);

            XRUserController controller = _userControllers.Find(x => x.UniqueID == nodeState.uniqueID);
            if (controller != null)
            {
                controller.gameObject.SetActive(false);
            }
            */
        }

        private void InputTracking_trackingAcquired(XRNodeState nodeState)
        {
//            Debug.Log("InputTracking_trackingAcquired " + nodeState.nodeType);
            //Debug.Log("InputTracking_trackingAcquired for UniqueID :" + nodeState.uniqueID);           

            
        }

        private void InputTracking_nodeRemoved(XRNodeState nodeState)
        {
            Debug.Log("InputTracking_nodeRemoved for UniqueID :" + nodeState.uniqueID);

            XRUserController controller = _userControllers.Find(x => x.UniqueID == nodeState.uniqueID);
            if(controller != null)
            {
                _userControllers.Remove(controller);
                Destroy(controller.gameObject);
            }
        }

        private void InputTracking_nodeAdded(XRNodeState nodeState)
        {
            Debug.Log("InputTracking_nodeAdded for nodeType :"+ nodeState.nodeType + "\nWith UniqueID : " + nodeState.uniqueID);

            if (_acquiredXRNodes.Contains(nodeState.nodeType))
            {
                return;
            }
            
            Chirality chirality = (Chirality)0; //0 is none
            if (nodeState.nodeType == XRNode.LeftHand)
            {
                chirality = (Chirality)1;
            }
            else if(nodeState.nodeType == XRNode.RightHand)
            {
                chirality = (Chirality)2;
            }
            else
            {
                return;
            }
            
            _acquiredXRNodes.Add(nodeState.nodeType);
            
            XRUserController addedController = Instantiate(
                configuration.UserControllerPrefab(chirality),
                Vector3.zero,
                Quaternion.identity);
            addedController.Initialize(
                chirality,
                this,
                configuration,
                nodeState.uniqueID);
            addedController.gameObject.RemoveCloneSuffix();
            addedController.transform.SetParent(transform);

            if (nodeState.nodeType == XRNode.LeftHand || nodeState.nodeType == XRNode.RightHand)
            {
                string joystickToFind = "left";
                if (nodeState.nodeType == XRNode.RightHand)
                {
                    joystickToFind = "right";
                }
                string[] joysticks = Input.GetJoystickNames();
                bool foundJoystick = false;
                foreach (string name in joysticks)
                {
                    if (name.ToLower().Contains(joystickToFind))
                    {
                        foundJoystick = true;
                        break;
                    }
                }

                XRUserController controller = _userControllers.Find(x => x.UniqueID == nodeState.uniqueID);
                if (controller != null)
                {
                    controller.gameObject.SetActive(foundJoystick);
                }
            }

            _userControllers.Add(addedController);
        }

        public XRUserController GetController(Chirality chirality)
        {
            return _userControllers.Find(c => c.ControllerChirality == chirality);
        }
        
        public void TweenTranslation(
            Vector3 translation,
            float speedMultiplier = 0,
            CurveAsset easeCurve = null,
            Action<Transform> step = null,
            Action<Transform> finished = null)
        {
            GetComponent<TransformTweener>().ToPosition(transform.position + translation, speedMultiplier, easeCurve, step, finished);
        }
            
        public void TweenToPosition(
            Vector3 target,
            float speedMultiplier = 0,
            CurveAsset easeCurve = null,
            Action<Transform> step = null,
            Action<Transform> finished = null)
        {
            GetComponent<TransformTweener>().ToPosition(target, speedMultiplier, easeCurve, step, finished);
        }
        
        private string OpenVRTrackingSystemName()
        {
            ETrackedDeviceProperty prop = ETrackedDeviceProperty.Prop_TrackingSystemName_String;
            uint deviceId = OpenVR.k_unTrackedDeviceIndex_Hmd;
            var error = ETrackedPropertyError.TrackedProp_Success;
            var capactiy = OpenVR.System.GetStringTrackedDeviceProperty(deviceId, prop, null, 0, ref error);
            var result = new System.Text.StringBuilder((int) capactiy);
            OpenVR.System.GetStringTrackedDeviceProperty(deviceId, prop, result, capactiy, ref error);
            return result.ToString();
        }
    }
}