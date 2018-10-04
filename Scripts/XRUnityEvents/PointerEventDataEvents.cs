using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.UnityEvents;
using UnityEngine;

namespace Fjord.XRInteraction.XRUnityEvents
{
    [Serializable]
    public class PointerEventDataEvents
    {
        [SerializeField]
        [Header("Fired on hover over an object.")]
        private PointerEventDataUnityEvent _enter;

        [SerializeField]
        [Header("Fired every frame hover over object occurs.")]
        private PointerEventDataUnityEvent _stay;

        [SerializeField]
        [Header("Fired when hover over object stops.")]
        private PointerEventDataUnityEvent _exit;

        [SerializeField]
        [Header("Fired for every button pressed down.")]
        private PointerEventDataUnityEvent _buttonDown;

        [SerializeField]
        [Header("Fired every frame, for every button, being held down.")]
        private PointerEventDataUnityEvent _buttonHold;

        [SerializeField]
        [Header("Fired for every button released.")]
        private PointerEventDataUnityEvent _buttonUp;

        /// <summary>
        /// Fired when interactor hovers over an object.
        /// </summary>
        public PointerEventDataUnityEvent Enter
        {
            get { return _enter; }
        }

        /// <summary>
        /// Fired every frame interactor hovers over object.
        /// </summary>
        public PointerEventDataUnityEvent Stay
        {
            get { return _stay; }
        }

        /// <summary>
        /// Fired when an interactor stops hovering object.
        /// </summary>
        public PointerEventDataUnityEvent Exit
        {
            get { return _exit; }
        }
        
        /// <summary>
        /// Fired for every button pressed down.
        /// </summary>
        public PointerEventDataUnityEvent ButtonDown
        {
            get { return _buttonDown; }
        }

        /// <summary>
        /// Fired every frame, for every button, being held down.
        /// </summary>
        public PointerEventDataUnityEvent ButtonHold
        {
            get { return _buttonHold; }
        }

        /// <summary>
        /// Fired for every button released.
        /// </summary>
        public PointerEventDataUnityEvent ButtonUp
        {
            get { return _buttonUp; }
        }
    }
}