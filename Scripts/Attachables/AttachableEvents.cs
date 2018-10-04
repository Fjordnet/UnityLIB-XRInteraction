using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.Attachables
{
    [System.Serializable]
    public class AttachableEvents
    {
        [SerializeField]
        private AttachableAttachableHolderUnityEvent _attached;

        [SerializeField]
        private AttachableAttachableHolderUnityEvent _detached;
        
        [SerializeField]
        private AttachableAttachableHolderUnityEvent _entered;
        
        [SerializeField]
        private AttachableAttachableHolderUnityEvent _exited;

        public AttachableAttachableHolderUnityEvent Attached
        {
            get { return _attached; }
        }

        public AttachableAttachableHolderUnityEvent Detached
        {
            get { return _detached; }
        }

        public AttachableAttachableHolderUnityEvent Entered
        {
            get { return _entered; }
        }

        public AttachableAttachableHolderUnityEvent Exited
        {
            get { return _exited; }
        }
    }
}