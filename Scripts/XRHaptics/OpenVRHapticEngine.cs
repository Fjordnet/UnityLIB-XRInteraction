using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fjord.Common.Types;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace Fjord.XRInteraction.XRHaptics
{
    public class OpenVRHapticEngine : XRHapticEngine
    {
        [SerializeField]
        private float _normalizeMultiplier = 1000f;

        private bool _isOculus;
        private Chirality _controllerChirality;
        private Coroutine _hapticCoroutine;

        public override void Initialize(Chirality chirality)
        {
            _controllerChirality = chirality;
        }

        public override void StartHaptics(float normalizedIntensity, float durationInSeconds)
        {
            if (null != _hapticCoroutine)
            {
                StopCoroutine(_hapticCoroutine);
            }
            _hapticCoroutine = StartCoroutine(RunHapticCoroutine(normalizedIntensity, durationInSeconds));
        }

        public override void StopHaptics()
        {
            StopCoroutine(_hapticCoroutine);
            _hapticCoroutine = null;
        }

        private void TriggerHapticPulse(float normalizedIntensity, uint controllerId)
        {
            if (controllerId != 0)
            {
                ushort durationMicroSec = (ushort) (normalizedIntensity * _normalizeMultiplier);
                EVRButtonId buttonId = EVRButtonId.k_EButton_SteamVR_Touchpad;
                var axisId = (uint) buttonId - (uint) EVRButtonId.k_EButton_Axis0;
                OpenVR.System.TriggerHapticPulse(controllerId, axisId, (char) durationMicroSec);
            }
        }

        private IEnumerator RunHapticCoroutine(float normalizedIntensity, float durationInSeconds)
        {
            //OpenVR may not be able to find the the id right away, the ID of one controller
            //may also change as new controllers are connect, so you must check this everytime.
            uint controllerId = GetControllerId(_controllerChirality);
            float time = 0f;
            while (time < durationInSeconds)
            {
                TriggerHapticPulse(normalizedIntensity, controllerId);
                time += Time.deltaTime;
                yield return null;
            }

            _hapticCoroutine = null;
        }

        private uint GetControllerId(Chirality chirality)
        {
            ETrackedControllerRole targetRole = ETrackedControllerRole.Invalid;
            if (chirality == Chirality.Left)
                targetRole = ETrackedControllerRole.LeftHand;
            if (chirality == Chirality.Right)
                targetRole = ETrackedControllerRole.RightHand;
            
            uint index = 0;
            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex((uint) i) == targetRole)
                {
                    index = (uint)i;
                    break;
                }
            }

            return index;
        }
    }
}