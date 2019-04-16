using Fjord.Common.Data;
using Fjord.Common.Enums;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInteractors;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Sprites;
using ColorUtility = Fjord.Common.Utilities.ColorUtility;

namespace Fjord.XRInteraction.XRInteractions
{
    [RequireComponent(typeof(XRInteractionEventReceiver))]
    public class XRInteractionHighlighter : MonoBehaviour
    {
        [SerializeField]
        private ColorAsset _hoverColor;

        [SerializeField]
        private ColorAsset _pressColor;

        [SerializeField]
        private Material _highlightMaterial;

        private readonly string[] SpreadPropertyNames = {"_SpreadL", "_SpreadR"};
        private readonly string[] ColorPropertyNames = {"_ColorL", "_ColorR"};
        private readonly string[] HighlightPropertyNames = {"_HighlightSourceL", "_HighlightSourceR"};

        private const float MaxSpread = 10;
        private XRInteractionEventReceiver _interactionEventReceiver;
        private MeshFilter[] _meshFilters;

        private HoverState[] _states = new HoverState[2];
        private MaterialPropertyBlock _leftBlock;

        private void Awake()
        {
            _interactionEventReceiver = GetComponent<XRInteractionEventReceiver>();
            _interactionEventReceiver.Events.Enter.AddListener(Enter);
            _interactionEventReceiver.Events.Exit.AddListener(Exit);
            _interactionEventReceiver.Events.ButtonDown.AddListener(ButtonDown);
            _interactionEventReceiver.Events.ButtonUp.AddListener(ButtonUp);

            _highlightMaterial = new Material(_highlightMaterial) {color = _hoverColor.Color};
            _highlightMaterial.color = _hoverColor.Color;
            _highlightMaterial.SetFloat(SpreadPropertyNames[0], MaxSpread);
            _highlightMaterial.SetFloat(SpreadPropertyNames[1], MaxSpread);
            _meshFilters = GetComponentsInChildren<MeshFilter>();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _states.Length; ++i)
            {
                switch (_states[i])
                {
                    case HoverState.Empty:
                        SetColor(Color.clear, ColorPropertyNames[i]);
                        SetSpread(MaxSpread, SpreadPropertyNames[i]);
                        break;
                    case HoverState.Hover:
                        SetColor(_hoverColor.Color, ColorPropertyNames[i]);
                        SetSpread(4, SpreadPropertyNames[i]);
                        SetHighlightPositions(i);
                        break;
                    case HoverState.Press:
                        SetColor(_pressColor.Color, ColorPropertyNames[i]);
                        SetSpread(6, SpreadPropertyNames[i]);
                        SetHighlightPositions(i);
                        break;
                }
            }

            if (_highlightMaterial.GetFloat(SpreadPropertyNames[0]) < MaxSpread - .01f ||
                _highlightMaterial.GetFloat(SpreadPropertyNames[1]) < MaxSpread - .01f)
            {
                Matrix4x4 matrix4X4 = new Matrix4x4();
                for (int i = 0; i < _meshFilters.Length; ++i)
                {
                    if (_meshFilters[i].gameObject.activeSelf && _meshFilters[i].GetComponent<MeshRenderer>().enabled)
                    {
                        matrix4X4.SetTRS(
                            _meshFilters[i].transform.position,
                            _meshFilters[i].transform.rotation,
                            _meshFilters[i].transform.lossyScale);

                        for (int si = 0; si < _meshFilters[i].sharedMesh.subMeshCount; ++si)
                        {
                            Graphics.DrawMesh(
                                _meshFilters[i].sharedMesh,
                                matrix4X4, _highlightMaterial,
                                0,
                                null,
                                si,
                                null,
                                false,
                                false,
                                false);
                        }
                    }
                }
            }
        }

        private void ButtonDown(XRButtonDatum datum, XRInteractionEventReceiver receiver)
        {
            _states[ButtonToChiralityIndex(datum)] = HoverState.Press;
        }

        private void ButtonUp(XRButtonDatum datum, XRInteractionEventReceiver receiver)
        {
            if (_states[ButtonToChiralityIndex(datum)] == HoverState.Press &&
                _interactionEventReceiver.enabled &&
                _interactionEventReceiver.HoveringInteractors.Contains(datum.ParentInteractor))
            {
                _states[ButtonToChiralityIndex(datum)] = HoverState.Hover;
            }
            else
            {
                _states[ButtonToChiralityIndex(datum)] = HoverState.Empty;
            }
        }

        private void Enter(XRPhysicsInteractor interactor, XRInteractionEventReceiver receiver)
        {
            if (interactor.ButtonHeldCount > 0)
            {
                _states[InteractorToChiralityIndex(interactor)] = HoverState.Press;
            }
            else
            {
                _states[InteractorToChiralityIndex(interactor)] = HoverState.Hover;
            }
        }

        private void Exit(XRPhysicsInteractor interactor, XRInteractionEventReceiver receiver)
        {
            if (interactor.ButtonHeldCount == 0 || !_interactionEventReceiver.enabled)
            {
                _states[InteractorToChiralityIndex(interactor)] = HoverState.Empty;
            }
        }

        private void SetColor(Color targetColor, string propertyName)
        {
            _highlightMaterial.SetColor(propertyName,
                ColorUtility.MoveTowards(
                    _highlightMaterial.GetColor(propertyName),
                    targetColor,
                    Time.deltaTime * 10));
        }

        private void SetSpread(float spreadTarget, string propertyName)
        {
            _highlightMaterial.SetFloat(
                propertyName,
                Mathf.MoveTowards(
                    _highlightMaterial.GetFloat(propertyName),
                    spreadTarget,
                    Time.deltaTime * 10));
        }

        private void SetHighlightPositions(int index)
        {
            XRPhysicsInteractor interactor = _interactionEventReceiver.GetHoveringInteractor((Chirality)index + 1);
            XRButtonDatum datum = _interactionEventReceiver.GetHeldButton((Chirality)index + 1);
            if (null != datum)
            {
                _highlightMaterial.SetVector(
                    HighlightPropertyNames[index],
                    datum.RayHitChildedToPressGameObject.origin);
            }
            else if (null != interactor)
            {
                _highlightMaterial.SetVector(
                    HighlightPropertyNames[index],
                    interactor.CurrentSourceRay.origin);
            }
        }

        private int InteractorToChiralityIndex(XRInteractor interactor)
        {
            return (int)interactor.ParentUserController.ControllerChirality - 1;
        }

        private int ButtonToChiralityIndex(XRButtonDatum datum)
        {
            return (int)datum.ParentInteractor.ParentUserController.ControllerChirality - 1;
        }
    }
}