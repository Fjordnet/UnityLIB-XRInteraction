using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Enums;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Visualizes a XRLaserInteractor.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(XRLaserVisualizer))]
    public class XRLaserVisualizer : XRVisualizer
    {
        [Header("Visualize the Hit Ray of this button event.")]
        [SerializeField]
        private XRInputName _visualizeButtonPressHitRay = XRInputName.TriggerButton;

        [SerializeField]
        private GameObject _hitTargetPrefab;

        [SerializeField]
        private float _hitTargetScale = .2f;

        [SerializeField]
        private Color _emptyStartColor;

        [SerializeField]
        private Color _emptyEndColor;

        [SerializeField]
        private Color _hoverStartColor;

        [SerializeField]
        private Color _hoverEndColor;

        [SerializeField]
        private Color _pressStartColor;

        [SerializeField]
        private Color _pressEndColor;

        [SerializeField]
        private bool _aimHitRayAtInteractor;

        private const int BezierSegmentCount = 32;
        private LineRenderer _lineRenderer;
        private HoverState _hoverState;
        private GameObject _hitTargetInstance;
        private Material _hitTargetMaterial;

        private void Awake()
        {
            _hitTargetInstance = Instantiate(_hitTargetPrefab, gameObject.transform);
            _hitTargetMaterial = _hitTargetInstance.GetComponent<MeshRenderer>().material;

            _lineRenderer = GetComponent<LineRenderer>();
            
            _hoverState = HoverState.Empty;
            _lineRenderer.startColor = _emptyStartColor;
            _lineRenderer.endColor = _emptyEndColor;
            _hitTargetMaterial.color = _emptyEndColor;
            _hitTargetInstance.transform.localScale = new Vector3(
                _hitTargetScale,
                _hitTargetScale,
                _hitTargetScale);
        }

        public override void Enter(XRPhysicsInteractor interactor)
        {
            if (interactor.CanInteractWithCurrentGameObject() && _hoverState != HoverState.Press)
            {
                _hoverState = HoverState.Hover;
                _lineRenderer.startColor = _hoverStartColor;
                _lineRenderer.endColor = _hoverEndColor;
                _hitTargetMaterial.color = _hoverEndColor;
            }
        }

         public override void Stay(XRPhysicsInteractor interactor)
        {
            if (_hoverState != HoverState.Press)
            {
                _hitTargetInstance.transform.position = interactor.CurrentHitRay.origin;
                _hitTargetInstance.transform.up = interactor.CurrentHitRay.direction;
                if (_lineRenderer.positionCount != 2)
                {
                    _lineRenderer.positionCount = 2;
                }
                _lineRenderer.SetPosition(0, interactor.CurrentSourceRay.origin);
                _lineRenderer.SetPosition(1, interactor.CurrentHitRay.origin);

                _hitTargetInstance.transform.position =  interactor.CurrentHitRay.origin;
                _hitTargetInstance.transform.up =  interactor.CurrentHitRay.direction;
            }
        }

         public override void Exit(XRPhysicsInteractor interactor)
        {
            if (interactor.CanInteractWithCurrentGameObject() && _hoverState != HoverState.Press)
            {
                _hoverState = HoverState.Empty;
                _lineRenderer.startColor = _emptyStartColor;
                _lineRenderer.endColor = _emptyEndColor;
                _hitTargetMaterial.color = _emptyEndColor;
            }
        }

         public override void ButtonDown(XRButtonDatum datum)
        {
            if (datum.InputName == _visualizeButtonPressHitRay && datum.PressCollider != null)
            {
                _hoverState = HoverState.Press;
                _lineRenderer.startColor = _pressStartColor;
                _lineRenderer.endColor = _pressEndColor;
                _hitTargetMaterial.color = _pressEndColor;
                _hitTargetInstance.transform.localScale = new Vector3(
                    _hitTargetScale * 1.5f,
                    _hitTargetScale * 1.5f,
                    _hitTargetScale * 1.5f);
                
                Ray hitRay = HitRay(datum.ParentInteractor);
                
                _hitTargetInstance.transform.position = hitRay.origin;
                _hitTargetInstance.transform.up = datum.RayHitChildedToPressGameObject.direction;
            }
        }

         public override void ButtonHold(XRButtonDatum datum)
        {
            if (datum.InputName == _visualizeButtonPressHitRay && _hoverState == HoverState.Press)
            {
                if (_lineRenderer.positionCount != BezierSegmentCount)
                {
                    _lineRenderer.positionCount = BezierSegmentCount;
                }
                
                Ray hitRay = HitRay(datum.ParentInteractor);
                
                float startEndDistance = Vector3.Distance(
                    datum.ParentInteractor.CurrentSourceRay.origin,
                    datum.RayHitChildedToPressGameObject.origin);
                float handleDistance = startEndDistance / 4f;
                
                _hitTargetInstance.transform.position = datum.RayHitChildedToPressGameObject.origin;
                _hitTargetInstance.transform.up = datum.RayHitChildedToPressGameObject.direction;
                
                CubicBezierSegment bezierSegment = new CubicBezierSegment(
                    datum.ParentInteractor.CurrentSourceRay.origin,
                    datum.ParentInteractor.CurrentSourceRay.GetPoint(handleDistance),
                    hitRay.GetPoint(handleDistance),
                    hitRay.origin);

                float step = 1f / BezierSegmentCount;
                float t = 0;
                for (int i = 0; i < BezierSegmentCount; ++i)
                {
                    _lineRenderer.SetPosition(i, bezierSegment.Point(t));
                    t += step;
                }
            }
        }

         public override void ButtonUp(XRButtonDatum datum)
        {
            if (datum.InputName == _visualizeButtonPressHitRay && _hoverState == HoverState.Press)
            {
                _hoverState = HoverState.Hover;
                _lineRenderer.startColor = _hoverStartColor;
                _lineRenderer.endColor = _hoverEndColor;
                _hitTargetMaterial.color = _hoverEndColor;
                _hitTargetInstance.transform.localScale = new Vector3(
                    _hitTargetScale,
                    _hitTargetScale,
                    _hitTargetScale);
            }
        }

         public override void Show()
         {
             _lineRenderer.enabled = true;
             _hitTargetInstance.gameObject.SetActive(true);
         }

         public override void Hide()
         {
             _lineRenderer.enabled = false;
             _hitTargetInstance.gameObject.SetActive(false);
         }

         private Ray HitRay(XRPhysicsInteractor interactor)
        {
            XRButtonDatum buttonDatum = interactor.GetButtonDatum(_visualizeButtonPressHitRay);
            
            Ray hitRay = new Ray();

            if (_aimHitRayAtInteractor)
            {
                hitRay.origin = buttonDatum.RayHitChildedToPressGameObject.origin;
                hitRay.direction = buttonDatum.RayHitChildedToPressGameObject.origin - interactor.transform.position;
            }
            else
            {
                hitRay.origin = buttonDatum.RayHitChildedToPressGameObject.origin;
                hitRay.direction = buttonDatum.RayHitChildedToPressGameObject.direction;
            }

            return hitRay;
        }
    }
}