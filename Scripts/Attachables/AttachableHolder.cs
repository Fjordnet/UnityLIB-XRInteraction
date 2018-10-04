using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Types;
using UnityEngine;

namespace Fjord.XRInteraction.Attachables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class AttachableHolder : MonoBehaviour
    {
        [Header("Which tags can this hold?")]
        [SerializeField]
        private AttachableTagMask _canHoldTags;

        [Header("Technique by which child will be hold be parent.")]
        [Tooltip("UpdateLock: Child rigidbody will be set to IsKinematic and " +
                 "it's transform position/rotation will be set every Update." +
                 "Child: Child rigidbody will be removed and made an actual child " +
                 "of the parent in the hiearchy.")]
        [SerializeField]
        private HoldBehaviour _holdBehaviour;

        [SerializeField]
        private Transform _visualizerTarget;

        [Header("Fired on attached/detach.")]
        [SerializeField]
        private AttachableEvents _attachableEvents;

        /// <summary>
        /// Attachables attached to this.
        /// </summary>
        private List<Attachable> _attached = new List<Attachable>();

        public AttachableTagMask CanHoldTags
        {
            get { return _canHoldTags; }
        }

        public AttachableEvents Events
        {
            get { return _attachableEvents; }
        }

        public Transform VisualizerTarget
        {
            get { return _visualizerTarget == null ? transform : _visualizerTarget; }
        }

        private void Update()
        {
            if (_holdBehaviour == HoldBehaviour.UpdateLock)
            {
                for (int i = 0; i < _attached.Count; ++i)
                {
                    _attached[i].SetPositionRotationTo(transform);
                }
            }
        }

        public bool CanHold(Attachable attachable)
        {
            if (!enabled) return false;
            if (_canHoldTags.CompareTags(attachable.Tags))
            {
                return true;
            }
            if (attachable.CanAttachTo.Contains(this))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// This will begin holding the passed in attachable.
        /// </summary>
        public void BeginHoldingAttachable(Attachable attachable)
        {
            _attached.Add(attachable);
            switch (_holdBehaviour)
            {
                case HoldBehaviour.UpdateLock:
                    if (null != attachable.AttachedRigidbody)
                    {
                        attachable.AttachedRigidbody.isKinematic = true;
                        attachable.MovementInteraction.RetainIsKinematic = true;
                    }
                    break;
                case HoldBehaviour.Child:
                    //is this really the only way? rigidbody needs to be added back on remove
                    if (null != attachable.AttachedRigidbody)
                    {
//                        Destroy(attachable.AttachedRigidbody); //causes a pauses or will it without meshcolliders?
                        attachable.AttachedRigidbody.isKinematic = true;
                        attachable.MovementInteraction.RetainIsKinematic = true;
                        attachable.AttachedRigidbody.transform.SetParent(transform);
                    }
                    else
                    {
                        attachable.transform.SetParent(transform);
                    }

                    break;
            }
            _attachableEvents.Attached.Invoke(attachable, this);
        }

        /// <summary>
        /// This will stop holding the passed in attachable.
        /// </summary>
        public void StopHoldingAttachable(Attachable attachable)
        {
            _attached.Remove(attachable);
            switch (_holdBehaviour)
            {
                case HoldBehaviour.UpdateLock:
                    attachable.ResetIsKinematicState();
                    break;
                case HoldBehaviour.Child:
                    throw new NotImplementedException();
                    //add rigidbody back on if it existed
            }
            _attachableEvents.Detached.Invoke(attachable, this);
        }
    }
}