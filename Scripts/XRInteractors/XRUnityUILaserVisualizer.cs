using Fjord.Common.Enums;
using Fjord.Common.Extensions;
using Fjord.Common.Types;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fjord.XRInteraction.XRInteractors
{
    /// <summary>
    /// Visualizes a XRUnityUILaserInteractor.
    /// </summary>
    [RequireComponent(typeof(XRUnityUILaserInteractor))]
    [RequireComponent(typeof(LineRenderer))]
    public class XRUnityUILaserVisualizer : MonoBehaviour
    {
        [SerializeField]
        private Color _hoverStartColor;

        [SerializeField]
        private Color _hoverEndColor;

        [SerializeField]
        private Color _pressStartColor;

        [SerializeField]
        private Color _pressEndColor;

        private const int BezierSegmentCount = 32;
        private CubicBezierSegment _bezierSegment;
        private LineRenderer _lineRenderer;
        private HoverState _hoverState;
        private XRUnityUILaserInteractor _uiLaserInteractor;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _uiLaserInteractor = GetComponent<XRUnityUILaserInteractor>();

            _lineRenderer.positionCount = BezierSegmentCount;

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
            UpdateCurve();
        }

        private void Stay(PointerEventData eventData)
        {
            UpdateCurve();
        }

        private void UpdateCurve()
        {
            Ray sourceRay = new Ray(_uiLaserInteractor.transform.position, _uiLaserInteractor.transform.forward);
            Ray hitRay = new Ray(_uiLaserInteractor.EventData.PointerCurrentWorldPosition(), Vector3.forward);
            hitRay.direction = (sourceRay.origin - hitRay.origin).normalized;

            float startEndDistance = Vector3.Distance(
                sourceRay.origin,
                hitRay.origin);
            float handleDistance = startEndDistance / 4f;

            CubicBezierSegment bezierSegment = new CubicBezierSegment(
                sourceRay.origin,
                sourceRay.GetPoint(handleDistance),
                hitRay.GetPoint(handleDistance),
                hitRay.origin);

            float step = 1f / BezierSegmentCount;
            float t = 0;
            for (int i = 0; i < BezierSegmentCount; ++i)
            {
                _lineRenderer.SetPosition(i, bezierSegment.Point(t));
                t += step;
            }
        }

        private void Exit(PointerEventData eventData)
        {
            _lineRenderer.enabled = false;
        }

        private void ButtonDown(PointerEventData eventData)
        {
            _hoverState = HoverState.Press;
            _lineRenderer.startColor = _pressStartColor;
            _lineRenderer.endColor = _pressEndColor;
        }

        private void ButtonUp(PointerEventData eventData)
        {
            _hoverState = HoverState.Hover;
            _lineRenderer.startColor = _hoverStartColor;
            _lineRenderer.endColor = _hoverEndColor;
        }
    }
}