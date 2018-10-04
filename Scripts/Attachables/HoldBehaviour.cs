using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.Attachables
{
    public enum HoldBehaviour
    {
        /// <summary>
        /// Child rigidbody will be set to IsKinematic and it's transform
        /// position/rotation will be set every Update.
        /// </summary>
        UpdateLock,

        /// <summary>
        /// Child rigidbody will be removed and made an actual child
        /// of the parent in the hiearchy.
        /// </summary>
        Child,
    }
}