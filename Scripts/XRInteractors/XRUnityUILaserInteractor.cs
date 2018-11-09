using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Enums;
using Fjord.Common.Extensions;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUnityEvents;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// This connects the XR Controllers to the Unity UI. The UI must have a XRCanvas on it
    /// in order to receive input, and the EventSystem should be paired with the XRInputModule.
    /// 
    /// The entire Unity UI system is deeply intertwined with the need for a Camera in order to work. 
    /// This systems works in reverse of how it typically works through a screen. Each controller
    /// has a disabled camera, and the disabled camera moves in sync with the controller, reporting the
    /// position of the pointer position as being in the center of the screen. So technically it is the
    /// camera moving, not the actual pointer on the screen. The Cameras do not need to be rendering to
    /// work with the UI system, so they still work disabled.
    /// 
    /// This gives you the benefit that you do not have to be looking at the UI to make it work. You
    /// also do not need to put a collider around the bounds of your UI for a pointer to intersect with.
    /// It also takes much less code for it to work like this, so it wil be easier to maintain. 
    /// </summary>
    public class XRUnityUILaserInteractor : XRInteractor
    {
        [Header("Button which will click Unity UI.")]
        [SerializeField]
        private XRInputName _uiClickButton = XRInputName.TriggerButton;

        [Header("Interactor Events")]
        [SerializeField]
        private PointerEventDataEvents _events;

        [SerializeField]
        private float _pointerPositionDamp = .1f;

        [SerializeField]
        private float _pointerRotationDamp = .1f;

        public Camera EventCamera { get; private set; }
        public Vector2 DeltaInScreen { get; private set; }
        public PointerEventData EventData { get; private set; }

        public PointerEventDataEvents Events
        {
            get { return _events; }
        }

        private bool _entered;
        private bool _pressed;
        private float _cumulatedDelta;
        private Vector3 _priorWorldPosition;
        private Vector3 _cameraVelocity;
        private float _currentRotationDamp;

        protected override void Start()
        {
            base.Start();
            GameObject eventCameraGameObject = new GameObject("EventCamera");
            eventCameraGameObject.transform.parent = transform.parent.parent;
            eventCameraGameObject.transform.position = transform.position;
            eventCameraGameObject.transform.rotation = transform.rotation;
            EventCamera = eventCameraGameObject.AddComponent<Camera>();
            EventCamera.enabled = false;
        }

        internal override void Process()
        {
        }

        private void Update()
        {
            EventCamera.transform.position = Vector3.SmoothDamp(
                EventCamera.transform.position,
                transform.position,
                ref _cameraVelocity,
                _pointerPositionDamp);

            _currentRotationDamp = Mathf.MoveTowards(_currentRotationDamp, _pointerRotationDamp, Time.deltaTime);
            float delta = Quaternion.Angle(EventCamera.transform.rotation, transform.rotation);
            
            EventCamera.transform.rotation = Quaternion.RotateTowards(
                EventCamera.transform.rotation,
                transform.rotation,
                (Time.deltaTime * delta) / _currentRotationDamp
            );

            Vector2 priorScreenPoint = EventCamera.WorldToScreenPoint(_priorWorldPosition);
            DeltaInScreen = PositionInCamera() - priorScreenPoint;
            Ray ray = EventCamera.ScreenPointToRay(PositionInCamera());
            _priorWorldPosition = ray.GetPoint(.01f);
        }

        public Vector2 PositionInCamera()
        {
            return new Vector2(EventCamera.pixelWidth / 2f, EventCamera.pixelHeight / 2f);
        }

        public bool GetButtonDown()
        {
            return ParentUserController.GetInput(_uiClickButton).GetButtonDown();
        }

        public bool GetButtonHold()
        {
            return ParentUserController.GetInput(_uiClickButton).GetButtonHold();
        }

        public bool GetButtonUp()
        {
            return ParentUserController.GetInput(_uiClickButton).GetButtonUp();
        }

        /// <summary>
        /// This is called from the XRInput module, where it will pass in the eventData that this Interactor is
        /// associated with. The UnityUI system relies heavily on the data structure PointerEventData, there is
        /// one PointerEventData per controller. So in essence this class acts like a wrapper of the PointerEventData
        /// so it can interact with the XRInteraction system appropriately..
        /// </summary>
        public void UpdatePointerEventData(PointerEventData eventData, bool pressed, bool released)
        {
            EventData = eventData;

            if (null != EventData.pointerEnter && !_entered)
            {
                EventCamera.transform.position = transform.position;
                EventCamera.transform.rotation = transform.rotation;
                if (_soloCondition == SoloCondition.InteractorHoveredOverInteraction)
                {
                    ParentUserController.SetSoloState(false, this);
                }
                _entered = true;
                ParentUserController.FireHapticKey("Enter");
                _events.Enter.Invoke(eventData);
            }
            else if (null == EventData.pointerEnter && _entered)
            {
                if (_soloCondition == SoloCondition.InteractorHoveredOverInteraction)
                {
                    ParentUserController.SetSoloState(true, this);
                }
                _entered = false;
                ParentUserController.FireHapticKey("Exit");
                _events.Exit.Invoke(eventData);
            }
            else if (_entered)
            {
                _events.Stay.Invoke(eventData);
            }

            if (pressed)
            {
                if (_soloCondition == SoloCondition.InteractorPressedOnInteraction)
                {
                    ParentUserController.SetSoloState(false, this);
                }
                ParentUserController.FireHapticKey("Press");
                _pressed = true;
                _events.ButtonDown.Invoke(eventData);
            }
            else if (released)
            {
                if (_soloCondition == SoloCondition.InteractorPressedOnInteraction)
                {
                    ParentUserController.SetSoloState(true, this);
                }
                ParentUserController.FireHapticKey("Release");
                _pressed = false;
                _events.ButtonUp.Invoke(eventData);
            }
            else if (_pressed)
            {
                _events.ButtonHold.Invoke(eventData);
            }

            if (eventData.dragging && eventData.pointerCurrentRaycast.gameObject != null)
            {
                _cumulatedDelta += DeltaInScreen.sqrMagnitude;
                if (_cumulatedDelta > 200)
                {
                    _cumulatedDelta = 0;
                    ParentUserController.FireHapticKey("DragTick");
                }
            }
        }
    }
}