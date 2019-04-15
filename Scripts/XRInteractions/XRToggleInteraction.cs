using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRInteractors;
using Fjord.XRInteraction.XRUnityEvents;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Events;

namespace Fjord.XRInteraction.XRInteractions
{
    /// <summary>
    /// A simple toggle which fires events for being swithched on and off.
    /// </summary>
    public class XRToggleInteraction : XRInteractionEventReceiver
    {
        [SerializeField]
        private bool _toggleState;
        
        [SerializeField]
        private UnityEvent _toggleOn;

        [SerializeField]
        private UnityEvent _toggleOff;

        [Header("Change color on these MeshRenderers when toggled.")]
        [SerializeField]
        private MeshRenderer[] _meshRenderers;

        [SerializeField]
        private Color _onColor = Color.white;

        [SerializeField]
        private Color _offColor = Color.gray;

        private void Awake()
        {
            SetColorOnRenders(_toggleState ? _onColor : _offColor);
        }

        public void SetState(bool state)
        {
            _toggleState = state;
            SetColorOnRenders(_toggleState ? _onColor : _offColor);
        }

        public override void OnButtonUp(XRButtonDatum buttonDatum)
        {
            base.OnButtonUp(buttonDatum);

            _toggleState = !_toggleState;

            if (_toggleState)
            {
                _toggleOn.Invoke();
                SetColorOnRenders(_onColor);
            }
            else
            {
                _toggleOff.Invoke();
                SetColorOnRenders(_offColor);
            }
        }

        private void SetColorOnRenders(Color color)
        {
            for (int i = 0; i < _meshRenderers.Length; ++i)
            {
                _meshRenderers[i].material.color = color;
            }
        }
    }
}