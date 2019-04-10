using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Enums;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Implement this class to create a visualizer. A visualizer placed next to an Interactor will serve as the global
    /// visualizer for that Interactor. A visualizer placed next to a Interaction will serve to override the global
    /// visualizer when an interactor is interfacing with that specific interaction.
    /// </summary>
    public abstract class XRVisualizer : MonoBehaviour
    {
        public virtual void Empty(XRPhysicsInteractor interactor)
        {}
        
        public virtual void Enter(XRPhysicsInteractor interactor)
        {}

        public virtual void Stay(XRPhysicsInteractor interactor)
        {}
        
        public virtual void Exit(XRPhysicsInteractor interactor)
        {}

        public virtual void ButtonDown(XRButtonDatum datum)
        {}
        
        public virtual void ButtonHold(XRButtonDatum datum)
        {}
        
        public virtual void ButtonUp(XRButtonDatum datum)
        {}

        public virtual void Show(XRPhysicsInteractor interactor)
        {}
        
        public virtual void Hide()
        {}
    }
}