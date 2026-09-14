using UnityEngine;
using UnityEngine.InputSystem;

public static class GridInputReader
{
    public static Vector2Int ReadDirection()
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
