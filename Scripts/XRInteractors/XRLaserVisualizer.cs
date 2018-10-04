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
    public class XRLaserVisualizer : MonoBehaviour
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
        private CubicBezierSegment _bezierSegment;
        private HoverState _hoverState;
        private XRLaserInteractor _laserInteractor;
        private GameObject _hitTargetInstance;
        private Material _hitTargetMaterial;

        private void Awake()
        {
            _hitTargetInstance = Instantiate(_hitTargetPrefab, gameObject.transform);
            _hitTargetMaterial = _hitTargetInstance.GetComponent<MeshRenderer>().material;

            _lineRenderer = GetComponent<LineRenderer>();
            _laserInteractor = GetComponent<XRLaserInteractor>();

            if (null == _laserInteractor)
            {
                Debug.LogWarning("No LaserInteractor specified on " + name);
            }

            _hoverState = HoverState.Empty;
            _lineRenderer.startColor = _emptyStartColor;
            _lineRenderer.endColor = _emptyEndColor;
            _hitTargetMaterial.color = _emptyEndColor;
        }

        private void LateUpdate()
        {
            //TODO change to use event callbacks

            if (!_laserInteractor.enabled || !_laserInteractor.gameObject.activeSelf)
            {
                _lineRenderer.enabled = false;
                _hitTargetInstance.gameObject.SetActive(false);
                return;
            }
            else
            {
                _lineRenderer.enabled = true;
                _hitTargetInstance.gameObject.SetActive(true);
            }

            XRButtonDatum buttonDatum = _laserInteractor.GetButtonDatum(_visualizeButtonPressHitRay);

            if (null == buttonDatum)
            {
                Debug.LogWarning("Button specified on " + name + " not found in it's specified LaserInteractor.");
                return;
            }

            if (null == buttonDatum.PressCollider)
            {
                if (_lineRenderer.positionCount != 2)
                {
                    _lineRenderer.positionCount = 2;
                }
                _lineRenderer.SetPosition(0, _laserInteractor.CurrentSourceRay.origin);
                _lineRenderer.SetPosition(1, _laserInteractor.CurrentHitRay.origin);

                if (!_laserInteractor.CanInteractWithCurrentGameObject() && _hoverState != HoverState.Empty)
                {
                    _hoverState = HoverState.Empty;
                    _lineRenderer.startColor = _emptyStartColor;
                    _lineRenderer.endColor = _emptyEndColor;
                    _hitTargetMaterial.color = _emptyEndColor;
                }
                else if (_laserInteractor.CanInteractWithCurrentGameObject() && _hoverState != HoverState.Hover)
                {
                    _hoverState = HoverState.Hover;
                    _lineRenderer.startColor = _hoverStartColor;
                    _lineRenderer.endColor = _hoverEndColor;
                    _hitTargetMaterial.color = _hoverEndColor;
                }
                
                _hitTargetInstance.transform.localScale = new Vector3(
                    _hitTargetScale,
                    _hitTargetScale,
                    _hitTargetScale);
                _hitTargetInstance.transform.position =  _laserInteractor.CurrentHitRay.origin;
                _hitTargetInstance.transform.up =  _laserInteractor.CurrentHitRay.direction;
            }
            else
            {
                if (_lineRenderer.positionCount != BezierSegmentCount)
                {
                    _lineRenderer.positionCount = BezierSegmentCount;
                }

                if (_hoverState != HoverState.Press)
                {
                    _hoverState = HoverState.Press;
                    _lineRenderer.startColor = _pressStartColor;
                    _lineRenderer.endColor = _pressEndColor;
                    _hitTargetMaterial.color = _pressEndColor;
                }

                float startEndDistance = Vector3.Distance(
                    _laserInteractor.CurrentSourceRay.origin,
                    buttonDatum.RayHitChildedToPressGameObject.origin);
                float handleDistance = startEndDistance / 4f;

                Ray hitRay = new Ray();

                if (_aimHitRayAtInteractor)
                {
                    hitRay.origin = buttonDatum.RayHitChildedToPressGameObject.origin;
                    hitRay.direction = buttonDatum.RayHitChildedToPressGameObject.origin - _laserInteractor.transform.position;
                }
                else
                {
                    hitRay.origin = buttonDatum.RayHitChildedToPressGameObject.origin;
                    hitRay.direction = buttonDatum.RayHitChildedToPressGameObject.direction;
                }

                _hitTargetInstance.transform.localScale = new Vector3(
                    _hitTargetScale * 1.5f,
                    _hitTargetScale * 1.5f,
                    _hitTargetScale * 1.5f);
                _hitTargetInstance.transform.position = hitRay.origin;
                _hitTargetInstance.transform.up = buttonDatum.RayHitChildedToPressGameObject.direction;

                CubicBezierSegment bezierSegment = new CubicBezierSegment(
                    _laserInteractor.CurrentSourceRay.origin,
                    _laserInteractor.CurrentSourceRay.GetPoint(handleDistance),
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
    }
}