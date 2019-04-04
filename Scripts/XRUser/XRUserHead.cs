using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fjord.Common.Extensions;
using Fjord.XRInteraction.XRUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fjord.XRInteraction.XRUser
{
    /// <summary>
    /// Encapsulates all things related to the XR Users head and the user's VR camera.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class XRUserHead : MonoBehaviour
    {
        [Tooltip("Head will fade out when enter colliders on these layers.")]
        [SerializeField]
        private LayerMask _fadeOutOnEnterLayers;

        private List<Collider> _headInColliders = new List<Collider>();
        private Camera _headCamera;
        private XRHeadUI _headUI;

        public Camera HeadCamera
        {
            get { return _headCamera; }
        }

        public XRHeadUI UI
        {
            get { return _headUI; }
        }

        private void Reset()
        {
            _fadeOutOnEnterLayers = LayerMask.GetMask("Default");
        }

        private void Awake()
        {
            _headCamera = GetComponent<Camera>();
            _headUI = GetComponentInChildren<XRHeadUI>();
            SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
        }

        private void SceneManagerOnSceneUnloaded(Scene arg0)
        {
            _headInColliders.RemoveAll(x => x == null);
            if (_headInColliders.Count == 0) _headUI.FadeInView();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ShouldFadeFromCollider(other))
            {
                //should probably subscribe to the obects OnDestroy in some manner
                _headInColliders.RemoveAll(x => x == null);

                _headInColliders.Add(other);
                if (_headInColliders.Count == 1) _headUI.FadeOutView();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (ShouldFadeFromCollider(other))
            {
                _headInColliders.Remove(other);
                if (_headInColliders.Count == 0) _headUI.FadeInView();
            }
        }

        /// <summary>
        /// Is collider on fading layer and is collider a rigidbody? It is assumed a collider
        /// with a rigidbody will be dynamic and moving around the scene, thus you would not
        /// want your head fading out when placed inside of it.
        /// </summary>
        private bool ShouldFadeFromCollider(Collider other)
        {
            int layerMask = 1 << other.gameObject.layer;
            return ((_fadeOutOnEnterLayers & layerMask) == layerMask) &&
                   other.attachedRigidbody == null &&
                   !other.isTrigger;
        }
    }
}