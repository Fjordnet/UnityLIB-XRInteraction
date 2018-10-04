using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.Attachables
{
    /// <summary>
    /// Represents an physics volume in code, giving events for colliders which enter and exit it.
    /// This essentially gives you the behaviour of OnTriggerEnter/Exit without needing to have
    /// an actual collider in the world.
    /// </summary>
    [Serializable]
    public class
        PhysicsVolume //TODO can this be made generic so the lists are stored as a type? Would that even be more valuable?
    {
        public event Action<Collider> Entered;
        public event Action<Collider> Exited;

        private Collider[] _recyclableColliderArray = new Collider[128];
        private List<Collider> _entered = new List<Collider>();

        public void TestForIntersections(Vector3 sourcePoint, float scanDistance)
        {
            Debug.DrawRay(sourcePoint, Vector3.down, Color.magenta);

            //the following more code is more complex than typical because it is done
            //in a way to produces no allocations
            int hitCount = Physics.OverlapSphereNonAlloc(
                sourcePoint,
                scanDistance,
                _recyclableColliderArray,
                1,
                QueryTriggerInteraction.Collide);
            if (hitCount > 0)
            {
                for (int i = _entered.Count - 1; i > -1; --i)
                {
                    bool contains = false;
                    for (int j = 0; j < hitCount; ++j)
                    {
                        if (_entered[i] == _recyclableColliderArray[j]) contains = true;
                    }
                    if (!contains)
                    {
                        Collider collider = _entered[i];
                        _entered.RemoveAt(i);

                        if (Exited != null) Exited(collider);
                    }
                }

                for (int i = 0; i < hitCount; ++i)
                {
                    if (!_entered.Contains(_recyclableColliderArray[i]))
                    {
                        _entered.Add(_recyclableColliderArray[i]);
                        if (Entered != null) Entered(_recyclableColliderArray[i]);
                    }
                }
            }
            else
            {
                for (int i = _entered.Count - 1; i > -1; --i)
                {
                    Collider collider = _entered[i];
                    _entered.RemoveAt(i);
                    if (Exited != null) Exited(collider);
                }
            }
        }
    }
}