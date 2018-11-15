using System;
using Fjord.XRInteraction.XRUnityEvents;
using UnityEngine;

namespace Fjord.XRInteraction.XRUser
{
    [Serializable]
    public class RegistrationEvents
    {
        [SerializeField]
        private XRUserControllerUnityEvent _trackingAcquired;

        [SerializeField]
        private XRUserControllerUnityEvent _trackingLost;

        [SerializeField]
        private XRUserControllerUnityEvent _added;

        [SerializeField]
        private XRUserControllerUnityEvent _removed;

        public XRUserControllerUnityEvent TrackingAcquired
        {
            get { return _trackingAcquired; }
        }

        public XRUserControllerUnityEvent TrackingLost
        {
            get { return _trackingLost; }
        }

        public XRUserControllerUnityEvent Added
        {
            get { return _added; }
        }

        public XRUserControllerUnityEvent Removed
        {
            get { return _removed; }
        }
    }
}