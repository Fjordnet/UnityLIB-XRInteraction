using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.XRInput
{
    /// <summary>
    /// Unified interface which describes all events, and data, any type of input would need
    /// (e.g. button, joystick, analog button, etc). This interface is accessed through
    /// XRController.GetInput. The way in which an XR Controller maps it's inputs through
    /// this processor is defined by the XRInputMapping.
    /// </summary>
    public interface IXRInputProcessor
    {
        XRInputDescription InputDescription { get; }
        event Action<XRInputName> ButtonDown;
        event Action<XRInputName> ButtonHold;
        event Action<XRInputName> ButtonUp;
        event Action<XRInputName, float> AxisMove1D;
        event Action<XRInputName, Vector2> AxisMove2D;

        /// <summary>
        /// Called every update.
        /// </summary>
        void Process();

        float Get1DValue();
        Vector2 Get2DValue();
        bool GetButtonDown();
        bool GetButtonHold();
        bool GetButtonUp();
    }
}