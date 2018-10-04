using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Types;
using Fjord.Common.Enums;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Visualizes a XRFlexLaserInteractor.
    /// </summary>
    [RequireComponent(typeof(XRFlexLaserInteractor))]
    [RequireComponent(typeof(LineRenderer))]
    public class XRFlexLaserVisualizer : MonoBehaviour
    {
        [SerializeField]
        private float _lineScrollMultiplier = 4;

        private XRFlexLaserInteractor _flexLaserInteractor;
        private LineRenderer _lineRenderer;
        private Material _material;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _flexLaserInteractor = GetComponent<XRFlexLaserInteractor>();
            _material = _lineRenderer.material;

            if (null == _flexLaserInteractor)
            {
                Debug.LogWarning("No LaserInteractor specified on " + name);
            }
        }

        private void LateUpdate()
        {
            _lineRenderer.enabled = _flexLaserInteractor.enabled;

            Vector2 newOffset = _material.mainTextureOffset;
            newOffset.x -= Time.deltaTime * _lineScrollMultiplier;
            _material.mainTextureOffset = newOffset;
            
            if (!_flexLaserInteractor.enabled)
            {
                return;
            }

            _lineRenderer.positionCount  = _flexLaserInteractor.StepCount;
            for (int i = 0; i < _flexLaserInteractor.StepCount; ++i)
            {
                _lineRenderer.SetPosition(i, _flexLaserInteractor.StepPoints[i]);
            }
        }
    }
}