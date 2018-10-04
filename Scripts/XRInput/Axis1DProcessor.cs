using System;
using Fjord.Common.Enums;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fjord.XRInteraction.XRInput
{
    /// <summary>
    /// Returns axis 1D values.
    /// </summary>
    public class Axis1DProcessor : IXRInputProcessor
    {
        public event Action<XRInputName, float> AxisMove1D;
        private readonly XRInputDescription _inputDescription;
        private readonly string _axis0Name;
        private float _priorValue;

        public XRInputDescription InputDescription
        {
            get { return _inputDescription; }
        }

        public Axis1DProcessor(XRInputDescription inputDescription)
        {
            _inputDescription = inputDescription;
            _axis0Name = inputDescription.Axis0Name.ToString();
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

        public event Action<XRInputName, Vector2> AxisMove2D
        {
            add { }
            remove { }
        }

        public void Process()
        {
            float value = Get1DValue();
            if (!Mathf.Approximately(value, _priorValue))
            {
                if (AxisMove1D != null) AxisMove1D(_inputDescription.InputName, value);
            }

            _priorValue = value;
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