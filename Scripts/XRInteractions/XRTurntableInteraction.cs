using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractions
{
    public class XRTurntableInteraction : XRInteractionEventReceiver
    {
        [SerializeField]
        private float _damp = .1f;
        
        public Rigidbody AttachedRigidbody { get; private set; }

        private float _priorAngle;
        private float _deltaAngle;
        private float _dampVelocity;
        
        protected virtual void Awake()
        {
            AttachedRigidbody = GetComponentInParent<Rigidbody>();
        }

        private void Update()
        {
            _deltaAngle = Mathf.MoveTowards(_deltaAngle, 0, Time.deltaTime);
            AttachedRigidbody.transform.Rotate(0, _deltaAngle, 0, Space.Self);
        }

        public override void OnButtonDown(XRButtonDatum buttonDatum)
        {
            base.OnButtonDown(buttonDatum);
            _priorAngle = YAngle();
            _deltaAngle = 0;
        }

        public override void OnButtonsHeld()
        {
            base.OnButtonsHeld();

            float angle = Mathf.SmoothDampAngle(_priorAngle, YAngle(), ref _dampVelocity, _damp);
            _deltaAngle = angle - _priorAngle;
            _priorAngle = angle;
        }

        public override void OnButtonUp(XRButtonDatum buttonDatum)
        {
            base.OnButtonUp(buttonDatum);
        }

        private float YAngle()
        {
            return Mathf.Atan2(
                       AttachedRigidbody.transform.position.x - HeldButtons[0].RayHitChildedToController.origin.x,
                       AttachedRigidbody.transform.position.z - HeldButtons[0].RayHitChildedToController.origin.z) *
                   Mathf.Rad2Deg;
        }
    }
}