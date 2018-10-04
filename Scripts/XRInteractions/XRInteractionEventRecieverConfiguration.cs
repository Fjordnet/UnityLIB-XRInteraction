using System;
using Fjord.Common.Attributes;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEngine;


namespace Fjord.XRInteraction.XRInteractions
{
    [Serializable]
    public class XRInteractionEventRecieverConfiguration
    {
        [Header("Which button events will this recieve?")]
        [Tooltip("Note: The interactor also must be set to forward those button events")]
        [EnumFlag]
        [SerializeField]
        private XRInputName _inputNames = XRInputName.TriggerButton;

        [Header("How many simoultaneous presses?")]
        [SerializeField]
        private int _maxSimoultaneousHolds = 2;

        public XRInputName InputNames
        {
            get { return _inputNames; }
        }

        public int MaxSimoultaneousHolds
        {
            get { return _maxSimoultaneousHolds; }
        }
    }
}