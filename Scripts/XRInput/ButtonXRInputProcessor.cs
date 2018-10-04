using System;
using Fjord.Common.Enums;
using UnityEngine;

namespace Fjord.XRInteraction.XRInput
{
    /// <summary>
    /// Returns appropriate buttons events.
    /// </summary>
    public class ButtonProcessor : IXRInputProcessor
    {
        public event Action<XRInputName> ButtonDown;
        public event Action<XRInputName> ButtonHold;
        public event Action<XRInputName> ButtonUp;
        private readonly XRInputDescription _inputDescription;
        private float _priorValue;

        public XRInputDescription InputDescription
        {
            get { return _inputDescription; }
        }

        public ButtonProcessor(XRInputDescription inputDescription)
        {
            _inputDescription = inputDescription;
        }

        public event Action<XRInputName, float> AxisMove1D
        {
            add { }
            remove { }
        }

        public event Action<XRInputName, Vector2> AxisMove2D
        {
            add { }
            remove { }
        }

        public void Process()
        {
            if (GetButtonDown())
            {
                if (ButtonDown != null) ButtonDown(_inputDescription.InputName);
            }
            else if (GetButtonUp())
            {
                if (ButtonUp != null) ButtonUp(_inputDescription.InputName);
            }
            else if (GetButtonHold())
            {
                if (ButtonHold != null) ButtonHold(_inputDescription.InputName);
            }
        }

        public float Get1DValue()
        {
            return 0;
        }

        public Vector2 Get2DValue()
        {
            return Vector2.zero;
        }

        public bool GetButtonDown()
        {
            return Input.GetKeyDown(_inputDescription.ButtonKeycode);
        }

        public bool GetButtonHold()
        {
            return Input.GetKey(_inputDescription.ButtonKeycode);
        }

        public bool GetButtonUp()
        {
            return Input.GetKeyUp(_inputDescription.ButtonKeycode);
        }
    }
}