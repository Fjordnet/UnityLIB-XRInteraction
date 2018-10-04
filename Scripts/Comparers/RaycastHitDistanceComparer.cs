using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.Comparers
{
    public class RaycastHitDistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }
}