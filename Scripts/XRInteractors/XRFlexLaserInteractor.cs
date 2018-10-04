using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;

namespace Fjord.XRInteraction.XRInteractors
{
    public class XRFlexLaserInteractor : XRLaserInteractor
    {
        [Header("---- Flex Laser Interactor")]
        [Header("How much to smooth the laser.")]
        [SerializeField]
        private float _laserDamp = .01f;

        [Header("How quickly should laser arc down.")]
        [SerializeField]
        private float _gravity = .1f;

        [Header("Length of each step on curve.")]
        [SerializeField]
        private float _raycastStepDistance = .1f;

        public int StepCount { get; private set; }
        public Vector3[] StepPoints { get; private set; }
        private Vector3[] _stepVeclocities;
        private Transform _stepTransform;
        private int _maxStepCount;
        private int _priorStepCount;
        private bool _firstCycle = true;

        private void OnEnable()
        {
            _firstCycle = true;
        }

        protected override void Awake()
        {
            base.Awake();
            _stepTransform = new GameObject("StepTransform").transform;
            _stepTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;
            _maxStepCount = (int)(_searchDistance / _raycastStepDistance);
            StepPoints = new Vector3[_maxStepCount];
            _stepVeclocities = new Vector3[_maxStepCount];
        }

        protected override Collider Raycast()
        {
            int stepCount = (int)(_searchDistance / _raycastStepDistance);
            int index = 0;
            float currentDistance = 0;
            Collider hitCollider = null;
            bool gameObjectHit = false;
            CurrentSourceRay = new Ray(transform.position, transform.forward);
            _stepTransform.position = transform.position;
            _stepTransform.forward = transform.forward;
            while (index < stepCount)
            {
                Ray ray = new Ray(_stepTransform.position, _stepTransform.forward);
                float raycastDistance = (currentDistance + _raycastStepDistance) > _searchDistance
                    ? _searchDistance - currentDistance
                    : _raycastStepDistance;

                int hitCount = Physics.RaycastNonAlloc(
                    ray,
                    _recyclableHits,
                    raycastDistance,
                    _interactionLayerMask);

                if (hitCount > 0)
                {
                    gameObjectHit = true;
                    Array.Sort(_recyclableHits, 0, hitCount, _raycastHitDistanceComparer);
                    RaycastHit nearestHit = _recyclableHits[0];

//                    hitGameObject = null != nearestHit.rigidbody
//                        ? nearestHit.rigidbody.gameObject
//                        : nearestHit.collider.gameObject;

                    hitCollider = nearestHit.collider;

//                    if (!ContainsNecessaryTags(hitGameObject))
//                    {
//                        hitGameObject = null;
//                    }
                    
                    currentDistance += nearestHit.distance;
                    CurrentHitDistance = currentDistance;
                    CurrentColliderDistance = Vector3.Distance(
                        nearestHit.point,
                        transform.position);
                    CurrentHitRay = new Ray(nearestHit.point, nearestHit.normal);

                    StepPoints[index] = nearestHit.point;
                    index++;
                    break;
                }

                currentDistance += raycastDistance;
                _stepTransform.Translate(0, 0, raycastDistance, Space.Self);
                _stepTransform.forward = Vector3.Lerp(_stepTransform.forward, Vector3.down, _gravity);

                if (index >= StepCount)
                {
                    _stepVeclocities[index] = Vector3.zero;
                    StepPoints[index] = _stepTransform.position;
                }
                else
                {
                    if (!_firstCycle)
                    {
                        _stepTransform.position = Vector3.SmoothDamp(
                            StepPoints[index],
                            _stepTransform.position,
                            ref _stepVeclocities[index],
                            _laserDamp);
                    }
                    StepPoints[index] = _stepTransform.position;
                }

                index++;
            }

            if (!gameObjectHit)
            {
                Vector3 position = CurrentSourceRay.GetPoint(_searchDistance);
                CurrentHitDistance = _searchDistance;
                CurrentHitDistance = 0;
                CurrentHitRay = new Ray(position, -CurrentSourceRay.direction);
            }

            StepCount = index;
            _firstCycle = false;
            
            return hitCollider;
        }
    }
}