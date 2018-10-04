using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.XRHaptics
{
    [Serializable]
    public class XRHapticDescription
    {
        [SerializeField]
        private string _hapticKey;
        
        [SerializeField]
        private float _hapticNormalizedIntensity;

        [SerializeField]
        private float _hapticDuration;

        public string HapticKey
        {
            get { return _hapticKey; }
        }

        public float HapticNormalizedIntensity
        {
            get { return _hapticNormalizedIntensity; }
        }

        public float HapticDuration
        {
            get { return _hapticDuration; }
        }
    }
}