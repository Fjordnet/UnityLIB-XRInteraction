using System;
using Fjord.Common.Enums;
using Fjord.Common.Utilities;
using UnityEngine;

namespace Fjord.XRInteraction.XRInput
{
    //TODO this is not done
    /// <summary>
    /// Watches a 2D axis joystick and fires off ButtonUp, down and hold events
    /// based on the 2D axis passing a certain threshold, within a certain range of angle
    /// thresholds.
    /// </summary>
    public class JoystickPressProcessor : IXRInputProcessor
    {
        public event Action<XRInputName, Vector2> AxisMove2D;
        private readonly XRInputDescription _inputDescription;
        private readonly string _axis0Name;
        private readonly string _axis1Name;
        private Vector2 _priorValue;

        public XRInputDescription InputDescription
        {
            get { return _inputDescription; }
        }

        public JoystickPressProcessor(XRInputDescription inputDescription)
        {
            _inputDescription = inputDescription;
            _axis0Name = _inputDescription.Axis0Name.ToString();
            _axis1Name = _inputDescription.Axis1Name.ToString();
        }

        public event Action<XRInputName> ButtonDown
        {
            add { }
            remove { }
        }

        public event Action<XRInputName> ButtonHold
        {
            add { }
            remove { }
        }

        public event Action<XRInputName> ButtonUp
        {
            add { }
            remove { }
        }

        public event Action<XRInputName, float> AxisMove1D
        {
            add { }
            remove { }
        }

        public void Process()
        {
            Vector2 value = Get2DValue();
            if (!Vector2Utility.Approximately(value, _priorValue))
            {
                if (AxisMove2D != null) AxisMove2D(_inputDescription.InputName, value);
            }

            _priorValue = value;
        }

        public float Get1DValue()
        {
            throw new NotImplementedException();
        }

        public Vector2 Get2DValue()
        {
            return new Vector2(
                Input.GetAxisRaw(_axis0Name),
                Input.GetAxisRaw(_axis1Name));
        }

        public bool GetButtonDown()
        {
            return false;
        }

        public bool GetButtonHold()
        {
            return false;
        }

        public bool GetButtonUp()
        {
            return false;
        }
    }
}