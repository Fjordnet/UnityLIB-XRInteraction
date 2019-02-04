using Fjord.Common.Data;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.Attachables
{
    [RequireComponent(typeof(Attachable))]
    public class AttachableVisualizer : MonoBehaviour
    {
        [Header("Mesh on different GameObject, will be swept to target.")]
        [SerializeField]
        private MeshFilter _sweepMesh;

        [SerializeField]
        private float _lineWidth = .01f;

        [SerializeField]
        private Material _lineMaterial;

        [SerializeField]
        private GradientAsset _lineGradient;

        [SerializeField]
        private float _lineScrollMultiplier = .05f;

        const float stepSize = .02f;
        private Attachable _attachable;

        private LineRenderer[] _lineRenderers;
        private Vector3[][] _lineVertices;
        private Vector3[] _sweepVertices;
        private int _stepLength;

        private void OnDestroy()
        {
            for (int i = 0; i < _lineRenderers.Length; ++i)
            {
                Destroy(_lineRenderers[i].gameObject);
            }
        }

        private void Start()
        {
            _attachable = GetComponent<Attachable>();
            _attachable.MovementInteraction.Events.ButtonDown.AddListener(ButtonDown);
            _attachable.MovementInteraction.Events.ButtonsHeld.AddListener(ButtonsHeld);
            _attachable.MovementInteraction.Events.ButtonUp.AddListener(ButtonUp);

            _sweepMesh.GetComponent<MeshRenderer>().enabled = false;

            _stepLength = (int)(1f / stepSize);
            _sweepVertices = _sweepMesh.sharedMesh.vertices;
            _lineVertices = new Vector3[_sweepVertices.Length][];
            _lineRenderers = new LineRenderer[_sweepVertices.Length];

            for (int i = 0; i < _sweepVertices.Length; ++i)
            {
                GameObject lineRendererGame = new GameObject("LineRenderer" + i);
                lineRendererGame.hideFlags = HideFlags.HideAndDontSave;
                _lineRenderers[i] = lineRendererGame.AddComponent<LineRenderer>();
                _lineRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _lineRenderers[i].receiveShadows = false;
                _lineRenderers[i].colorGradient = _lineGradient.Gradient;
                _lineRenderers[i].widthMultiplier = _lineWidth;
                _lineRenderers[i].material = _lineMaterial;
                _lineRenderers[i].positionCount = _stepLength;
                _lineRenderers[i].textureMode = LineTextureMode.Tile;
                _lineRenderers[i].enabled = false;
                _lineVertices[i] = new Vector3[_stepLength];
            }
        }

        public void LineVisibility(bool state)
        {
            for (int i = 0; i < _lineRenderers.Length; ++i)
            {
                _lineRenderers[i].enabled = state;
            }
        }

        private void ButtonDown(XRButtonDatum buttonDatum, XRInteractionEventReceiver receiver)
        {

        }

        private void ButtonsHeld(XRInteractionEventReceiver receiver)
        {
            if (_attachable.CanAttachTo.Count > 0 && _attachable.CanAttachTo.Count > 0 &&
                _attachable.CanAttachTo[0].enabled)
            {
                if (!_lineRenderers[0].enabled)
                {
                    LineVisibility(true);
                }

                Transform targetTransform = _attachable.CanAttachTo[0].VisualizerTarget.transform;
                Ray sourceRay = new Ray(_sweepMesh.transform.position, _sweepMesh.transform.forward);
                Ray targetRay = new Ray(targetTransform.position, targetTransform.forward);

                float distance = Vector3.Distance(transform.position, targetTransform.position);
                float handleDistance = distance / 3f;

                CubicBezierSegment segment = new CubicBezierSegment(
                    sourceRay.origin,
                    sourceRay.GetPoint(handleDistance),
                    targetRay.GetPoint(-handleDistance),
                    targetRay.origin);

                Vector3 initialSweepPosition = _sweepMesh.transform.localPosition;
                Quaternion initialLocalSweepRotation = _sweepMesh.transform.localRotation;
                Quaternion initialSweepRotation = _sweepMesh.transform.rotation;

                for (int i = 0; i < _lineRenderers.Length; ++i)
                {
                    int positionIndex = 0;
                    for (int step = 0; step < _stepLength; ++step)
                    {
                        float stepNormalized = (float)step / (float)_stepLength;
                        _sweepMesh.transform.position = segment.Point(stepNormalized);
                        _sweepMesh.transform.rotation =
                            Quaternion.Slerp(initialSweepRotation, targetTransform.rotation, stepNormalized);

                        _lineVertices[i][positionIndex] = _sweepMesh.transform.TransformPoint(_sweepVertices[i]);
                        positionIndex++;

                        _lineRenderers[i].SetPositions(_lineVertices[i]);

                        Vector2 newOffset = _lineRenderers[i].material.mainTextureOffset;
                        newOffset.x -= Time.deltaTime * _lineScrollMultiplier;
                        _lineRenderers[i].material.mainTextureOffset = newOffset;
                    }
                }

                _sweepMesh.transform.localPosition = initialSweepPosition;
                _sweepMesh.transform.localRotation = initialLocalSweepRotation;
            }
        }

        private void ButtonUp(XRButtonDatum buttonDatum, XRInteractionEventReceiver receiver)
        {
            if (receiver.HeldButtons.Count == 0)
            {
                for (int i = 0; i < _lineRenderers.Length; ++i)
                {
                    _lineRenderers[i].enabled = false;
                }
            }
        }

        [ContextMenu("Unhide All LineRenderers")]
        private void UnhideAllLineRenders()
        {
            Debug.Log("UnhideAllLineRenders");
            foreach (LineRenderer lineRenderer in FindObjectsOfType<LineRenderer>())
            {
                Debug.Log(lineRenderer.gameObject);
                lineRenderer.hideFlags = HideFlags.None;
                lineRenderer.gameObject.hideFlags = HideFlags.None;
            }
        }
    }
}