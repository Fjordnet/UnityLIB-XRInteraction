using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Events;

namespace Fjord.XRInteraction.XRUnityEvents
{
    [System.Serializable]
    public class XRButtonDatumInteractionUnityEvent : UnityEvent<XRButtonDatum, XRInteractionEventReceiver>
    { }
}