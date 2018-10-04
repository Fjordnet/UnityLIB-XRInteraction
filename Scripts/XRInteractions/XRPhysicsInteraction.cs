using System;
using Fjord.Common.Types;
using Fjord.Common.Utilities;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fjord.XRInteraction.XRInteractions
{
    /// <summary>
    /// Implements common functionality for any interaction which rests on rigidbody and
    /// can interface with the attachable system.
    /// </summary>
    public class XRPhysicsInteraction : XRInteractionEventReceiver
    {
        [Tooltip("Smooth damping of position.")]
        [SerializeField]
        private float _moveDamp = .1f;

        [Tooltip("Smooth damping of rotation.")]
        [SerializeField]
        private float _rotationDamp = .1f;

        [FormerlySerializedAs("_overrideSlowDownMultiplier")]
        [Tooltip("Higher value will slow the transition in or out of a override transform.")]
        [SerializeField]
        private float _overrideSnapSlowDown = 6f;

        [Tooltip("Set RigidBody to isKinematic during translation.")]
        [SerializeField]
        private bool _setRigidbodyIsKinematic;

        private Vector3 _targetFixedUpdatePosition;
        private Vector3 _targetEuler;
        
        private bool _initialIsKinematic;
        //should be some kind of counter for when the MovementInteraction
        //wants it to be isKinematic and also Torqueable or other also sets isKinematic

        private bool _initialRigidyUseGravity;
        private Vector3 _moveVelocity;
        private Vector3 _rotateVelocity;
        private Transform _overrideDampTarget;
        private bool _setToInitialKinematicWhenNearInputTarget;
        private float _currentPositionDamp;
        private float _currentRotationDamp;

        /// <summary>
        /// The rotation which current input would like to place object at.
        /// </summary>
        public Quaternion? InputTargetRotation { get; protected set; }

        /// <summary>
        /// The position which the current input would like to place object at.
        /// </summary>
        public Vector3? InputTargetPosition { get; protected set; }

        public Vector3 PriorPosition { get; private set; }

        public Quaternion PriorRotation { get; private set; }

        public bool DisableAfterFinishDamp { get; set; }

        public Rigidbody AttachedRigidbody { get; private set; }
        
        public bool RetainIsKinematic { get; set; }

        public float MoveDamp
        {
            get { return _moveDamp; }
            set { _moveDamp = value; }
        }

        public float RotationDamp
        {
            get { return _rotationDamp; }
            set { _rotationDamp = value; }
        }

        protected virtual void Awake()
        {
            AttachedRigidbody = GetComponentInParent<Rigidbody>();
            if (null != AttachedRigidbody)
            {
                _initialIsKinematic = AttachedRigidbody.isKinematic;
                _initialRigidyUseGravity = AttachedRigidbody.useGravity;
            }
            else
            {
                Debug.LogError(name + " did not find RigidBody next to it, or parented to it.");
                enabled = false;
                return;
            }

            _currentPositionDamp = _moveDamp;
            _currentRotationDamp = _rotationDamp;
        }

        private void Update()
        {
            if (_overrideDampTarget != null || InputTargetRotation != null)
            {
                Quaternion targetRotation = null == _overrideDampTarget
                    ? InputTargetRotation.Value
                    : _overrideDampTarget.rotation;

                _currentRotationDamp = Mathf.MoveTowards(_currentRotationDamp, _rotationDamp, Time.deltaTime);
                float delta = Quaternion.Angle(AttachedRigidbody.transform.rotation, targetRotation);
                
                SetRotation(
                    Quaternion.RotateTowards(
                        AttachedRigidbody.transform.rotation, 
                        targetRotation, 
                        (Time.deltaTime * delta) / _currentRotationDamp
                        )
                    );
            }

            if (_overrideDampTarget != null || InputTargetPosition != null)
            {
                Vector3 targetPosition = null == _overrideDampTarget
                    ? InputTargetPosition.Value
                    : _overrideDampTarget.position;

                //wait until object has smoothed back near hand to set isKinematic is false
                //in case object had been smoothed inside another object, otherwise the object
                //would pop out of the collider
                if (_setToInitialKinematicWhenNearInputTarget &&
                    Vector3.Distance(AttachedRigidbody.transform.position, targetPosition) < .1f)
                {
                    _setToInitialKinematicWhenNearInputTarget = false;
                    AttachedRigidbody.isKinematic = _initialIsKinematic || RetainIsKinematic;
                }

                _currentPositionDamp = Mathf.MoveTowards(_currentPositionDamp, _moveDamp, Time.deltaTime);

                SetPosition(Vector3.SmoothDamp(
                    AttachedRigidbody.transform.position,
                    targetPosition,
                    ref _moveVelocity,
                    _currentPositionDamp)
                );
            }

            if (DisableAfterFinishDamp &&
                (AttachedRigidbody.transform.position - PriorPosition).sqrMagnitude < .01f &&
                Quaternion.Angle(AttachedRigidbody.transform.rotation, PriorRotation) < .01f)
            {
                enabled = false;
            }

            PriorPosition = AttachedRigidbody.transform.position;
            PriorRotation = AttachedRigidbody.transform.rotation;
        }

        public void SetOverrideDampTarget(Transform target)
        {
            _overrideDampTarget = target;
            _currentPositionDamp = _moveDamp * _overrideSnapSlowDown;
            _currentRotationDamp = _rotationDamp * _overrideSnapSlowDown;

            if (null != AttachedRigidbody)
            {
                AttachedRigidbody.isKinematic = true;
                _setToInitialKinematicWhenNearInputTarget = false;
            }
        }

        public void ClearOverrideDampTarget()
        {
            _currentPositionDamp = _moveDamp * _overrideSnapSlowDown;
            _currentRotationDamp = _rotationDamp * _overrideSnapSlowDown;

            _overrideDampTarget = null;
            if (null != AttachedRigidbody && !_initialIsKinematic)
            {
                //You do not want IsKinematic to be set to false immediately in case
                //in the attached state the object was intersecting with something.
                //This will make it wait until it smooths back to the hand before
                //turning off isKinematic
                _setToInitialKinematicWhenNearInputTarget = true;
            }
        }

        public override void OnButtonDown(XRButtonDatum buttonDatum)
        {
            base.OnButtonDown(buttonDatum);
            if (HeldButtons.Count == 1)
            {
                if (null != AttachedRigidbody)
                {
                    AttachedRigidbody.isKinematic = true;
                    AttachedRigidbody.useGravity = false;
                }
            }
        }

        public override void OnButtonUp(XRButtonDatum buttonDatum)
        {
            base.OnButtonUp(buttonDatum);

            if (HeldButtons.Count == 0)
            {
                _setToInitialKinematicWhenNearInputTarget = false;
                if (null != AttachedRigidbody && null == _overrideDampTarget)
                {
                    AttachedRigidbody.isKinematic = _initialIsKinematic || RetainIsKinematic;
                    AttachedRigidbody.useGravity = _initialRigidyUseGravity;
                    AttachedRigidbody.velocity = _moveVelocity;
                    AttachedRigidbody.angularVelocity = new Vector3(
                        _rotateVelocity.x * Mathf.Deg2Rad,
                        _rotateVelocity.y * Mathf.Deg2Rad,
                        _rotateVelocity.z * Mathf.Deg2Rad);
                }
            }
        }

        protected void SetPosition(Vector3 position)
        {
            if (null == AttachedRigidbody)
            {
                Debug.LogError(name + " did not find RigidBody next to it, or parented to it.");
            }
            else
            {
                AttachedRigidbody.velocity = Vector3.zero;
                if (AttachedRigidbody.isKinematic)
                {
                    AttachedRigidbody.transform.position = position;
                }
                else
                {
                    AttachedRigidbody.MovePosition(position);
                }
            }
        }
        
        protected void SetRotation(Quaternion rotation)
        {
            if (null == AttachedRigidbody)
            {
                Debug.LogError(name + " did not find RigidBody next to it, or parented to it.");
            }
            else
            {
                AttachedRigidbody.angularVelocity = Vector3.zero;
                if (AttachedRigidbody.isKinematic)
                {
                    AttachedRigidbody.transform.rotation = rotation;
                }
                else
                {
                    AttachedRigidbody.MoveRotation(rotation);
                }
            }
        }

        protected void SetEulerAngles(Vector3 eulerAngles)
        {
            if (null == AttachedRigidbody)
            {
                Debug.LogError(name + " did not find RigidBody next to it, or parented to it.");
            }
            else
            {
                AttachedRigidbody.angularVelocity = Vector3.zero;
                if (AttachedRigidbody.isKinematic)
                {
                    AttachedRigidbody.transform.eulerAngles = eulerAngles;
                }
                else
                {
                    AttachedRigidbody.MoveRotation(Quaternion.Euler(eulerAngles));
                }
            }
        }
    }
}