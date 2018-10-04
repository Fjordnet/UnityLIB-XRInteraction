using Fjord.Common.Enums;
using Fjord.Common.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Visualizes a XRFlexLaserInteractor.
    /// </summary>
    [RequireComponent(typeof(XRUnityUILaserInteractor))]
    [RequireComponent(typeof(LineRenderer))]
    public class XRUnityUILaserVisualizer : MonoBehaviour
    {
        [SerializeField]
        private GameObject _hitTargetPrefab;

        [SerializeField]
        private float _hitTargetScale = .2f;

        [SerializeField]
        private Color _hoverStartColor;

        [SerializeField]
        private Color _hoverEndColor;

        [SerializeField]
        private Color _pressStartColor;

        [SerializeField]
        private Color _pressEndColor;

        private LineRenderer _lineRenderer;
        private GameObject _hitTargetInstance;
        private Material _hitTargetMaterial;
        private HoverState _hoverState;
        private XRUnityUILaserInteractor _uiLaserInteractor;
        
        private void Awake()
        {
            _hitTargetInstance = Instantiate(_hitTargetPrefab, gameObject.transform);
            _hitTargetMaterial = _hitTargetInstance.GetComponent<MeshRenderer>().material;
            _hitTargetInstance.gameObject.SetActive(false);
            
            _lineRenderer = GetComponent<LineRenderer>();
            _uiLaserInteractor = GetComponent<XRUnityUILaserInteractor>();

            _lineRenderer.positionCount = 2;

            if (null == _uiLaserInteractor)
            {
                Debug.LogWarning("No LaserInteractor specified on " + name);
            }

            _uiLaserInteractor.Events.Enter.AddListener(Enter);
            _uiLaserInteractor.Events.Stay.AddListener(Stay);
            _uiLaserInteractor.Events.Exit.AddListener(Exit);
            _uiLaserInteractor.Events.ButtonDown.AddListener(ButtonDown);
            _uiLaserInteractor.Events.ButtonUp.AddListener(ButtonUp);
        }

        private void Enter(PointerEventData eventData)
        {
            _lineRenderer.enabled = true;
            _hitTargetInstance.gameObject.SetActive(true);
        }
        
        private void Stay(PointerEventData eventData)
        {
            _hitTargetInstance.transform.localScale = new Vector3(_hitTargetScale, _hitTargetScale, _hitTargetScale);
            _hitTargetInstance.transform.position = _uiLaserInteractor.EventData.PointerCurrentWorldPosition();
            _hitTargetInstance.transform.up =
                -_uiLaserInteractor.EventData.pointerCurrentRaycast.gameObject.transform.forward;
            
            _lineRenderer.SetPosition(0, _uiLaserInteractor.transform.position);
            _lineRenderer.SetPosition(1, _uiLaserInteractor.EventData.PointerCurrentWorldPosition());
        }
        
        private void Exit(PointerEventData eventData)
        {
            _lineRenderer.enabled = false;
            _hitTargetInstance.gameObject.SetActive(false);
        }
        
        private void ButtonDown(PointerEventData eventData)
        {
            _hoverState = HoverState.Press;
            _lineRenderer.startColor = _pressStartColor;
            _lineRenderer.endColor = _pressEndColor;
            _hitTargetMaterial.color = _pressEndColor;
        }
        
        private void ButtonUp(PointerEventData eventData)
        {
            _hoverState = HoverState.Hover;
            _lineRenderer.startColor = _hoverStartColor;
            _lineRenderer.endColor = _hoverEndColor;
            _hitTargetMaterial.color = _hoverEndColor;
        }
    }
}
