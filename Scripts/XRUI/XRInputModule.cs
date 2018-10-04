using Fjord.Common.Types;
using Fjord.XRInteraction.XRInteractors;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fjord.XRInteraction.XRUI
{
    /// <summary>
    /// Use in place of StandAloneInputModule for UI to work XR controllers.
    /// </summary>
    public class XRInputModule : StandaloneInputModule
    {
        private XRUserRoot _userRoot;

        private XRUserRoot UserRoot
        {
            get { return _userRoot ?? (_userRoot = FindObjectOfType<XRUserRoot>()); }
        }

        protected new void Reset()
        {
            //The VR controller is prone to more "shaking" thus the drag threshold needs
            //to be higher. This is important because some default
            //ui components like the DropDown menu rely on the .dragging to determine whether to cancel
            //selection of items, thus if the DragThreshold is too easy pass it doesn't work.
            GetComponent<EventSystem>().pixelDragThreshold = 20;
        }

        /// <summary>
        /// Copied from StandaloneInputModule - Majority has been edited.
        /// </summary>
        public override void Process()
        {
            bool usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }

            ProcessTouchEvents();
        }

        /// <summary>
        /// Copied from StandaloneInputModule - Majority has been edited.
        /// </summary>
        private void ProcessTouchEvents()
        {
            for (int i = 1; i < 3; ++i)
            {
                XRUserController userController = UserRoot.GetController((Chirality)i);

                if (null == userController || null == userController.UnityUIInteractor) 
                    continue;
                
                bool released;
                bool pressed;
                var pointer = GetTouchPointerEventData(userController.UnityUIInteractor, out pressed, out released);
                
                ProcessTouchPress(pointer, pressed, released);

                if (!released)
                {
                    ProcessMove(pointer);
                    ProcessDrag(pointer);
                }

                userController.UnityUIInteractor.UpdatePointerEventData(pointer, pressed, released);
            }
        }

        /// <summary>
        /// Copied from PointerInputModule - Majority has been edited.
        /// </summary>
        private PointerEventData GetTouchPointerEventData(
            XRUnityUILaserInteractor uiLaserInteractor,
            out bool pressed,
            out bool released)
        {
            PointerEventData pointerData;
            int chiralityId = (int)uiLaserInteractor.ParentUserController.ControllerChirality;
            var created = GetPointerData(chiralityId, out pointerData, true);

            bool enabled = uiLaserInteractor.enabled && uiLaserInteractor.gameObject.activeSelf;
            
            pointerData.Reset();

            if (enabled)
            {
                pressed = created || uiLaserInteractor.GetButtonDown();
                released = uiLaserInteractor.GetButtonUp();

                if (created)
                    pointerData.position = uiLaserInteractor.PositionInCamera();

                if (pressed)
                    pointerData.delta = Vector2.zero;
                else
                    pointerData.delta = uiLaserInteractor.DeltaInScreen;

                pointerData.position = uiLaserInteractor.PositionInCamera();

                pointerData.button = PointerEventData.InputButton.Left;

                eventSystem.RaycastAll(pointerData, m_RaycastResultCache);

                var raycast = FindFirstRaycast(m_RaycastResultCache);
                pointerData.pointerCurrentRaycast = raycast;
                m_RaycastResultCache.Clear();
            }
            else
            {
                pressed = false;
                
                if (null != pointerData.pointerPress)
                {
                    released = true;
                }
                else
                {
                    released = false;
                }
                
                pointerData.pointerCurrentRaycast = new RaycastResult();
            }

            return pointerData;
        }

        /// <summary>
        /// Copied from PointInputModule - 1 Removal
        /// </summary>
        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            if (!pointerEvent.IsPointerMoving() ||
                /*REMOVED Cursor.lockState == CursorLockMode.Locked ||*/
                pointerEvent.pointerDrag == null)
                return;

            if (!pointerEvent.dragging
                && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold,
                    pointerEvent.useDragThreshold))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        /// <summary>
        /// Copied from StandaloneInputModule - 1 Removal - 1 Addition
        /// </summary>
        protected new void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                if (pointerEvent.pointerEnter != currentOverGo)
                {
                    // send a pointer enter to the touched element if it isn't the one to select...
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                    pointerEvent.pointerEnter = currentOverGo;
                }

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed =
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent,
                        ExecuteEvents.initializePotentialDrag);
            }
            //ADD
            else
            {
                //The position is always in the center of the camera
                //thus the pressPosition moves away from the center.
                pointerEvent.pressPosition -= pointerEvent.delta;
            }
            //ADD END

            // PointerUp notification
            if (released)
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                /* REMOVE
                // send exit events as we need to simulate this on touch up on touch device
                ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;
                */
            }

            //ADD
            if (null == currentOverGo)
            {
                pointerEvent.pointerEnter = null;
            }
            //END ADD
        }


        /// <summary>
        /// Copied from PointInputModule 
        /// </summary>
        private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold,
            bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }
    }
}