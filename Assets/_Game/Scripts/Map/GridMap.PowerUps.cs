using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GridMap
{
    private Action<BlockTile, Vector2> hammerTargetCallback;
    private bool hammerSelecting;
    private bool hammerImpactPending;

    public bool IsHammerSelecting => hammerSelecting;
    public bool IsPowerUpBusy => hammerSelecting || hammerImpactPending;

    public void FillHammerTargetScreenPositions(List<Vector2> positions)
    {
        positions.Clear();
        foreach (BlockTile block in blocks.Values)
            positions.Add(cameraController.WorldToScreen(block.Tile.WorldPosition));
    }

    public bool BeginHammerSelection(Action<BlockTile, Vector2> selected)
    {
        if (IsPowerUpBusy || blocks.Count == 0 || player.IsBusy || AreTilesRotating())
            return false;
        hammerSelecting = true;
        hammerTargetCallback = selected;
        inputReader.Reset();
        return true;
    }

    public void CancelHammerSelection()
    {
        if (!IsPowerUpBusy) return;
        hammerSelecting = false;
        hammerImpactPending = false;
        hammerTargetCallback = null;
        inputReader.Reset();
    }

    public void BreakBlock(BlockTile target)
    {
        Vector2Int cell = target.Tile.Cell;
        if (!blocks.TryGetValue(cell, out BlockTile registered) || registered != target)
            throw new UnityException("Hammer target is not a registered obstacle.");
        blocks.Remove(cell);
        hammerImpactPending = false;
        target.PlayBreak();
        cameraController.PlayImpactImpulse(0.24f, 0.08f);
    }

    private void ReadHammerInput()
    {
        bool canSelect = GameManager.IsState(GameState.Gameplay)
            && player != null && !player.IsBusy && !AreTilesRotating();
        if (!inputReader.TryReadTap(canSelect, out Vector2 screenPosition)) return;
        Vector3 world = cameraController.ScreenToWorld(screenPosition, runtimeRoot.position.z);
        Vector2 local = runtimeRoot.InverseTransformPoint(world);
        if (!boardLayout.TryGetCell(local, out Vector2Int cell)) return;
        if (!blocks.TryGetValue(cell, out BlockTile target))
        {
            PulseInvalidTargets();
            return;
        }
        SelectHammerTarget(target);
    }

    private void SelectHammerTarget(BlockTile target)
    {
        hammerSelecting = false;
        hammerImpactPending = true;
        inputReader.Reset();
        Vector2 screenPosition = cameraController.WorldToScreen(target.Tile.WorldPosition);
        Action<BlockTile, Vector2> callback = hammerTargetCallback;
        hammerTargetCallback = null;
        callback(target, screenPosition);
    }

    private void PulseInvalidTargets()
    {
        foreach (BlockTile block in blocks.Values) block.PulseHighlight();
    }
}
