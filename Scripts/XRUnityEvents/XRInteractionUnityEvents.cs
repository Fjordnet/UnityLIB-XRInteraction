using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.XRUnityEvents
{
    [Serializable]
    public class XRInteractionUnityEvents
    {
        [SerializeField]
        [Header("Fired when first interactor hovers over this object.")]
        private XRInteractorInteractionUnityEvent _firstEnter = new XRInteractorInteractionUnityEvent();

        [SerializeField]
        [Header("Fired when an interactor hovers over this object.")]
        private XRInteractorInteractionUnityEvent _enter = new XRInteractorInteractionUnityEvent();

        [SerializeField]
        [Header("Fired every frame, for every interactor, that hovers on this object.")]
        private XRInteractorInteractionUnityEvent _stay = new XRInteractorInteractionUnityEvent();

        [SerializeField]
        [Header("Fired only once a frame if there are interactors hovering.")]
        private XRInteractionUnityEvent _interactorsHovering = new XRInteractionUnityEvent();

        [SerializeField]
        [Header("Fired when all interactors stop hovering over this object.")]
        private XRInteractorInteractionUnityEvent _lastExit = new XRInteractorInteractionUnityEvent();

        [SerializeField]
        [Header("Fired when an interactor stops hovering over this object.")]
        private XRInteractorInteractionUnityEvent _exit = new XRInteractorInteractionUnityEvent();

        [SerializeField]
        [Header("Fired for first button down.")]
        private XRButtonDatumInteractionUnityEvent _firstButtonDown = new XRButtonDatumInteractionUnityEvent();
        
        [SerializeField]
        [Header("Fired for every button pressed down on an interactor hovering over this object.")]
        private XRButtonDatumInteractionUnityEvent _buttonDown = new XRButtonDatumInteractionUnityEvent();

        [SerializeField]
        [Header("Fired for every frame, for every button, being held down.")]
        private XRButtonDatumInteractionUnityEvent _buttonHold = new XRButtonDatumInteractionUnityEvent();

        [SerializeField]
        [Header("Fired for every button released.")]
        private XRButtonDatumInteractionUnityEvent _buttonUp = new XRButtonDatumInteractionUnityEvent();
        
        [SerializeField]
        [Header("Fired for last button released.")]
        private XRButtonDatumInteractionUnityEvent _lastButtonUp = new XRButtonDatumInteractionUnityEvent();

        [SerializeField]
        [Header("Fired only once a frame if there are interactors with buttons being held.")]
        private XRInteractionUnityEvent _buttonsHeld = new XRInteractionUnityEvent();
        
        [SerializeField]
        [Header("Fired when enabled.")]
        private XRInteractionUnityEvent _enabled = new XRInteractionUnityEvent();
                
        [SerializeField]
        [Header("Fired when disabled.")]
        private XRInteractionUnityEvent _disabled = new XRInteractionUnityEvent();

        /// <summary>
        /// Fired when first interactor hovers over this object.
        /// </summary>
        public XRInteractorInteractionUnityEvent FirstEnter
        {
            get { return _firstEnter; }
        }

        /// <summary>
        /// Fired when an interactor hovers over this object.
        /// </summary>
        public XRInteractorInteractionUnityEvent Enter
        {
            get { return _enter; }
        }

        /// <summary>
        /// Fired every frame, for every interactor, that hovers on this object.
        /// </summary>
        public XRInteractorInteractionUnityEvent Stay
        {
            get { return _stay; }
        }

        /// <summary>
        /// Fired only once a frame if there are interactors hovering.
        /// </summary>
        public XRInteractionUnityEvent InteractorsHovering
        {
            get { return _interactorsHovering; }
        }

        /// <summary>
        /// Fired when all interactors stop hovering over this object.
        /// </summary>
        public XRInteractorInteractionUnityEvent LastExit
        {
            get { return _lastExit; }
        }

        /// <summary>
        /// Fired when an interactor stops hovering over this object.
        /// </summary>
        public XRInteractorInteractionUnityEvent Exit
        {
            get { return _exit; }
        }

        /// <summary>
        /// Fired for first button down.
        /// </summary>
        public XRButtonDatumInteractionUnityEvent FirstButtonDown
        {
            get { return _firstButtonDown; }
        }

        /// <summary>
        /// Fired for every button pressed down on an interactor hovering over this object.
        /// </summary>
        public XRButtonDatumInteractionUnityEvent ButtonDown
        {
            get { return _buttonDown; }
        }

        /// <summary>
        /// Fired for every frame, for every button, being held down.
        /// </summary>
        public XRButtonDatumInteractionUnityEvent ButtonHold
        {
            get { return _buttonHold; }
        }

        /// <summary>
        /// Fired for every button released.
        /// </summary>
        public XRButtonDatumInteractionUnityEvent ButtonUp
        {
            get { return _buttonUp; }
        }

        /// <summary>
        /// Fired for last button released.
        /// </summary>
        public XRButtonDatumInteractionUnityEvent LastButtonUp
        {
            get { return _lastButtonUp; }
        }

        /// <summary>
        /// Fired only once a frame if there are interactors with buttons being held.
        /// </summary>
        public XRInteractionUnityEvent ButtonsHeld
        {
            get { return _buttonsHeld; }
        }

        public XRInteractionUnityEvent Enabled
        {
            get { return _enabled; }
        }

        public XRInteractionUnityEvent Disabled
        {
            get { return _disabled; }
        }
    }
}