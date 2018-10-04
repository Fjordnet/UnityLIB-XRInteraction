using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractors
{
    [RequireComponent(typeof(XRFlexLaserInteractor))]
    [RequireComponent(typeof(LineRenderer))]
    public class XRTeleporter : MonoBehaviour
    {
        [Header("Movement blocked by Layers")]
        [SerializeField]
        private LayerMask _blockedLayerMask;

        [Header("Button which enables teleport action.")]
        [SerializeField]
        private XRInputName _teleportButton;

        [SerializeField]
        private GameObject _teleportTargetPrefab;

        [SerializeField]
        private float _lineScrollMultiplier = 4;

        [SerializeField]
        private Color _validStartColor;

        [SerializeField]
        private Color _validEndColor;

        [SerializeField]
        private Color _invalidStartColor;

        [SerializeField]
        private Color _invalidEndColor;

        private XRFlexLaserInteractor _flexLaserInteractor;
        private XRUserRoot _userRoot;
        private LineRenderer _lineRenderer;
        private Material _lineMaterial;
        private GameObject _teleportTargetInstance;
        private Material _teleportTargetMaterial;

        private void Reset()
        {
            _blockedLayerMask = LayerMask.GetMask("Default");
        }

        private void Start()
        {
            _flexLaserInteractor = GetComponent<XRFlexLaserInteractor>();
            _flexLaserInteractor.Events.ButtonDown.AddListener(ButtonDown);
            _flexLaserInteractor.Events.ButtonUp.AddListener(ButtonUp);
            _flexLaserInteractor.Events.ButtonHold.AddListener(ButtonHold);
            _userRoot = _flexLaserInteractor.ParentUserController.UserRoot;

            _lineRenderer = GetComponent<LineRenderer>();
            _lineMaterial = _lineRenderer.material;
            _lineRenderer.enabled = false;

            _teleportTargetInstance = Instantiate(_teleportTargetPrefab, gameObject.transform);
            _teleportTargetInstance.gameObject.SetActive(false);
            _teleportTargetMaterial = _teleportTargetInstance.GetComponent<Renderer>().material;
            _teleportTargetMaterial.color = _validEndColor;
        }

        private void ButtonDown(XRButtonDatum buttonDatum)
        {
            if (_teleportButton == buttonDatum.InputName)
            {
                _lineRenderer.enabled = true;
                _teleportTargetInstance.gameObject.SetActive(true);
            }
        }

        private void ButtonUp(XRButtonDatum buttonDatum)
        {
            if (_teleportButton == buttonDatum.InputName)
            {
                _lineRenderer.enabled = false;
                _teleportTargetInstance.gameObject.SetActive(false);

                if (IsCurrentInteractorPositionValid())
                {
                    _userRoot.TweenTranslation(TranslationToTarget());
                }
            }
        }

        private void ButtonHold(XRButtonDatum buttonDatum)
        {
            Vector2 newOffset = _lineMaterial.mainTextureOffset;
            newOffset.x -= Time.deltaTime * _lineScrollMultiplier;
            _lineMaterial.mainTextureOffset = newOffset;

            _lineRenderer.positionCount = _flexLaserInteractor.StepCount;
            for (int i = 0; i < _flexLaserInteractor.StepCount; ++i)
            {
                _lineRenderer.SetPosition(i, _flexLaserInteractor.StepPoints[i]);
            }

            _teleportTargetInstance.transform.position = buttonDatum.ParentInteractor.CurrentHitRay.origin;
            _teleportTargetInstance.transform.up = buttonDatum.ParentInteractor.CurrentHitRay.direction;

            if (IsCurrentInteractorPositionValid())
            {
                _lineRenderer.startColor = _validStartColor;
                _lineRenderer.endColor = _validEndColor;
                _teleportTargetMaterial.color = _validEndColor;
            }
            else
            {
                _lineRenderer.startColor = _invalidStartColor;
                _lineRenderer.endColor = _invalidEndColor;
                _teleportTargetMaterial.color = _invalidEndColor;
            }
        }

        private Vector3 TranslationToTarget()
        {
            Vector3 targetPosition = _flexLaserInteractor.CurrentHitRay.origin;
            Vector3 headFloorPositon = _userRoot.UserHead.transform.position;
            headFloorPositon.y -= _userRoot.UserHead.transform.localPosition.y;
            Vector3 deltaPosition = targetPosition - headFloorPositon;
            return deltaPosition;
        }

        private bool IsCurrentInteractorPositionValid()
        {
            Vector3 headTop = _userRoot.UserHead.transform.position;
            Vector3 headBottom = new Vector3(headTop.x, headTop.y - .2f, headTop.z);

            Vector3 targetBodyTop = _flexLaserInteractor.CurrentHitRay.origin;
            targetBodyTop.y += _userRoot.UserHead.transform.localPosition.y;
            Vector3 targetBodyBottom = _flexLaserInteractor.CurrentHitRay.origin;
            targetBodyBottom.y += .3f;
            
            float bodyRadius = .25f;
            Vector3 translation = TranslationToTarget();

            //check if it is hitting something and surface is upward pointing
            if (null == _flexLaserInteractor.CurrentCollider ||
                Vector3.Angle(Vector3.up, _flexLaserInteractor.CurrentHitRay.direction) > 40)
            {
                return false;
            }
            //check if the head will collide with any objects when transitioning
            if (Physics.CapsuleCast(headBottom, headTop, bodyRadius, translation, translation.magnitude, _blockedLayerMask))
            {
                return false;
            }
            //check that the landing location will fit player
            if (Physics.CheckCapsule(targetBodyBottom, targetBodyTop, bodyRadius, _blockedLayerMask))
            {
                return false;
            }
            return true;
        }
    }
}