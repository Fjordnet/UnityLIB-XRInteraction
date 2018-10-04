using System;
using Fjord.XRInteraction.Comparers;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Sends XRInput events from a raycast "Laser".
    /// </summary>
    public class XRLaserInteractor : XRPhysicsInteractor
    {
        protected readonly RaycastHit[] _recyclableHits = new RaycastHit[SearchCacheSize];
        protected readonly RaycastHitDistanceComparer _raycastHitDistanceComparer = new RaycastHitDistanceComparer();

        protected override Collider Raycast()
        {
            Collider hitCollider = null;
            CurrentSourceRay = new Ray(transform.position, transform.forward);

            int hitCount = Physics.RaycastNonAlloc(
                CurrentSourceRay,
                _recyclableHits,
                _searchDistance,
                _interactionLayerMask);

            if (hitCount > 0)
            {
                Array.Sort(_recyclableHits, 0, hitCount, _raycastHitDistanceComparer);
                RaycastHit nearestHit = _recyclableHits[0];

//                hitGameObject = null != nearestHit.rigidbody
//                    ? nearestHit.rigidbody.gameObject
//                    : nearestHit.collider.gameObject;

                hitCollider = nearestHit.collider;

//                if (!ContainsNecessaryTags(hitGameObject))
//                {
//                    hitGameObject = null;
//                }
                
                CurrentColliderDistance = Vector3.Distance(
                    nearestHit.point,
                    transform.position);
                CurrentHitDistance = nearestHit.distance;
                CurrentHitRay = new Ray(nearestHit.point, nearestHit.normal);
            }
            else 
            {
                Vector3 position = CurrentSourceRay.GetPoint(_searchDistance);
                CurrentHitDistance = _searchDistance;
                CurrentHitDistance = 0;
                CurrentHitRay = new Ray(position, -CurrentSourceRay.direction);
            }

            return hitCollider;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawRay(transform.position, transform.forward / 4);

            foreach (var buttonDatum in ButtonDatums)
            {
                if (null != buttonDatum.Value.PressCollider)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(buttonDatum.Value.PressColliderPositionChildedToController, .02f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere((buttonDatum.Value).RayHitChildedToController.origin, .02f);
                }
            }
        }
    }
}