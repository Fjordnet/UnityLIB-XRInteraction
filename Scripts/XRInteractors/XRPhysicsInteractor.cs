﻿using System.Collections.Generic;
using System.Linq;
using Fjord.Common.Attributes;
using Fjord.Common.Enums;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUnityEvents;
using Fjord.XRInteraction.XRUser;
using Fjord.Common.Extensions;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Base class which searches for Interactions to send events to.
    /// </summary>
    public abstract class XRPhysicsInteractor : XRInteractor
    {
        [Header("---- Physics Interactor")]
        [Header("Layers which this will interact with.")]
        [SerializeField]
        protected LayerMask _interactionLayerMask;

        [Header("Tags which this will interact with.")]
        [SerializeField]
        private TagMask _tagMask;

        [Header("Forward events from these buttons.")]
        [EnumFlag]
        [SerializeField]
        private XRInputName _forwardButtonEvents;

        [Header("Distance interactor will search for a reciever.")]
        [SerializeField]
        protected float _searchDistance = 40;

        [SerializeField]
        [Header("Haptic bump on enter/exit/press/release.")]
        private bool _enterExitPressReleaseHaptics = true;

        [SerializeField]
        [Header("Haptic tick when dragging GameObject.")]
        private bool _dragHaptics = true;

        [Header("Interactor Events")]
        [SerializeField]
        protected XRInteractorEvents _events = new XRInteractorEvents();

        [SerializeField]
        private float _dragTickSteps = .8f;

        protected const int SearchCacheSize = 8;
        protected readonly Dictionary<int, XRButtonDatum> ButtonDatums = new Dictionary<int, XRButtonDatum>();
        private readonly List<XRButtonDatum> _heldButtons = new List<XRButtonDatum>();

        private readonly List<XRInteractionEventReceiver> _recyclableInteractions =
            new List<XRInteractionEventReceiver>();

        private float _currentColliderDistance;
        private Collider _currentCollider;
        private Ray _currentSourceRay;
        private Ray _currentHitRay;
        private float _currentHitDistance;
        private float _accumulatedDelta;

        /// <summary>
        /// The GameObject this is currently hovering over.
        /// </summary>
        public Collider CurrentCollider
        {
            get { return _currentCollider; }
            protected set
            {
                PriorCollider = _currentCollider;
                _currentCollider = value;
            }
        }

        /// <summary>
        /// The last GameObject this is currently hovering over.
        /// </summary>
        public Collider PriorCollider { get; private set; }

        /// <summary>
        /// The distance to the current center pivot of current GameObject.
        /// </summary>
        public float CurrentColliderDistance
        {
            get { return _currentColliderDistance; }
            protected set
            {
                PriorColliderDistance = _currentColliderDistance;
                _currentColliderDistance = value;
                ColliderDistanceDelta = _currentColliderDistance - PriorColliderDistance;
            }
        }

        private float PriorColliderDistance { get; set; }
        public float ColliderDistanceDelta { get; private set; }

        /// <summary>
        /// Ray which represents the point from which interactor started raycasting.
        /// </summary>
        public Ray CurrentSourceRay
        {
            get { return _currentSourceRay; }
            protected set
            {
                PriorSourceRay = _currentSourceRay;
                _currentSourceRay = value;
                SourcePositionDelta = _currentSourceRay.origin - PriorSourceRay.origin;
            }
        }

        private Ray PriorSourceRay { get; set; }
        public Vector3 SourcePositionDelta { get; private set; }

        /// <summary>
        /// Ray which represents where interactor intersected an object.
        /// </summary>
        public Ray CurrentHitRay
        {
            get { return _currentHitRay; }
            protected set
            {
                PriorHitRay = _currentHitRay;
                _currentHitRay = value;
                HitPositionDelta = _currentHitRay.origin - PriorHitRay.origin;
            }
        }

        private Ray PriorHitRay { get; set; }
        public Vector3 HitPositionDelta { get; private set; }

        /// <summary>
        /// Distance from interactor to intersection.
        /// </summary>
        public float CurrentHitDistance
        {
            get { return _currentHitDistance; }
            protected set
            {
                PriorHitDistance = _currentHitDistance;
                _currentHitDistance = value;
                HitDistanceDelta = _currentHitDistance - PriorHitDistance;
            }
        }

        private float PriorHitDistance { get; set; }
        public float HitDistanceDelta { get; private set; }

        /// <summary>
        /// Number of buttons currently held down.
        /// </summary>
        public int ButtonHeldCount { get; private set; }

        public XRInteractorEvents Events
        {
            get { return _events; }
        }

        protected void Reset()
        {
            _forwardButtonEvents = 0 << -1;
            _interactionLayerMask = LayerMask.GetMask("Default");
        }

        protected virtual void OnDisable()
        {
            if (CurrentCollider != null)
            {
                SendExitEvents(CurrentCollider);
            }

            CurrentSourceRay = new Ray(transform.position, transform.forward);
            CurrentHitRay = new Ray(transform.position, transform.forward);
            CurrentHitDistance = 0;
            CurrentColliderDistance = 0;
            CurrentCollider = null;
            foreach (var datum in ButtonDatums)
            {
                if (null != datum.Value.PressCollider)
                {
                    OnButtonUp(datum.Value.InputName);
                }
            }
        }

        protected override void Start()
        {
            foreach (XRInputName inputName in _forwardButtonEvents.GetFlags())
            {
                ParentUserController.GetInput(inputName).ButtonDown += OnButtonDown;
                ParentUserController.GetInput(inputName).ButtonHold += OnButtonHold;
                ParentUserController.GetInput(inputName).ButtonUp += OnButtonUp;
                ButtonDatums.Add((int)inputName, new XRButtonDatum(ParentUserController, this, inputName));
            }

            base.Start();
        }

        /// <summary>
        /// Called from XRController in order to control exectuion order of raycasting and button events.
        /// </summary>
        internal override void Process()
        {
            Collider hitCollider = Raycast();
            ProcessEnterStayExit(hitCollider);
        }

        /// <summary>
        /// Is this Interactor currently pressed on any GameObjects?
        /// </summary>
        public bool IsPressingOnGameObject()
        {
            foreach (var datum in ButtonDatums)
            {
                if (null != datum.Value.PressCollider)
                {
                    return true;
                }
            }

            return false;
        }

        public XRButtonDatum GetButtonDatum(XRInputName inputName)
        {
            return ButtonDatums.GetValue((int)inputName);
        }

        public bool CanInteractWithCurrentGameObject()
        {
            if (null == _currentCollider)
            {
                return false;
            }
            else
            {
                return ContainsNecessaryTags(_currentCollider) &&
                       HasInteractionReciever(_currentCollider);
            }
        }

        public bool ContainsNecessaryTags(Collider targetCollider)
        {
            GetInteractions(targetCollider, _recyclableInteractions);
            for (int i = 0; i < _recyclableInteractions.Count; ++i)
            {
                if (_tagMask.CompareTags(_recyclableInteractions[i].gameObject))
                {
                    return true;
                }
            }

            return false;
        }

        protected abstract Collider Raycast();

        private void ProcessEnterStayExit(Collider hitCollider)
        {
            if (hitCollider != CurrentCollider)
            {
                if (null != CurrentCollider)
                {
                    SendExitEvents(CurrentCollider);
                    _events.Exit.Invoke(this);
                }

                CurrentCollider = hitCollider;

                if (null != CurrentCollider)
                {
                    SendEnterEvents(CurrentCollider);
                    _events.Enter.Invoke(this);
                }
            }
            else if (null != CurrentCollider)
            {
                SendStayEvents(CurrentCollider);
                _events.Stay.Invoke(this);
            }
        }

        private void OnButtonDown(XRInputName inputName)
        {
            ButtonHeldCount++;
            XRButtonDatum datum = GetButtonDatum(inputName);
            _heldButtons.Add(datum);

            if (CanInteractWithCurrentGameObject())
            {
                datum.ButtonDown(CurrentCollider, CurrentHitRay);
                SendButtonDownEvents(CurrentCollider, datum);
            }

            if (ButtonHeldCount == 1)
            {
                _events.FirstButtonDown.Invoke(datum);
            }

            _events.ButtonDown.Invoke(datum);
        }

        private void OnButtonHold(XRInputName inputName)
        {
            XRButtonDatum datum = GetButtonDatum(inputName);
            if (null != datum.PressCollider)
            {
                datum.ButtonHold();
                SendButtonHoldEvents(datum.PressCollider, datum);
            }

            _events.ButtonHold.Invoke(datum);
        }

        private void OnButtonUp(XRInputName inputName)
        {
            //button up may get fired when this interactor is disabled
            //so which buttons are held, and have been released
            //must be checked so UnButtonUp does not get called twice
            XRButtonDatum datum = GetButtonDatum(inputName);
            if (_heldButtons.Contains(datum))
            {
                _heldButtons.Remove(datum);
                ButtonHeldCount--;

                if (null != datum.PressCollider)
                {
                    SendButtonUpEvents(datum.PressCollider, datum);
                    datum.ButtonUp();
                }

                if (ButtonHeldCount == 0)
                {
                    _events.LastButtonUp.Invoke(datum);
                }

                _events.ButtonUp.Invoke(datum);
            }
        }

        private void SendExitEvents(Collider targetCollider)
        {
            GetInteractions(targetCollider, _recyclableInteractions);
            bool eventsSent = false;
            for (int i = 0; i < _recyclableInteractions.Count; ++i)
            {
                if (_recyclableInteractions[i].enabled &&
                    _tagMask.CompareTags(_recyclableInteractions[i].gameObject))
                {
                    _recyclableInteractions[i].OnExit(this);
                    eventsSent = true;
                }
            }

            if (eventsSent)
            {
                if (ButtonHeldCount == 0 && _enterExitPressReleaseHaptics)
                {
                    ParentUserController.FireHapticKey("Exit");
                }

                if (_soloCondition == SoloCondition.InteractorHoveredOverInteraction)
                {
                    ParentUserController.SetSoloState(true, this);
                }
            }
        }

        private void SendEnterEvents(Collider targetCollider)
        {
            GetInteractions(targetCollider, _recyclableInteractions);
            bool eventsSent = false;
            for (int i = 0; i < _recyclableInteractions.Count; ++i)
            {
                if (_recyclableInteractions[i].enabled &&
                    _tagMask.CompareTags(_recyclableInteractions[i].gameObject))
                {
                    _recyclableInteractions[i].OnEnter(this);
                    eventsSent = true;
                }
            }

            if (eventsSent)
            {
                if (ButtonHeldCount == 0 && _enterExitPressReleaseHaptics)
                {
                    ParentUserController.FireHapticKey("Enter");
                }

                if (_soloCondition == SoloCondition.InteractorHoveredOverInteraction)
                {
                    ParentUserController.SetSoloState(false, this);
                }
            }
        }

        private void SendStayEvents(Collider targetCollider)
        {
            GetInteractions(targetCollider, _recyclableInteractions);
            for (int i = 0; i < _recyclableInteractions.Count; ++i)
            {
                if (_recyclableInteractions[i].enabled &&
                    _tagMask.CompareTags(_recyclableInteractions[i].gameObject))
                {
                    _recyclableInteractions[i].OnStay(this);
                }
            }
        }

        private void SendButtonDownEvents(Collider targetCollider, XRButtonDatum datum)
        {
            GetInteractions(targetCollider, _recyclableInteractions);
            bool pressOccured = false;
            for (int i = 0; i < _recyclableInteractions.Count; ++i)
            {
                if (_recyclableInteractions[i].enabled &&
                    _recyclableInteractions[i].RecievesInput(datum.InputName) &&
                    _recyclableInteractions[i].HoldAvailable() &&
                    _tagMask.CompareTags(_recyclableInteractions[i].gameObject))
                {
                    _recyclableInteractions[i].OnButtonDown(datum);
                    pressOccured = true;
                }
            }

            if (pressOccured)
            {
                if (_enterExitPressReleaseHaptics) ParentUserController.FireHapticKey("Press");

                if (ButtonHeldCount == 1 && _soloCondition == SoloCondition.InteractorPressedOnInteraction)
                {
                    ParentUserController.SetSoloState(false, this);
                }
            }
        }

        private void SendButtonHoldEvents(Collider targetCollider, XRButtonDatum datum)
        {
            GetInteractions(targetCollider, _recyclableInteractions);
            bool holdOccured = false;
            for (int i = 0; i < _recyclableInteractions.Count; ++i)
            {
                if (_recyclableInteractions[i].enabled &&
                    _recyclableInteractions[i].RecievesInput(datum.InputName) &&
                    _tagMask.CompareTags(_recyclableInteractions[i].gameObject))
                {
                    _recyclableInteractions[i].OnButtonHold(datum);
                    holdOccured = true;
                }
            }

            if (holdOccured && _dragHaptics)
            {
                _accumulatedDelta += datum.PressColliderPositionChildedToControllerDelta.magnitude;
                if (_accumulatedDelta > _dragTickSteps)
                {
                    _accumulatedDelta = 0;
                    ParentUserController.FireHapticKey("DragTick");
                }
            }
        }

        private void SendButtonUpEvents(Collider targetCollider, XRButtonDatum datum)
        {
            GetInteractions(targetCollider, _recyclableInteractions);
            bool releaseOccured = false;
            for (int i = 0; i < _recyclableInteractions.Count; ++i)
            {
                if (_recyclableInteractions[i].enabled &&
                    _recyclableInteractions[i].RecievesInput(datum.InputName) &&
                    _tagMask.CompareTags(_recyclableInteractions[i].gameObject))
                {
                    _recyclableInteractions[i].OnButtonUp(datum);
                    releaseOccured = true;
                }
            }

            if (releaseOccured)
            {
                if (_enterExitPressReleaseHaptics) ParentUserController.FireHapticKey("Release");

                if (ButtonHeldCount == 0 && _soloCondition == SoloCondition.InteractorPressedOnInteraction)
                {
                    ParentUserController.SetSoloState(true, this);
                }
            }
        }

        private void GetInteractions(Collider targetCollider, List<XRInteractionEventReceiver> recyclableInteractions)
        {
            targetCollider.GetComponents(recyclableInteractions);
            if (recyclableInteractions.Count == 0 && null != targetCollider.attachedRigidbody)
            {
                targetCollider.attachedRigidbody.GetComponents(recyclableInteractions);
            }
        }

        private bool HasInteractionReciever(Collider targetCollider)
        {
            GetInteractions(targetCollider, _recyclableInteractions);
            for (int i = 0; i < _recyclableInteractions.Count; ++i)
            {
                if (_recyclableInteractions[i].enabled)
                {
                    return true;
                }
            }
            return false;
        }
    }
}