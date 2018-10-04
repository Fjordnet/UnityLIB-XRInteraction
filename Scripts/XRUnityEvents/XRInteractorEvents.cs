using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.XRUnityEvents
{
    [Serializable]
    public class XRInteractorEvents
    {
        [SerializeField]
        [Header("Fired when interactor hovers over an object.")]
        private XRInteractorUnityEvent _enter;

        [SerializeField]
        [Header("Fired every frame interactor hovers over object.")]
        private XRInteractorUnityEvent _stay;

        [SerializeField]
        [Header("Fired when an interactor stops hovering object.")]
        private XRInteractorUnityEvent _exit;

        [SerializeField]
        [Header("Fired for first button down.")]
        private XRButtonDatumUnityEvent _firstButtonDown;
        
        [SerializeField]
        [Header("Fired for every button pressed down.")]
        private XRButtonDatumUnityEvent _buttonDown;

        [SerializeField]
        [Header("Fired every frame, for every button, being held down.")]
        private XRButtonDatumUnityEvent _buttonHold;

        [SerializeField]
        [Header("Fired for every button released.")]
        private XRButtonDatumUnityEvent _buttonUp;
        
        [SerializeField]
        [Header("Fired for last button released.")]
        private XRButtonDatumUnityEvent _lastButtonUp;

        /// <summary>
        /// Fired when interactor hovers over an object.
        /// </summary>
        public XRInteractorUnityEvent Enter
        {
            get { return _enter; }
        }

        /// <summary>
        /// Fired every frame interactor hovers over object.
        /// </summary>
        public XRInteractorUnityEvent Stay
        {
            get { return _stay; }
        }

        /// <summary>
        /// Fired when an interactor stops hovering object.
        /// </summary>
        public XRInteractorUnityEvent Exit
        {
            get { return _exit; }
        }

        /// <summary>
        /// Fired for first button down.
        /// </summary>
        public XRButtonDatumUnityEvent FirstButtonDown
        {
            get { return _firstButtonDown; }
        }

        /// <summary>
        /// Fired for every button pressed down.
        /// </summary>
        public XRButtonDatumUnityEvent ButtonDown
        {
            get { return _buttonDown; }
        }

        /// <summary>
        /// Fired every frame, for every button, being held down.
        /// </summary>
        public XRButtonDatumUnityEvent ButtonHold
        {
            get { return _buttonHold; }
        }

        /// <summary>
        /// Fired for every button released.
        /// </summary>
        public XRButtonDatumUnityEvent ButtonUp
        {
            get { return _buttonUp; }
        }

        /// <summary>
        /// Fired for last button released.
        /// </summary>
        public XRButtonDatumUnityEvent LastButtonUp
        {
            get { return _lastButtonUp; }
        }
    }
}