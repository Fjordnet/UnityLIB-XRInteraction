using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Types;
using UnityEngine;
using UnityEngine.UI;

namespace Fjord.XRInteraction.XRUI
{
    [RequireComponent(typeof(Canvas))]
    public class XRCanvas : MonoBehaviour
    {
        [SerializeField]
        private GraphicRaycaster.BlockingObjects m_BlockingObjects = GraphicRaycaster.BlockingObjects.All;

        [SerializeField]
        private LayerMask m_BlockingMask = -1;

        private void Awake()
        {
            XRGraphicRaycaster leftRaycast = gameObject.AddComponent<XRGraphicRaycaster>();
            leftRaycast.Initialize(Chirality.Left, m_BlockingObjects, m_BlockingMask);
            
            XRGraphicRaycaster rightRaycast = gameObject.AddComponent<XRGraphicRaycaster>();
            rightRaycast.Initialize(Chirality.Right, m_BlockingObjects, m_BlockingMask);
        }
    }
}