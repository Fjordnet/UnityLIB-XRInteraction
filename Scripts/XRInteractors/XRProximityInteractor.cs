using System;
using Fjord.XRInteraction.Comparers;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Sends XRInput events based on proximity via OverlapSphere.
    /// </summary>
    public class XRProximityInteractor : XRPhysicsInteractor
    {
        private readonly Collider[] _recyclableColliders = new Collider[SearchCacheSize];
        private readonly ColliderDistanceComparer _colliderDistanceComparer = new ColliderDistanceComparer();

        protected override Collider Raycast()
        {
            Collider hitCollider = null;

            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                _searchDistance,
                _recyclableColliders,
                _interactionLayerMask);

            if (hitCount > 0)
            {
                _colliderDistanceComparer.FromPoint = transform.position;
                Array.Sort(_recyclableColliders, 0, hitCount, _colliderDistanceComparer);
                Collider nearestCollider = ColliderWithInteractions(_recyclableColliders, hitCount);

                if (null != nearestCollider)
                {
//                    hitGameObject = null != nearestCollider.attachedRigidbody
//                        ? nearestCollider.attachedRigidbody.gameObject
//                        : nearestCollider.gameObject;

                    hitCollider = nearestCollider;

                    CurrentColliderDistance = Vector3.Distance(
                        hitCollider.transform.position,
                        transform.position);

                    //When outside the collider, aim sourceRay to closest point on collider.
                    //Note: closestPoint returns the point of the interactor when inside collider
                    Vector3 hitPoint = nearestCollider.ClosestPoint(transform.position);

                    //right on the very edge of intersecting with Collider ClosestPoint in 2017.3
                    //will return nearestCollider origin, so test for if it is returing that, and force
                    //hitpoint to be the position of the interactor. This may just be a unitybug in 2017.3
                    Vector3 hitGameObjectRay = (hitPoint - nearestCollider.transform.position);
                    if (hitGameObjectRay.sqrMagnitude < .00001f)
                    {
                        hitPoint = transform.position;
                    }
                    
                    Vector3 sourceRay = (hitPoint - transform.position).normalized;
                    
                    //sqrMag == 0 means interactor is inside collider, aim sourceRay to center of collider.
                    if (sourceRay.sqrMagnitude == 0)
                    {
                        sourceRay = (hitCollider.transform.position - transform.position).normalized;
                    }

                    CurrentHitDistance = Vector3.Distance(transform.position, hitPoint);
                    CurrentHitRay = new Ray(hitPoint, -sourceRay);
                    CurrentSourceRay = new Ray(transform.position, sourceRay);
                }
            }

            if (null == hitCollider)
            {
                CurrentColliderDistance = 0;
                CurrentHitDistance = 0;
                CurrentHitDistance = 0;
                CurrentHitRay = new Ray(transform.position, -transform.forward);
                CurrentSourceRay = new Ray(transform.position, transform.forward);
            }

            return hitCollider;
        }

        private Collider ColliderWithInteractions(Collider[] colliders, int hitCount)
        {
            for (int i = 0; i < hitCount; ++i)
            {
                if (ContainsNecessaryTags(colliders[i]))
                {
                    return colliders[i];
                }
            }

            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, _searchDistance);

            foreach (var buttonDatum in ButtonDatums)
            {
                if (null != buttonDatum.Value.PressCollider)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(buttonDatum.Value.PressColliderPositionChildedToController, .02f);
                }
            }
        }
    }
}