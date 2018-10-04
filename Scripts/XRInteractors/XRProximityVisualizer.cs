using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Types;
using Fjord.Common.Enums;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Visualizes a XRLaserInteractor.
    /// </summary>
    [RequireComponent(typeof(XRProximityInteractor))]
    public class XRProximityVisualizer : MonoBehaviour
    {
        [Header("Visualize the Hit Ray of this button event.")]
        [SerializeField]
        private XRInputName _visualizeButtonPressHitRay = XRInputName.TriggerButton;

        [SerializeField]
        private GameObject _hitTargetPrefab;

        [SerializeField]
        private float _hitTargetScale = .2f;

        [SerializeField]
        private Color _hoverStartColor;

        [SerializeField]
        private Color _hoverEndColor;

        [SerializeField]
        private Color _pressStartColor;

        [SerializeField]
        private Color _pressEndColor;

        private HoverState _hoverState;
        private XRProximityInteractor _proximityInteractor;
        private GameObject _hitTargetInstance;
        private Material _hitTargetMaterial;

        private void Awake()
        {
            _hitTargetInstance = Instantiate(_hitTargetPrefab);
            _hitTargetMaterial = _hitTargetInstance.GetComponent<MeshRenderer>().material;
            _hitTargetMaterial.color = _hoverEndColor;
            _hitTargetInstance.gameObject.SetActive(false);
            _hitTargetInstance.transform.localScale = new Vector3(
                _hitTargetScale,
                _hitTargetScale,
                _hitTargetScale);
            
            _proximityInteractor = GetComponent<XRProximityInteractor>();
            _proximityInteractor.Events.Enter.AddListener(Enter);
            _proximityInteractor.Events.Stay.AddListener(Stay);
            _proximityInteractor.Events.Exit.AddListener(Exit);
            _proximityInteractor.Events.ButtonDown.AddListener(ButtonDown);
            _proximityInteractor.Events.ButtonHold.AddListener(ButtonHold);
            _proximityInteractor.Events.ButtonUp.AddListener(ButtonUp);

            if (null == _proximityInteractor)
            {
                Debug.LogWarning("No LaserInteractor specified on " + name);
            }
        }

        private void Enter(XRPhysicsInteractor interactor)
        {
            _hitTargetInstance.gameObject.SetActive(true);
        }

        private void Stay(XRPhysicsInteractor interactor)
        {
            if (_hoverState != HoverState.Press)
            {
                _hitTargetInstance.transform.position = interactor.CurrentHitRay.origin;
                _hitTargetInstance.transform.up = interactor.CurrentHitRay.direction;
            }
        }
        
        private void Exit(XRPhysicsInteractor interactor)
        {
            if (_hoverState != HoverState.Press)
            {
                _hitTargetInstance.gameObject.SetActive(false);
            }
        }

        private void ButtonDown(XRButtonDatum datum)
        {
            if (datum.InputName == _visualizeButtonPressHitRay && datum.PressCollider != null)
            {
                _hoverState = HoverState.Press;
                _hitTargetMaterial.color = _pressEndColor;
                _hitTargetInstance.transform.localScale = new Vector3(
                    _hitTargetScale * 1.5f,
                    _hitTargetScale * 1.5f,
                    _hitTargetScale * 1.5f);
            }
        }
        
        private void ButtonHold(XRButtonDatum datum)
        {
            if (datum.InputName == _visualizeButtonPressHitRay && _hoverState == HoverState.Press)
            {
                _hitTargetInstance.transform.position = datum.RayHitChildedToPressGameObject.origin;
                _hitTargetInstance.transform.up = datum.RayHitChildedToPressGameObject.direction;
            }
        }
        
        private void ButtonUp(XRButtonDatum datum)
        {
            if (datum.InputName == _visualizeButtonPressHitRay && _hoverState == HoverState.Press)
            {
                _hoverState = HoverState.Hover;
                _hitTargetMaterial.color = _hoverEndColor;
                _hitTargetInstance.transform.localScale = new Vector3(
                    _hitTargetScale,
                    _hitTargetScale,
                    _hitTargetScale);
            }
        }
    }
}