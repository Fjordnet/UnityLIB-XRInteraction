using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fjord.XRInteraction.XRUI
{
    public class XRPointerEventData : PointerEventData
    {
        public Ray WorldSourceRay { get; set; }

        public XRPointerEventData(EventSystem eventSystem) : base(eventSystem)
        { }
    }
}
