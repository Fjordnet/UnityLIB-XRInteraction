using System;
using System.Collections.Generic;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRHaptics;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRInteractors;
using UnityEngine;
using UnityEngine.XR;

namespace Fjord.XRInteraction.XRUser
{
    /// <summary>
    /// Represents a single controller of the user in the environment.
    /// Groups multiple Interactors as children to describe how the controller
    /// should interact with the scene.
    /// </summary>
    public class XRUserController : MonoBehaviour
    {
        private readonly Dictionary<XRInputName, IXRInputProcessor> _inputProcessors
            = new Dictionary<XRInputName, IXRInputProcessor>();

        protected readonly List<XRPhysicsInteractor> _physicsInteractors = new List<XRPhysicsInteractor>();
        private XRNode _xrNode;
        private XRInputMap _inputMap;
        
        //The UnityUIInteractor is so idiosyncratic from the other 'Physics' interactors that it is dealt with
        //as a seperate reference and abstraction.
        public XRUnityUILaserInteractor UnityUIInteractor { get; protected set; }

        /// <summary>
        /// Is this associated with left or right hand?
        /// </summary>
        public Chirality ControllerChirality { get; private set; }

        /// <summary>
        /// The XRPlayerRoot which this controller belongs to.
        /// </summary>
        public XRUserRoot UserRoot { get; private set; }

        public ulong UniqueID { get; private set; }

        public XRHapticEngine HapticEngine { get; private set; }

        private XRHapticMap _hapticMap;

        protected virtual void Awake()
        {
            //find existing interactors
            XRUnityUILaserInteractor[] uiInteractors = GetComponentsInChildren<XRUnityUILaserInteractor>();
            if (uiInteractors.Length > 0)
            {
                UnityUIInteractor = uiInteractors[0];
            }
            else if (uiInteractors.Length > 1)
            {
                Debug.LogWarning(name + " has more than one XRUnityUILaserInteractor, only one is supported.");
            }

            _physicsInteractors.AddRange(GetComponentsInChildren<XRPhysicsInteractor>());
            
            //SetInteractorState<XRLaserInteractor>(false);
        }

        public virtual void Initialize(
            Chirality controllerChirality, 
            XRUserRoot userRoot, 
            XRUserRootConfig config,
            ulong uniqueID)
        {
            UserRoot = userRoot;
            UniqueID = uniqueID;
            ControllerChirality = controllerChirality;
            _inputMap = config.InputMapping(controllerChirality);
            _hapticMap = config.HapticMap;

            switch (ControllerChirality)
            {
                case Chirality.Left:
                    _xrNode = XRNode.LeftHand;
                    break;
                case Chirality.Right:
                    _xrNode = XRNode.RightHand;
                    break;
                case Chirality.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (null != config.HapticEngine)
            {
                HapticEngine = Instantiate(config.HapticEngine);
                HapticEngine.transform.SetParent(transform);
                HapticEngine.transform.localPosition = Vector3.zero;
                HapticEngine.Initialize(controllerChirality);
            }
            else
            {
                Debug.Log("HapticEngine is null for " + _inputMap.name + " on " + name);
            }

            SetupInputs(_inputMap);

            XRControllerModel model = Instantiate(config.ControllerModelPrefab(controllerChirality));
            model.transform.SetParent(transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.Initialize(this);

            for (int i = 0; i < _physicsInteractors.Count; ++i)
            {
                _physicsInteractors[i].transform.position = model.FrontAnchor.transform.position;
                _physicsInteractors[i].transform.rotation = model.FrontAnchor.transform.rotation;
            }
            if (null != UnityUIInteractor)
            {
                UnityUIInteractor.transform.position = model.FrontAnchor.transform.position;
                UnityUIInteractor.transform.rotation = model.FrontAnchor.transform.rotation;
            }
        }

        protected virtual void Update()
        {
            transform.localPosition = InputTracking.GetLocalPosition(_xrNode);
            transform.localRotation = InputTracking.GetLocalRotation(_xrNode);

            foreach (var input in _inputProcessors)
            {
                input.Value.Process();
            }

            for (int i = 0; i < _physicsInteractors.Count; ++i)
            {
               if (_physicsInteractors[i].gameObject.activeSelf && _physicsInteractors[i].enabled) 
                   _physicsInteractors[i].Process();
            }
        }

        /// <summary>
        /// Fires a Haptic determined by the key specified in the HapticMap on the XRUserRoot
        /// </summary>
        public void FireHapticKey(string hapticKey)
        {
            XRHapticDescription description = _hapticMap.GetHapticDescription(hapticKey);
            if (null != description && null != HapticEngine)
            {
                HapticEngine.PlayHapticDescription(description);
            }
        }

        public void SetInteractorState<T>(bool state) where T: XRInteractor
        {
            if (typeof(T) == typeof(XRUnityUILaserInteractor))
            {
                UnityUIInteractor.gameObject.SetActive(state);
            }
            
            XRPhysicsInteractor interactor = _physicsInteractors.Find(i => i is T);
            if (null != interactor)
            {
                interactor.gameObject.SetActive(state);
            }
        }
        
        public void SetSoloState(bool state, XRInteractor exception)
        {
            for (int i = 0; i < _physicsInteractors.Count; ++i)
            {
                if (_physicsInteractors[i] != exception)
                {
                    _physicsInteractors[i].enabled = state;
                }
            }
            if (UnityUIInteractor != exception)
            {
                UnityUIInteractor.enabled = state;
            }
        }

        public IXRInputProcessor GetInput(XRInputName inputName)
        {
            IXRInputProcessor inputProcessor;
            _inputProcessors.TryGetValue(inputName, out inputProcessor);
            if (null != inputProcessor)
            {
                return inputProcessor;
            }
            else
            {
                Debug.LogWarning(inputName + " not found on " + name);
                return null;
            }
        }

        private void SetupInputs(XRInputMap inputMap)
        {
            foreach (XRInputDescription map in inputMap.Descriptions)
            {
                if (map.Chirality != ControllerChirality) continue;
                if (_inputProcessors.ContainsKey(map.InputName))
                {
                    Debug.LogWarning("Duplicate InputName " +
                                     map.InputName +
                                     " specified for " +
                                     name +
                                     ", skipping.");
                    continue;
                }
                Type createType = map.InputType.GetProcessorType();
                IXRInputProcessor inputProcessor = (IXRInputProcessor)Activator.CreateInstance(createType, map);

//                Debug.Log("Adding input " + map.InputName + " on " + name);
                _inputProcessors.Add(map.InputName, inputProcessor);
            }
        }
    }
}