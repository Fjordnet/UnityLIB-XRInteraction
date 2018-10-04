using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRInteractors;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Events;

namespace Fjord.XRInteraction.XRUnityEvents
{
    [System.Serializable]
    public class XRInteractionUnityEvent : UnityEvent<XRInteractionEventReceiver>
    {
    }
}