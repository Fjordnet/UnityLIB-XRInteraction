using Fjord.XRInteraction.XRInteractions;
using UnityEngine;

namespace Fjord.XRInteraction.XRUser
{
    /// <summary>
    /// Accepts one or two controllers inputs and responds appropriately to
    /// position and rotation in free movement.
    /// </summary>
    public class XRFreeMovementInteraction : XRPhysicsInteraction
    {
        [Tooltip("Attach this object to the interactor.")]
        [SerializeField]
        private bool _attachToInteractorPosition;

        [Tooltip("Match rotation to the interactor.")]
        [SerializeField]
        private bool _matchInteractorRotation;

        [Tooltip("When matching interactor rotation, this will offset the rotation.")]
        [SerializeField]
        private Vector3 _rotationOffset;

        [Tooltip("Will rotate more appropriately around forward direction of the user. " +
                 "This is ideal for the rotation movement similiar to a steering wheel.")]
        [SerializeField]
        private bool _preferRotationAroundZ;

        private Vector3 _priorManipulationPosition;
        private Quaternion _priorManipulateRotation;
        private Transform _manipulationTransform;
        private Transform _subManipulationTransform;

        protected override void Awake()
        {
            base.Awake();
            _manipulationTransform = new GameObject("ManipulationTransform").transform;
            _subManipulationTransform = new GameObject("SubManipulationTransform").transform;
            _subManipulationTransform.SetParent(_manipulationTransform);
            _manipulationTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;
            _subManipulationTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        public override void OnButtonDown(XRButtonDatum buttonDatum)
        {
            base.OnButtonDown(buttonDatum);

            if (HeldButtons.Count == 1)
            {
                OneButtonBegin(true);
            }
            else if (HeldButtons.Count == 2)
            {
                _manipulationTransform.position = AveragedPosition();
                _manipulationTransform.LookAt(HeldButtons[1].RayHitChildedToController.origin, RayOnHitObjectUp());
                _subManipulationTransform.position = InputTargetPosition.Value;
                _subManipulationTransform.rotation = AttachedRigidbody.transform.rotation;
                
                if (_attachToInteractorPosition)
                {
                    _priorManipulateRotation = _manipulationTransform.rotation;
                }
                else
                {
                    _priorManipulateRotation = _subManipulationTransform.rotation;
                }
                InputTargetRotation = _priorManipulateRotation;
                
                if (_attachToInteractorPosition)
                    InputTargetPosition = _manipulationTransform.position;
                else
                    InputTargetPosition = _subManipulationTransform.position;
            }
        }

        public override void OnButtonsHeld()
        {
            base.OnButtonsHeld();

            //position
            if (HeldButtons.Count == 1)
            {
                _manipulationTransform.position = HeldButtons[0].ParentInteractor.transform.position;
            }
            else if (HeldButtons.Count == 2)
            {
                _manipulationTransform.position = AveragedPosition();
            }

            if (_attachToInteractorPosition)
                InputTargetPosition = _manipulationTransform.position;
            else
                InputTargetPosition = _subManipulationTransform.position;

            //rotation
            if (HeldButtons.Count == 1)
            {
                _manipulationTransform.rotation = HeldButtons[0].ParentInteractor.transform.rotation;
                _manipulationTransform.Rotate(_rotationOffset, Space.Self);
            }
            else if (HeldButtons.Count == 2)
            {
                _manipulationTransform.LookAt(HeldButtons[1].RayHitChildedToController.origin, RayOnHitObjectUp());
            }

            Quaternion deltaRotation = Quaternion.identity;
            if (_matchInteractorRotation)
            {
                deltaRotation = _manipulationTransform.rotation *
                                Quaternion.Inverse(_priorManipulateRotation);
                _priorManipulateRotation = _manipulationTransform.rotation;
            }
            else
            {
                deltaRotation = _subManipulationTransform.rotation *
                                Quaternion.Inverse(_priorManipulateRotation);
                _priorManipulateRotation = _subManipulationTransform.rotation;
            }

            InputTargetRotation = deltaRotation * InputTargetRotation;
        }

        public override void OnButtonUp(XRButtonDatum buttonDatum)
        {
            base.OnButtonUp(buttonDatum);
            if (HeldButtons.Count == 1)
            {
                OneButtonBegin(false);
            }
            if (HeldButtons.Count == 0)
            {
                InputTargetPosition = null;
                InputTargetRotation = null;
            }
        }

        /// <summary>
        /// Called to setup state for one button interaction.
        /// </summary>
        /// <param name="interactionBegining"> Is this first cycle of this interaction? </param>
        private void OneButtonBegin(bool interactionBegining)
        {
            _manipulationTransform.position = HeldButtons[0].ParentInteractor.transform.position;
            _manipulationTransform.rotation = HeldButtons[0].ParentInteractor.transform.rotation;
            _manipulationTransform.Rotate(_rotationOffset, Space.Self);
            _subManipulationTransform.position =
                interactionBegining ? AttachedRigidbody.transform.position : InputTargetPosition.Value;
            _subManipulationTransform.rotation = AttachedRigidbody.transform.rotation;

            if (_attachToInteractorPosition)
                InputTargetPosition = _manipulationTransform.position;
            else
                InputTargetPosition = _subManipulationTransform.position;

            if (_matchInteractorRotation)
            {
                InputTargetRotation = _manipulationTransform.rotation;
            }
            else
            {
                InputTargetRotation = _subManipulationTransform.rotation;
            }
            _priorManipulateRotation = InputTargetRotation.Value;
        }

        private Vector3 AverageInteractorForward()
        {
            Vector3 averageUp = HeldButtons[0].ParentInteractor.transform.forward +
                                HeldButtons[1].ParentInteractor.transform.forward;
            averageUp /= 2;
            return averageUp;
        }

        private Vector3 AverageInteractorUp()
        {
            Vector3 averageUp = HeldButtons[0].ParentInteractor.transform.up +
                                HeldButtons[1].ParentInteractor.transform.up;
            averageUp /= 2;
            return averageUp;
        }

        private Vector3 RayOnHitObjectUp()
        {
            Vector3 rayHitDirection = HeldButtons[1].RayHitChildedToController.origin -
                                      HeldButtons[0].RayHitChildedToController.origin;
            Vector3 averageForward = Vector3.zero;
            if (_preferRotationAroundZ)
            {
                averageForward = AverageInteractorForward();
            }
            else
            {
                averageForward = AverageInteractorUp();
            }
            
            Vector3 rayUp = Vector3.Cross(rayHitDirection, averageForward);
            return rayUp;
        }

        private Vector3 AveragedPosition()
        {
            return (HeldButtons[0].RayHitChildedToController.origin +
                    HeldButtons[1].RayHitChildedToController.origin) / 2f;
        }
    }
}