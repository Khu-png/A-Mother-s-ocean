using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GridInputReader
{
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    private Vector2 swipeStart;
    private bool trackingSwipe;
    private int touchId;

    public Vector2Int ReadDirection(bool canMove)
    {
        if (!canMove || !Application.isFocused)
        {
            Reset();
            return Vector2Int.zero;
        }

        Vector2Int swipe = ReadSwipe();
        Vector2Int keyboard = ReadKeyboard();
        if (keyboard != Vector2Int.zero)
        {
            Reset();
            return keyboard;
        }
        return swipe;
    }

    public void Reset()
    {
        trackingSwipe = false;
        touchId = 0;
    }

    public bool TryReadTap(bool canTap, out Vector2 screenPosition)
    {
        screenPosition = Vector2.zero;
        if (!canTap || !Application.isFocused)
        {
            Reset();
            return false;
        }

        Touchscreen screen = Touchscreen.current;
        if (screen != null)
        {
            UnityEngine.InputSystem.Controls.TouchControl touch = screen.primaryTouch;
            if (touch.press.wasPressedThisFrame) BeginSwipe(touch.position.ReadValue());
            if (touch.press.wasReleasedThisFrame)
                return CompleteTap(touch.position.ReadValue(), out screenPosition);
        }
        Mouse mouse = Mouse.current;
        if (mouse == null) return false;
        if (mouse.leftButton.wasPressedThisFrame) BeginSwipe(mouse.position.ReadValue());
        return mouse.leftButton.wasReleasedThisFrame
            && CompleteTap(mouse.position.ReadValue(), out screenPosition);
    }

    private bool CompleteTap(Vector2 position, out Vector2 screenPosition)
    {
        bool validStart = trackingSwipe;
        Vector2 delta = position - swipeStart;
        Reset();
        screenPosition = position;
        float maximumTravel = Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) * 0.025f, 16f, 48f);
        return validStart && !IsOverUI(position)
            && delta.sqrMagnitude <= maximumTravel * maximumTravel;
    }

    private Vector2Int ReadSwipe()
    {
        Touchscreen screen = Touchscreen.current;
        if (screen != null)
        {
            int fingers = 0;
            for (int index = 0; index < screen.touches.Count; index++)
                if (screen.touches[index].press.isPressed) fingers++;
            if (fingers > 1)
            {
                Reset();
                return Vector2Int.zero;
            }

            UnityEngine.InputSystem.Controls.TouchControl touch = screen.primaryTouch;
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                Reset();
                return Vector2Int.zero;
            }
            if (touch.press.wasPressedThisFrame)
            {
                touchId = touch.touchId.ReadValue();
                BeginSwipe(touch.position.ReadValue());
            }
            if (touch.press.isPressed) return ReadActiveSwipe(touch.position.ReadValue());
            if (touch.press.wasReleasedThisFrame) Reset();
            if (fingers > 0 || touchId != 0) return Vector2Int.zero;
        }
        return ReadMouseSwipe();
    }

    private Vector2Int ReadMouseSwipe()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return Vector2Int.zero;
        if (mouse.leftButton.wasPressedThisFrame) BeginSwipe(mouse.position.ReadValue());
        if (mouse.leftButton.isPressed) return ReadActiveSwipe(mouse.position.ReadValue());
        if (mouse.leftButton.wasReleasedThisFrame) Reset();
        return Vector2Int.zero;
    }

    // Fires as soon as the finger travels far enough, without waiting for release.
    // One swipe moves exactly one cell: tracking stops until the finger is lifted.
    private Vector2Int ReadActiveSwipe(Vector2 position)
    {
        if (!trackingSwipe) return Vector2Int.zero;
        Vector2Int direction = SwipeDirection(position - swipeStart);
        if (direction != Vector2Int.zero) trackingSwipe = false;
        return direction;
    }

    private void BeginSwipe(Vector2 position)
    {
        swipeStart = position;
        trackingSwipe = !IsOverUI(position);
    }

    private static Vector2Int SwipeDirection(Vector2 delta)
    {
        float threshold = Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) * 0.04f, 20f, 80f);
        if (delta.sqrMagnitude < threshold * threshold) return Vector2Int.zero;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0f ? Vector2Int.right : Vector2Int.left;
        return delta.y > 0f ? Vector2Int.up : Vector2Int.down;
    }

    private bool IsOverUI(Vector2 position)
    {
        EventSystem events = EventSystem.current;
        if (events == null) return false;
        uiHits.Clear();
        events.RaycastAll(new PointerEventData(events) { position = position }, uiHits);
        for (int index = 0; index < uiHits.Count; index++)
            if (uiHits[index].module is GraphicRaycaster) return true;
        return false;
    }

    private static Vector2Int ReadKeyboard()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector2Int.zero;
        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) return Vector2Int.up;
        if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) return Vector2Int.down;
        if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) return Vector2Int.left;
        if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) return Vector2Int.right;
        return Vector2Int.zero;
    }
}
