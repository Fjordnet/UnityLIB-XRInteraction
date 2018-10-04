using System.Collections.ObjectModel;
using Fjord.XRInteraction.XRInteractors;
using Fjord.XRInteraction.XRUser;
using UnityEngine.Events;

namespace Fjord.XRInteraction.XRUnityEvents
{
    [System.Serializable]
    public class XRInteractorCollectionUnityEvent : UnityEvent<ReadOnlyCollection<XRPhysicsInteractor>>
    { }
}