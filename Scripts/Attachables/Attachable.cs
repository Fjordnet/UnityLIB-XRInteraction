using System.Collections.Generic;
using System.Collections.ObjectModel;
using Fjord.Common.Extensions;
using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.Attachables
{
    public class Attachable : MonoBehaviour
    {
        [Header("Tags which represent this.")]
        [SerializeField]
        private AttachableTagMask _tags;

        [Header("Explicitly allow attachment to these holders.")]
        [SerializeField]
        private List<AttachableHolder> _canAttachTo = new List<AttachableHolder>();

        [Header("When object attaches, disable it's ability to removed.")]
        [SerializeField]
        private bool _lockObjectOnAttach;

        [Header("How far to search for AttachableHolders.")]
        [SerializeField]
        private float _scanDistance = .4f;

        [Header("AudioSource which will play attach sound.")]
        [SerializeField]
        private AudioClip _attachSound;

        [Header("AudioSource which will play release sound.")]
        [SerializeField]
        private AudioClip _releaseSound;

        [Header("Attachable Events")]
        [SerializeField]
        private AttachableEvents _events;

        /// <summary>
        /// Attachable areas this is currently in.
        /// </summary>
        private List<AttachableHolder> _entered = new List<AttachableHolder>();

        /// <summary>
        /// Attachable this is currently attached to.
        /// </summary>
        private AttachableHolder _attachedTo;

        private ReadOnlyCollection<AttachableHolder> _enteredReadOnly;
        private bool _initialIsKinematicState;
        private Vector3 _previewAttachVelocity;
        private Vector3 _attachOffset;
        private Transform _attachPoint;
        private AudioSource _audioSource;
        private PhysicsVolume _physicsVolume = new PhysicsVolume();

        public XRPhysicsInteraction MovementInteraction { get; private set; }

        /// <summary>
        /// If is a child GameObject in a more complex assembly, will be the parent
        /// transform which object should be moved through. Otherwise will be this transform.
        /// </summary>
        public Transform ParentTransform { get; private set; }

        public Rigidbody AttachedRigidbody { get; private set; }

        public AttachableTagMask Tags
        {
            get { return _tags; }
        }

        public List<AttachableHolder> CanAttachTo
        {
            get { return _canAttachTo; }
        }

        public AttachableEvents Events
        {
            get { return _events; }
        }

        public ReadOnlyCollection<AttachableHolder> Entered
        {
            get { return _enteredReadOnly ?? (_enteredReadOnly = _entered.AsReadOnly()); }
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (null == _audioSource && (null != _releaseSound || null != _attachSound))
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 1;
            }
            AttachedRigidbody = GetComponentInParent<Rigidbody>();
            ParentTransform = null == AttachedRigidbody ? transform : AttachedRigidbody.transform;
            if (null != AttachedRigidbody) _initialIsKinematicState = AttachedRigidbody.isKinematic;
            MovementInteraction = GetComponentInParent<XRPhysicsInteraction>();
            if (null != MovementInteraction && (_canAttachTo.Count > 0 || _tags.Count() > 0))
            {
                MovementInteraction.Events.ButtonDown.AddListener(ButtonDown);
                MovementInteraction.Events.ButtonsHeld.AddListener(ButtonsHeld);
                MovementInteraction.Events.ButtonUp.AddListener(ButtonUp);
            }
            if (MovementInteraction == null)
            {
                Debug.LogError(null == transform.parent
                    ? ""
                    : transform.parent.name + " " + name +
                      " could not find a MovementInteraction next to, or above it, in hiearchy.");
            }
            if (null != AttachedRigidbody && AttachedRigidbody.gameObject != gameObject)
            {
                _attachOffset = transform.localPosition;
            }
            _physicsVolume.Entered += PhysicsVolumeOnEntered;
            _physicsVolume.Exited += PhysicsVolumeOnExited;
        }

        public bool CanAttach(AttachableHolder holder)
        {
            if (holder.CanHoldTags.CompareTags(_tags))
            {
                return true;
            }
            if (_canAttachTo.Contains(holder))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Passed in attachable will begin holding this.
        /// </summary>
        public void AttachTo(AttachableHolder holder)
        {
            _attachedTo = holder;
            holder.BeginHoldingAttachable(this);
            if (_lockObjectOnAttach)
            {
                MovementInteraction.DisableAfterFinishDamp = true;
            }
            _events.Attached.Invoke(this, holder);
        }

        /// <summary>
        /// This will detach from anything holding it.
        /// </summary>
        public void Detach()
        {
            _events.Detached.Invoke(this, _attachedTo);
            if (null != _attachedTo)
            {
                _attachedTo.StopHoldingAttachable(this);
                _attachedTo = null;
            }
        }

        public void ResetIsKinematicState()
        {
            if (null != AttachedRigidbody) AttachedRigidbody.isKinematic = _initialIsKinematicState;
        }

        public void SetPositionRotationTo(Transform targetTransform)
        {
            ParentTransform.position = targetTransform.position;
            ParentTransform.rotation = targetTransform.rotation;
        }

        private void ButtonDown(XRButtonDatum buttonDatum, XRInteractionEventReceiver receiver)
        {
            if (receiver.HeldButtons.Count == 1)
            {
                Detach();
            }
        }

        private void ButtonsHeld(XRInteractionEventReceiver receiver)
        {
            Vector3 sourcePoint = MovementInteraction.InputTargetPosition.Value;
            if (_attachOffset != Vector3.zero)
            {
                sourcePoint += (MovementInteraction.InputTargetRotation.Value * _attachOffset);
            }
            _physicsVolume.TestForIntersections(sourcePoint, _scanDistance);
            Debug.DrawRay(sourcePoint, Vector3.up, Color.yellow);
        }

        private void PhysicsVolumeOnEntered(Collider collider)
        {
            AttachableHolder holder = collider.GetComponent<AttachableHolder>();
            if (null != holder && holder.CanHold(this))
            {
                if (null != _attachSound) _audioSource.PlayOneShot(_attachSound);
                _entered.Add(holder);
                if (_entered.Count == 1)
                {
                    MovementInteraction.SetOverrideDampTarget(holder.transform);
                }
                _events.Entered.Invoke(this, holder);
            }
        }

        private void PhysicsVolumeOnExited(Collider collider)
        {
            AttachableHolder holder = collider.GetComponent<AttachableHolder>();
            if (null != holder && _entered.Contains(holder))
            {
                if (null != _releaseSound) _audioSource.PlayOneShot(_releaseSound);
                _entered.Remove(holder);
                if (_entered.Count == 0)
                {
                    MovementInteraction.ClearOverrideDampTarget();
                }
                _events.Exited.Invoke(this, holder);
            }
        }

        private void ButtonUp(XRButtonDatum buttonDatum, XRInteractionEventReceiver receiver)
        {
            if (_entered.Count > 0 && null == _attachedTo)
            {
                AttachTo(_entered[0]);
            }
        }

        [ContextMenu("Set To First AttachableHolder")]
        private void SetToFirstAttachableHolder()
        {
            if (_canAttachTo.Count > 0)
            {
                Rigidbody _attachedRigidbody = GetComponentInParent<Rigidbody>();
                if (_attachedRigidbody != null)
                {
                    _attachedRigidbody.transform.position = _canAttachTo[0].transform.position;
                    _attachedRigidbody.transform.rotation = _canAttachTo[0].transform.rotation;
                }
                else
                {
                    transform.position = _canAttachTo[0].transform.position;
                    transform.rotation = _canAttachTo[0].transform.rotation;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_canAttachTo.Count > 0)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < _canAttachTo.Count; ++i)
                {
                    if (null != _canAttachTo[i])
                    {
                        Gizmos.DrawLine(transform.position, _canAttachTo[i].transform.position);
                    }
                }
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _scanDistance);
        }
    }
}