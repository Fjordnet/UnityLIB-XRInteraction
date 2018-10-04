using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.XRUser
{
    public class XRControllerModel : MonoBehaviour
    {
        [ContextMenuItem("Generate Front Anchor", "GenerateFrontAnchor")]
        [SerializeField]
        private Transform _frontAnchor;

        public XRUserController UserController { get; private set; }
        
        public Transform FrontAnchor
        {
            get { return _frontAnchor; }
        }

        private void Reset()
        {
            GenerateFrontAnchor();
        }

        public virtual void Initialize(XRUserController userController)
        {
            UserController = userController;
        }

        private void GenerateFrontAnchor()
        {
            GameObject gameObject = new GameObject("FrontAnchor");
            gameObject.transform.position = transform.position;
            gameObject.transform.rotation = transform.rotation;
            gameObject.transform.SetParent(transform);
            _frontAnchor = gameObject.transform;
        }
        
        private void OnDrawGizmos()
        {
            Debug.DrawRay(_frontAnchor.transform.position, _frontAnchor.transform.forward);
        }
    }
}