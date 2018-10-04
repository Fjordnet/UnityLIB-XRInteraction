using System.Collections.Generic;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fjord.XRInteraction.XRUI
{
    /// <summary>
    /// PointerEventData retrieves it's camera via the eventCamera property on the Raycaster.
    /// This largely exists, constrained to chirality, to be something which holds a reference
    /// to the appropriate XRUnityUIInterfaces camera.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class XRGraphicRaycaster : GraphicRaycaster
    {
        private Chirality _controllerChirality;
        private XRUserRoot _userRoot;
        private Canvas _canvas;
        
        private Canvas canvas
        {
            get
            {
                return _canvas ?? (_canvas = GetComponent<Canvas>());
            }
        }

        public override Camera eventCamera
        {
            get { return UserRoot.GetController(_controllerChirality).UnityUIInteractor.EventCamera; }
        }

        private XRUserRoot UserRoot
        {
            get { return _userRoot ?? (_userRoot = FindObjectOfType<XRUserRoot>()); }
        }

        public void Initialize(Chirality controllerChirality, BlockingObjects blockingObjects, LayerMask blockingMask)
        {
            _controllerChirality = controllerChirality;
            this.blockingObjects = blockingObjects;
            m_BlockingMask = blockingMask;
        }

//        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
//        {
//            //Set controller chirality so calls to eventCamera will return proper camera
//            //pointerId correspond to chirality of XR Controllers.
//            _controllerChirality = (Chirality)eventData.pointerId;
//
//            //specifically the dropdown menu goes through a route where it will access eventCamera through
//            //the base class, ultimately returning the worldCamera on the Canvas, so you set worldCamera
//            //to the appropriate camera for that eventData's cycle.
//            _canvas.worldCamera = eventCamera;
//            
//            base.Raycast(eventData, resultAppendList);
//        }

        //All code below this point is copied from GraphicsRaycaster just to change the logic in the section
        //which raycast for blocking objects, becuase as of 2017.3 the RaycastAll method was not returning
        //an array of RaycastHits that were sorted, and the UI code in the DLL was assuming it was. Either
        //this was a Unity big that will get fixed, or the guy who wrote the original UI source code didn't know this.
        //Either way, when that is fixed, the code below can be removed, and the method above commented back in.
        
        private List<Graphic> m_RaycastResults = new List<Graphic>();

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (_controllerChirality != (Chirality)eventData.pointerId)
                return;
            
            canvas.worldCamera = eventCamera;

            if (canvas == null)
                return;

            var canvasGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
            if (canvasGraphics == null || canvasGraphics.Count == 0)
                return;

            int displayIndex;
            var currentEventCamera = eventCamera; // Propery can call Camera.main, so cache the reference

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || currentEventCamera == null)
                displayIndex = canvas.targetDisplay;
            else
                displayIndex = currentEventCamera.targetDisplay;

            var eventPosition = Display.RelativeMouseAt(eventData.position);
            if (eventPosition != Vector3.zero)
            {
                // We support multiple display and display identification based on event position.

                int eventDisplayIndex = (int)eventPosition.z;

                // Discard events that are not part of this display so the user does not interact with multiple displays at once.
                if (eventDisplayIndex != displayIndex)
                    return;
            }
            else
            {
                // The multiple display system is not supported on all platforms, when it is not supported the returned position
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                eventPosition = eventData.position;

                // We dont really know in which display the event occured. We will process the event assuming it occured in our display.
            }

            // Convert to view space
            Vector2 pos;
            if (currentEventCamera == null)
            {
                // Multiple display support only when not the main display. For display 0 the reported
                // resolution is always the desktops resolution since its part of the display API,
                // so we use the standard none multiple display method. (case 741751)
                float w = Screen.width;
                float h = Screen.height;
                if (displayIndex > 0 && displayIndex < Display.displays.Length)
                {
                    w = Display.displays[displayIndex].systemWidth;
                    h = Display.displays[displayIndex].systemHeight;
                }
                pos = new Vector2(eventPosition.x / w, eventPosition.y / h);
            }
            else
                pos = currentEventCamera.ScreenToViewportPoint(eventPosition);

            // If it's outside the camera's viewport, do nothing
            if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
                return;

            float hitDistance = float.MaxValue;

            Ray ray = new Ray();

            if (currentEventCamera != null)
                ray = currentEventCamera.ScreenPointToRay(eventPosition);

            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && blockingObjects != BlockingObjects.None)
            {
                float distanceToClipPlane = 100.0f;

                if (currentEventCamera != null)
                {
                    float projectionDirection = ray.direction.z;
                    distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                        ? Mathf.Infinity
                        : Mathf.Abs((currentEventCamera.farClipPlane - currentEventCamera.nearClipPlane) /
                                    projectionDirection);
                }

                if (blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All)
                {
                    //Edit
                    //Changed to use actual call from Physics rather than the reflection cache
                    var hits = Physics.RaycastAll(ray, distanceToClipPlane, m_BlockingMask);

                    if (hits.Length > 0)
                    {
                        //added search to ensure it is getting the nearest hit gameobject, as of 2017.3
                        //the results returned by Physics.RaycastAll were not ensured to be sorted by distance
                        hitDistance = float.MaxValue;
                        for (int i = 0; i < hits.Length; ++i)
                        {
                            if (hits[i].distance < hitDistance)
                            {
                                hitDistance = hits[i].distance;
                            }
                        }
                    }
                    //end edit
                }
            }

            m_RaycastResults.Clear();
            Raycast(canvas, currentEventCamera, eventPosition, canvasGraphics, m_RaycastResults);

            int totalCount = m_RaycastResults.Count;
            for (var index = 0; index < totalCount; index++)
            {
                var go = m_RaycastResults[index].gameObject;
                bool appendGraphic = true;

                if (ignoreReversedGraphics)
                {
                    if (currentEventCamera == null)
                    {
                        // If we dont have a camera we know that we should always be facing forward
                        var dir = go.transform.rotation * Vector3.forward;
                        appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
                    }
                    else
                    {
                        // If we have a camera compare the direction against the cameras forward.
                        var cameraFoward = currentEventCamera.transform.rotation * Vector3.forward;
                        var dir = go.transform.rotation * Vector3.forward;
                        appendGraphic = Vector3.Dot(cameraFoward, dir) > 0;
                    }
                }

                if (appendGraphic)
                {
                    float distance = 0;

                    if (currentEventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        distance = 0;
                    else
                    {
                        Transform trans = go.transform;
                        Vector3 transForward = trans.forward;
                        // http://geomalgorithms.com/a06-_intersect-2.html
                        distance = (Vector3.Dot(transForward, trans.position - currentEventCamera.transform.position) /
                                    Vector3.Dot(transForward, ray.direction));

                        // Check to see if the go is behind the camera.
                        if (distance < 0)
                            continue;
                    }
                    
                    if (distance >= hitDistance)
                        continue;

                    var castResult = new RaycastResult
                    {
                        gameObject = go,
                        module = this,
                        distance = distance,
                        screenPosition = eventPosition,
                        index = resultAppendList.Count,
                        depth = m_RaycastResults[index].depth,
                        sortingLayer = canvas.sortingLayerID,
                        sortingOrder = canvas.sortingOrder
                    };
                    resultAppendList.Add(castResult);
                }
            }
        }

        static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();

        private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition,
            IList<Graphic> foundGraphics, List<Graphic> results)
        {
            // Debug.Log("ttt" + pointerPoision + ":::" + camera);
            // Necessary for the event system
            int totalCount = foundGraphics.Count;
            for (int i = 0; i < totalCount; ++i)
            {
                Graphic graphic = foundGraphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition,
                    eventCamera))
                    continue;

                if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z >
                    eventCamera.farClipPlane)
                    continue;

                if (graphic.Raycast(pointerPosition, eventCamera))
                {
                    s_SortedGraphics.Add(graphic);
                }
            }

            s_SortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            //      StringBuilder cast = new StringBuilder();
            totalCount = s_SortedGraphics.Count;
            for (int i = 0; i < totalCount; ++i)
                results.Add(s_SortedGraphics[i]);
            //      Debug.Log (cast.ToString());

            s_SortedGraphics.Clear();
        }
    }
}