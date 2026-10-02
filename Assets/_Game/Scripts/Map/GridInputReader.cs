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

    private Vector2Int ReadSwipe()
    {
        Touchscreen screen = Touchscreen.current;
        if (screen != null)
        {
            int fingers = 0;
            foreach (var finger in screen.touches)
                if (finger.press.isPressed) fingers++;
            if (fingers > 1)
            {
                Reset();
                return Vector2Int.zero;
            }

            var touch = screen.primaryTouch;
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
            if (touch.press.wasReleasedThisFrame)
                return EndSwipe(touch.position.ReadValue());
            if (fingers > 0 || touchId != 0) return Vector2Int.zero;
        }
        return ReadMouseSwipe();
    }

    private Vector2Int ReadMouseSwipe()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return Vector2Int.zero;
        if (mouse.leftButton.wasPressedThisFrame) BeginSwipe(mouse.position.ReadValue());
        if (mouse.leftButton.wasReleasedThisFrame) return EndSwipe(mouse.position.ReadValue());
        return Vector2Int.zero;
    }

    private void BeginSwipe(Vector2 position)
    {
        swipeStart = position;
        trackingSwipe = !IsOverUI(position);
    }

    private Vector2Int EndSwipe(Vector2 position)
    {
        bool wasTracking = trackingSwipe;
        Vector2 delta = position - swipeStart;
        Reset();
        if (!wasTracking || IsOverUI(position)) return Vector2Int.zero;
        float threshold = Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) * 0.04f, 20f, 80f);
        if (delta.magnitude < threshold) return Vector2Int.zero;
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
        foreach (RaycastResult hit in uiHits)
            if (hit.module is GraphicRaycaster) return true;
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
