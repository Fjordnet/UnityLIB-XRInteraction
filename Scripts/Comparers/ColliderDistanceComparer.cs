using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.Comparers
{
    public class ColliderDistanceComparer : IComparer<Collider>
    {
        public Vector3 FromPoint { get; set; }

        public int Compare(Collider x, Collider y)
        {
            return (x.transform.position - FromPoint).sqrMagnitude.CompareTo(
                (y.transform.position - FromPoint).sqrMagnitude);
        }
    }
}