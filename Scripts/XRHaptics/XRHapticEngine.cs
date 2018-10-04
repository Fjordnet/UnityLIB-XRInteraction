using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Types;
using UnityEngine;

namespace Fjord.XRInteraction.XRHaptics
{
    /// <summary>
    /// Implements Haptics on each platform. Implemented as a MonoBehaviour because depending on platform,
    /// it may be necessary to rely on Coroutines or an Update loop in order to produce certain Haptic effects.
    /// </summary>
    public abstract class XRHapticEngine : MonoBehaviour
    {
        public abstract void Initialize(Chirality chirality);
        public abstract void StartHaptics(float normalizedIntensity, float durationInSeconds);
        public abstract void StopHaptics();
        
        public virtual void PlayHapticDescription(XRHapticDescription description)
        {
            StartHaptics(description.HapticNormalizedIntensity, description.HapticDuration);
        }
    }
}