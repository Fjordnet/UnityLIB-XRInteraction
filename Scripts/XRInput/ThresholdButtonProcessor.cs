using System;
using Fjord.Common.Enums;
using UnityEngine;

namespace Fjord.XRInteraction.XRInput
{
    using ButtonState = Fjord.Common.Enums.ButtonState;

    public class ThresholdButtonProcessor : IXRInputProcessor
    {
        public event Action<XRInputName> ButtonDown;
        public event Action<XRInputName> ButtonHold;
        public event Action<XRInputName> ButtonUp;
        private readonly XRInputDescription _inputDescription;
        private readonly string _axis0Name;
        private ButtonState _state;

        public XRInputDescription InputDescription
        {
            get { return _inputDescription; }
        }

        public ThresholdButtonProcessor(XRInputDescription inputDescription)
        {
            _inputDescription = inputDescription;
            _axis0Name = _inputDescription.Axis0Name.ToString();
        }

        public event Action<XRInputName, float> AxisMove1D
        {
            add { }
            remove { }
        }

        public event Action<XRInputName, Vector2> AxisMove2D
        {
            add {  }
            remove {  }
        }

        public void Process()
        {
            if (_state == ButtonState.None && Get1DValue() > _inputDescription.DownThreshold)
            {
                _state = ButtonState.Down;
                if (ButtonDown != null) ButtonDown(_inputDescription.InputName);
            }
            else if (_state == ButtonState.Down)
            {
                _state = ButtonState.Hold;
                if (ButtonHold != null) ButtonHold(_inputDescription.InputName);
            }
            else if (_state == ButtonState.Hold && Get1DValue() < _inputDescription.UpThreshold)
            {
                _state = ButtonState.Up;
                if (ButtonUp != null) ButtonUp(_inputDescription.InputName);
            }
            else if (_state == ButtonState.Hold)
            {
                if (ButtonHold != null) ButtonHold(_inputDescription.InputName);
            }
            else if (_state == ButtonState.Up)
            {
                _state = ButtonState.None;
            }
        }

        public float Get1DValue()
        {
            return Input.GetAxisRaw(_axis0Name);
        }

        public Vector2 Get2DValue()
        {
            return Vector2.zero;
        }

        public bool GetButtonDown()
        {
            return _state == ButtonState.Down;
        }

        public bool GetButtonHold()
        {
            return _state == ButtonState.Hold;
        }

        public bool GetButtonUp()
        {
            return _state == ButtonState.Up;
        }
    }
}