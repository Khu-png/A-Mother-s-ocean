using System.Collections.Generic;
using UnityEngine;

public partial class GridMap
{
    private bool tutorialInputLocked;
    private bool tutorialTapPending;

    // While a tutorial intro is up, swipes are ignored and a tap advances it.
    public void SetTutorialLock(bool locked)
    {
        tutorialInputLocked = locked;
        tutorialTapPending = false;
        inputReader.Reset();
    }

    public bool ConsumeTutorialTap()
    {
        bool tapped = tutorialTapPending;
        tutorialTapPending = false;
        return tapped;
    }

    private void ReadTutorialTap()
    {
        bool canTap = !LoadingManager.Ins.IsLoading && GameManager.IsState(GameState.Gameplay);
        if (inputReader.TryReadTap(canTap, out Vector2 _)) tutorialTapPending = true;
    }

    public TutorialFocusData GetTutorialFocus(TutorialFocusKind kind)
    {
        switch (kind)
        {
            case TutorialFocusKind.Player:
                return CreateFocus(player.WorldPosition,
                    player.DisplaySprite, player.DisplayColor);
            case TutorialFocusKind.Target:
                return TargetFocus();
            case TutorialFocusKind.Finish:
                return CreateFocus(finishPoint.WorldPosition,
                    finishPoint.DisplaySprite, finishPoint.DisplayColor);
            case TutorialFocusKind.Rock:
                return CellFocus(blocks, "cục đá");
            case TutorialFocusKind.OneWay:
                return CellFocus(oneWayPaths, "ô một chiều");
            case TutorialFocusKind.Whirlpool:
                return CellFocus(rotateButtons, "xoáy nước");
            default:
                throw new UnityException("Loại mục tiêu tutorial không hợp lệ.");
        }
    }

    private TutorialFocusData TargetFocus()
    {
        foreach (TargetPoint target in targets.Values)
        {
            return CreateFocus(target.WorldPosition,
                target.DisplaySprite, target.DisplayColor);
        }
        throw new UnityException("Level tutorial cần có một cá mập con.");
    }

    // Copies the nearest tile's look so it can be drawn above the dim overlay.
    private TutorialFocusData CellFocus<T>(Dictionary<Vector2Int, T> cells, string label)
    {
        MapTile tile = tiles[NearestCell(cells, label)];
        Vector3 center = tile.WorldPosition;
        Vector2 worldSize = tile.WorldSize;
        Vector2 origin = cameraController.WorldToScreen(center);
        Vector2 right = cameraController.WorldToScreen(center + Vector3.right * worldSize.x);
        Vector2 up = cameraController.WorldToScreen(center + Vector3.up * worldSize.y);
        Vector2 screenSize = new Vector2(Mathf.Abs(right.x - origin.x), Mathf.Abs(up.y - origin.y));
        return new TutorialFocusData(origin, tile.DisplaySprite, tile.DisplayColor,
            screenSize, tile.WorldRotation);
    }

    private Vector2Int NearestCell<T>(Dictionary<Vector2Int, T> cells, string label)
    {
        bool found = false;
        Vector2Int nearest = default;
        int nearestDistance = int.MaxValue;
        foreach (Vector2Int cell in cells.Keys)
        {
            int distance = (cell - playerCell).sqrMagnitude;
            if (distance >= nearestDistance) continue;
            nearest = cell;
            nearestDistance = distance;
            found = true;
        }
        if (!found) throw new UnityException($"Level tutorial cần có ít nhất một {label}.");
        return nearest;
    }

    private TutorialFocusData CreateFocus(Vector3 worldPosition,
        Sprite sprite, Color color)
    {
        Vector2 screenPosition = cameraController.WorldToScreen(worldPosition);
        return new TutorialFocusData(screenPosition, sprite, color);
    }
}
