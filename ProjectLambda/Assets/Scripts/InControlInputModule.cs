using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using InControl;

public class InControlInputModule : PointerInputModule
{
    public override void ActivateModule()
    {
        base.ActivateModule();

        GameObject toSelect = eventSystem.currentSelectedGameObject;
        if (toSelect == null)
        {
            toSelect = eventSystem.firstSelectedGameObject;
        }

        eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
    }

    public override void DeactivateModule()
    {
        base.DeactivateModule();
    }
    public override void Process()
    {
        bool usedEvent = sendUpdatEvent();
        if (eventSystem.sendNavigationEvents)
        {
            sendMoveEvent();

            sendSubmitEvent();
        }

        handleMouseEvents();
    }

    bool sendUpdatEvent()
    {
        if (eventSystem.currentSelectedGameObject == null)
        {
            return false;
        }

        BaseEventData data = GetBaseEventData();
        ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
        return data.used;
    }
    bool sendMoveEvent() {
        Vector2 movement = InputManager.ActiveDevice.DPad.Vector;

        AxisEventData axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
        if (movement.x != 0 || movement.y != 0)
        {
            if (eventSystem.currentSelectedGameObject == null)
            {
                eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject, GetBaseEventData());
            }
            else
            {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
            }
        }
        return axisEventData.used;
    }
    bool sendSubmitEvent() {
        if (eventSystem.currentSelectedGameObject == null)
        {
            return false;
        }

        BaseEventData data = GetBaseEventData();

        if (submitWasPressed()) {
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
        }

        if (cancelWasPressed()) {
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
        }

        return data.used;
    }
    void handleMouseEvents() {
        MouseState mouseData = GetMousePointerEventData();
        bool pressed = mouseData.AnyPressesThisFrame();
        bool released = mouseData.AnyReleasesThisFrame();

        MouseButtonEventData leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

        if (!UseMouse(pressed, released, leftButtonData.buttonData))
        {
            return;
        }

        ProcessMousePress(leftButtonData);
        ProcessMove(leftButtonData.buttonData);
        ProcessDrag(leftButtonData.buttonData);
    }
    void ProcessMousePress(MouseButtonEventData data)
    {
        var pointerEvent = data.buttonData;
        var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

        if (data.PressedThisFrame())
        {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            DeselectIfSelectionChanged(currentOverGo, pointerEvent);

            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

            if (newPressed == null)
            {
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            }

            float time = Time.unscaledTime;

            if (newPressed == pointerEvent.lastPress)
            {
                var diffTime = time - pointerEvent.clickTime;
                if (diffTime < 0.3f)
                {
                    ++pointerEvent.clickCount;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.clickTime = time;
            }
            else
            {
                pointerEvent.clickCount = 1;
            }

            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;

            pointerEvent.clickTime = time;

            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
        }

        if (data.ReleasedThisFrame())
        {

            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            else if (pointerEvent.pointerDrag != null)
            {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
            }

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            if (currentOverGo != pointerEvent.pointerEnter)
            {
                HandlePointerExitAndEnter(pointerEvent, null);
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
            }
        }
    }
    bool submitWasPressed() {
        if (InputManager.ActiveDevice.Name == "Keyboard & Mouse")
        {
            if (InputManager.ActiveDevice.GetControlByName("Button0").WasPressed)
            {
                return true;
            }
        }
        else
        {
            if (InputManager.ActiveDevice.Action1.WasPressed)
            {
                return true;
            }
        }
        return false;
    }
    bool cancelWasPressed() {
        if (InputManager.ActiveDevice.Name == "Keyboard & Mouse")
        {
            if (InputManager.ActiveDevice.GetControlByName("Button1").WasPressed)
            {
                return true;
            }
        }
        else
        {
            if (InputManager.ActiveDevice.Action2.WasPressed)
            {
                return true;
            }
        }
        return false;
    }
    bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
    {
        if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
            return true;

        return false;
    }
}
