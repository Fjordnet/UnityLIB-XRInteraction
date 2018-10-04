using System.Collections.Generic;
using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.Attachables
{
    //TODO Can this fundamentally be merged with Attachable, and this and Attachable go off of a base class?
    //or should these types of things be kept separate and the common functionality moved into composable classes
    //ala physicsVolume 
    public class TorqueTool : MonoBehaviour
    {
        [Header("Tags which this can affect.")]
        [SerializeField]
        private AttachableTagMask _tags;

        [Header("How far to search for Torquables.")]
        [SerializeField]
        private float _scanDistance = .05f;

        [SerializeField]
        private float _tightenAnglePerSound = 4;

        [SerializeField]
        private AudioClip _attachSound;

        [SerializeField]
        private AudioClip _releaseSound;

        [SerializeField]
        private AudioClip _torqueForwardSound;

        [SerializeField]
        private AudioClip _torqueReverseSound;

        [SerializeField]
        private AudioClip _torqueLimitSound;

        private PhysicsVolume _physicsVolume = new PhysicsVolume();
        private List<Torquable> _entered = new List<Torquable>();
        private Vector3 _attachOffset;
        private Transform _auxTransform;
        private float _priorAngle;
        private float _accumulatedRotation;
        private float _priorDeltaAngle;
        private AudioSource _audioSource;
        private float _initialMoveDamp;
        private float _initialRotationDamp;

        public XRPhysicsInteraction MovementInteraction { get; private set; }
        public Rigidbody AttachedRigidbody { get; private set; }

        /// <summary>
        /// If is a child GameObject in a more complex assembly, will be the parent
        /// transform which object should be moved through. Otherwise will be this transform.
        /// </summary>
        public Transform ParentTransform { get; private set; }

        public AttachableTagMask Tags
        {
            get { return _tags; }
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (null == _audioSource &&
                (null != _attachSound ||
                 null != _releaseSound ||
                 null != _torqueForwardSound ||
                 null != _torqueReverseSound ||
                 null != _torqueLimitSound))
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 1;
            }

            _auxTransform = new GameObject("AuxTransform").transform;

            MovementInteraction = GetComponentInParent<XRPhysicsInteraction>();
            MovementInteraction.Events.ButtonsHeld.AddListener(ButtonsHeld);
            MovementInteraction.Events.ButtonDown.AddListener(ButtonDown);
            _physicsVolume.Entered += PhysicsVolumeOnEntered;
            _physicsVolume.Exited += PhysicsVolumeOnExited;
            _initialMoveDamp = MovementInteraction.MoveDamp;
            _initialRotationDamp = MovementInteraction.RotationDamp;

            AttachedRigidbody = GetComponentInParent<Rigidbody>();
            ParentTransform = null == AttachedRigidbody ? transform : AttachedRigidbody.transform;
            if (null != AttachedRigidbody && AttachedRigidbody.gameObject != gameObject)
            {
                _attachOffset = Vector3.Scale(transform.localPosition, transform.parent.lossyScale);
            }
        }

        private void Update()
        {
            if (_entered.Count > 0 && MovementInteraction.InputTargetPosition != null)
            {
//                MovementInteraction.SetOverrideDampTarget(_auxTransform);

                _auxTransform.position = _entered[0].transform.position;
                _auxTransform.rotation = _entered[0].transform.rotation;

                float angle = AngleOfTorquing();
                float deltaAngle = Mathf.DeltaAngle(angle, _priorAngle);

                if ((_priorDeltaAngle > 0 && deltaAngle < 0) || (_priorDeltaAngle < 0 && deltaAngle > 0))
                {
                    _accumulatedRotation = 0;
                }
                _accumulatedRotation += deltaAngle;

                if (deltaAngle < 0)
                {
                    bool limitReached = _entered[0].ApplyTorque(deltaAngle);
                    if (limitReached && _accumulatedRotation < -_tightenAnglePerSound)
                    {
                        _accumulatedRotation = 0;
                        _audioSource.PlayOneShot(_attachSound);
                        MovementInteraction.HeldButtons[0].ParentInteractor.ParentUserController
                            .FireHapticKey("BigTick");
                    }
                    else if (_accumulatedRotation < -_tightenAnglePerSound)
                    {
                        _accumulatedRotation = 0;
                        _audioSource.PlayOneShot(_torqueForwardSound);
                        MovementInteraction.HeldButtons[0].ParentInteractor.ParentUserController
                            .FireHapticKey("SmallTick");
                    }
                }

                if (_accumulatedRotation > _tightenAnglePerSound)
                {
                    _accumulatedRotation = 0;
                    _audioSource.PlayOneShot(_torqueReverseSound);
                    MovementInteraction.HeldButtons[0].ParentInteractor.ParentUserController.FireHapticKey("MidTick");
                }

                _priorDeltaAngle = deltaAngle;
                _priorAngle = angle;

                _auxTransform.Rotate(0, angle + 180, 0, Space.Self);
                _auxTransform.Translate(-_attachOffset, Space.Self);
            }
        }

        private void PhysicsVolumeOnEntered(Collider collider)
        {
            Torquable torquable = collider.GetComponent<Torquable>();
            if (null != torquable && torquable.CanBeTorquedBy(this))
            {
                _entered.Add(torquable);
                if (_entered.Count == 1)
                {
                    _priorAngle = AngleOfTorquing();
                    MovementInteraction.SetOverrideDampTarget(_auxTransform);
                    MovementInteraction.MoveDamp = 0;
                    MovementInteraction.RotationDamp = 0;
                    _audioSource.PlayOneShot(_attachSound);
                    MovementInteraction.HeldButtons[0].ParentInteractor.ParentUserController.FireHapticKey("Enter");
                }
            }
        }

        private void PhysicsVolumeOnExited(Collider collider)
        {
            Torquable torquable = collider.GetComponent<Torquable>();
            if (null != torquable && _entered.Contains(torquable))
            {
                _entered.Remove(torquable);
                if (torquable.Torqued) torquable.enabled = false;
                if (_entered.Count == 0)
                {
                    MovementInteraction.ClearOverrideDampTarget();
                    MovementInteraction.MoveDamp = _initialMoveDamp;
                    MovementInteraction.RotationDamp = _initialRotationDamp;
                    _audioSource.PlayOneShot(_releaseSound);
                    MovementInteraction.HeldButtons[0].ParentInteractor.ParentUserController.FireHapticKey("Exit");
                }
            }
        }

        private void ButtonDown(XRButtonDatum buttonDatum, XRInteractionEventReceiver receiver)
        {
//            if (_entered.Count > 0)
//            {
//                MovementInteraction.SetOverrideDampTarget(_auxTransform);
//            }
        }

        private void ButtonsHeld(XRInteractionEventReceiver receiver)
        {
            if (_entered.Count > 0)
            {
                const float extensionMultiplier = 1.2f;
                _physicsVolume.TestForIntersections(
                    MovementInteraction.InputTargetPosition.Value,
                    (_attachOffset.magnitude + _scanDistance) * extensionMultiplier);
            }
            else
            {
                Vector3 sourcePoint = SourcePoint();
                Debug.DrawRay(sourcePoint, Vector3.up * _scanDistance, Color.red);
                _physicsVolume.TestForIntersections(sourcePoint, _scanDistance);
            }
        }

        private float AngleOfTorquing()
        {
            Vector3 delta = _entered[0].transform
                .InverseTransformPoint(MovementInteraction.InputTargetPosition.Value);
            return Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
        }

        private Vector3 SourcePoint()
        {
            Vector3 sourcePoint = MovementInteraction.InputTargetPosition.Value;
            if (_attachOffset != Vector3.zero)
            {
                sourcePoint += (MovementInteraction.InputTargetRotation.Value * _attachOffset);
            }
            return sourcePoint;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _scanDistance);
        }
    }
}