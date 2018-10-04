using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fjord.Common.Attributes;
using Fjord.Common.Enums;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUnityEvents;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Base class which searches for Interactions to send events to.
    /// </summary>
    public abstract class XRInteractor : MonoBehaviour
    {
        [Header("Buttons which enable this Interactor. Nothing = Always on")]
        [EnumFlag]
        [SerializeField]
        private XRInputName _enableButtons;

        [Header("Disable other interactors on this condition.")]
        [SerializeField]
        protected SoloCondition _soloCondition;
        
        private int _enableButtonCount;
        private XRUserController _parentUserController;

        /// <summary>
        /// XRController which this interactor belongs to.
        /// </summary>
        public XRUserController ParentUserController
        {
            get { return _parentUserController; }
        }

        protected virtual void Awake()
        {
            _parentUserController = GetComponentInParent<XRUserController>();
            if (null == _parentUserController)
            {
                Debug.LogWarning(name + " needs to be a child of an XRController.");
            }
        }

        protected virtual void Start()
        {
            var enableButtonFlags = _enableButtons.GetFlags();
            if (enableButtonFlags.Count() > 0)
            {
                foreach (XRInputName inputName in enableButtonFlags)
                {
                    _parentUserController.GetInput(inputName).ButtonUp += EnableButtonUp;
                    _parentUserController.GetInput(inputName).ButtonDown += EnableButtonDown;
                }
                enabled = false;
            }
        }

        /// <summary>
        /// Called from XRController in order to control exectuion order of raycasting and button events.
        /// </summary>
        internal abstract void Process();

        private void EnableButtonDown(XRInputName inputName)
        {
            _enableButtonCount++;
            if (_enableButtonCount == 1)
            {
                enabled = true;
//                Process(); //should a buttonDown event also be sent on same frame that interactor is enabled?
                if (_soloCondition == SoloCondition.InteractorEnabled) _parentUserController.SetSoloState(false, this);
            }
        }

        private void EnableButtonUp(XRInputName inputName)
        {
            _enableButtonCount--;
            if (_enableButtonCount == 0)
            {
                enabled = false;
                if (_soloCondition == SoloCondition.InteractorEnabled) _parentUserController.SetSoloState(true, this);
            }
        }
    }
}