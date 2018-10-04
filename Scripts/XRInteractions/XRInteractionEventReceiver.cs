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

namespace Fjord.XRInteraction.XRInteractions
{
    /// <summary>
    /// Recieves events from Interactor. Implement, and override virtual OnRecieveEvent methods
    /// in order to create an Interaction, or subscribe to public events. Provides common functionality
    /// useful to all Interactions such as FirstEnter, LastExit events and keeping track of what buttons
    /// and interactors are interfacing with it.
    /// </summary>
    public class XRInteractionEventReceiver : MonoBehaviour
    {
        [Header("Interaction Event Reciever Configuration.")]
        [SerializeField]
        private XRInteractionEventRecieverConfiguration _recieverConfiguration;

        [Header("Interaction Events")]
        [SerializeField]
        private XRInteractionUnityEvents _events;

        private List<XRPhysicsInteractor> _hoveringInteractors = new List<XRPhysicsInteractor>();
        private ReadOnlyCollection<XRPhysicsInteractor> _hoveringReadOnly;
        private List<XRButtonDatum> _heldButtons = new List<XRButtonDatum>();
        private ReadOnlyCollection<XRButtonDatum> _heldReadOnly;
        private Coroutine _hoveringCoroutine;

        /// <summary>
        /// Interactors currently hovering over this.
        /// </summary>
        public ReadOnlyCollection<XRPhysicsInteractor> HoveringInteractors
        {
            get { return _hoveringReadOnly ?? (_hoveringReadOnly = _hoveringInteractors.AsReadOnly()); }
        }

        /// <summary>
        /// Buttons currently being held down on this.
        /// </summary>
        public ReadOnlyCollection<XRButtonDatum> HeldButtons
        {
            get { return _heldReadOnly ?? (_heldReadOnly = _heldButtons.AsReadOnly()); }
        }

        public XRInteractionUnityEvents Events
        {
            get { return _events; }
        }

        protected virtual void OnEnable()
        {
            _events.Enabled.Invoke(this);
        }
        
        protected virtual void OnDisable()
        {
            for (int i = _heldButtons.Count - 1; i > -1; --i)
            {
                OnButtonUp(_heldButtons[i]);
            }
            _heldButtons.Clear();
            
            for (int i = _hoveringInteractors.Count - 1; i > -1; --i)
            {
                OnExit(_hoveringInteractors[i]);
            }
            _hoveringInteractors.Clear();
            
            _events.Disabled.Invoke(this);
        }

        public XRButtonDatum GetHeldButton(Chirality chirality)
        {
            return _heldButtons.Find((b) => b.ParentUserController.ControllerChirality == chirality);
        }

        public XRPhysicsInteractor GetHoveringInteractor(Chirality chirality)
        {
            return _hoveringInteractors.Find((i) => i.ParentUserController.ControllerChirality == chirality);
        }
        
        /// <summary>
        /// Has the max amount buttons being held down on this reciever been reached?
        /// </summary>
        public bool HoldAvailable()
        {
            return _heldButtons.Count < _recieverConfiguration.MaxSimoultaneousHolds;
        }

        public bool RecievesInput(XRInputName inputName)
        {
            return (_recieverConfiguration.InputNames & inputName) == inputName;
        }

        public virtual void OnFirstEnter(XRPhysicsInteractor interactor)
        {
            _events.FirstEnter.Invoke(interactor, this);
        }

        public virtual void OnEnter(XRPhysicsInteractor interactor)
        {
            _hoveringInteractors.Add(interactor);
            if (_hoveringInteractors.Count == 1 && _hoveringCoroutine == null)
            {
                _hoveringCoroutine = StartCoroutine(HoveringCoroutine());
                OnFirstEnter(interactor);
            }

            _events.Enter.Invoke(interactor, this);
        }

        public virtual void OnStay(XRPhysicsInteractor interactor)
        {
            _events.Stay.Invoke(interactor, this);
        }

        public virtual void OnInteractorsHovering()
        {
            _events.InteractorsHovering.Invoke(this);
        }

        public virtual void OnExit(XRPhysicsInteractor interactor)
        {
            _hoveringInteractors.Remove(interactor);
            if (_hoveringInteractors.Count == 0 && _heldButtons.Count == 0)
            {
                if (null != _hoveringCoroutine) StopCoroutine(_hoveringCoroutine);
                _hoveringCoroutine = null;
            }

            if (_hoveringInteractors.Count == 0)
            {
                OnLastExit(interactor);
            }

            _events.Exit.Invoke(interactor, this);
        }

        public virtual void OnLastExit(XRPhysicsInteractor interactor)
        {
            _events.LastExit.Invoke(interactor, this);
        }

        public virtual void OnButtonDown(XRButtonDatum buttonDatum)
        {
            _heldButtons.Add(buttonDatum);
            if (_heldButtons.Count == 1 && _hoveringCoroutine == null)
            {
                _hoveringCoroutine = StartCoroutine(HoveringCoroutine());
            }

            _events.ButtonDown.Invoke(buttonDatum, this);
        }

        public virtual void OnButtonHold(XRButtonDatum buttonDatum)
        {
            _events.ButtonHold.Invoke(buttonDatum, this);
        }

        public virtual void OnButtonsHeld()
        {
            _events.ButtonsHeld.Invoke(this);
        }

        public virtual void OnButtonUp(XRButtonDatum buttonDatum)
        {
            _heldButtons.Remove(buttonDatum);
            if (_hoveringInteractors.Count == 0 && _heldButtons.Count == 0)
            {
                if (null != _hoveringCoroutine) StopCoroutine(_hoveringCoroutine);
                _hoveringCoroutine = null;
            }
            
            _events.ButtonUp.Invoke(buttonDatum, this);
        }

        private IEnumerator HoveringCoroutine()
        {
            while (enabled)
            {
                yield return null;
                if (_hoveringInteractors.Count > 0)
                {
                    OnInteractorsHovering();
                }

                if (_heldButtons.Count > 0)
                {
                    OnButtonsHeld();
                }
            }
        }
    }
}