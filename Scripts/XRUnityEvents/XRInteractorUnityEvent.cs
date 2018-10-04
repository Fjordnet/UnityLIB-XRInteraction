using Fjord.XRInteraction.XRInteractors;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Events;

namespace Fjord.XRInteraction.XRUnityEvents
{
    [System.Serializable]
    public class XRInteractorUnityEvent : UnityEvent<XRPhysicsInteractor>
    { }
}