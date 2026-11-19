using UnityEngine;

public partial class GridMap
{
    private enum MoveBlockReason
    {
        None,
        Obstacle,
        LockedFinish,
        Frozen
    }

    private void ReadMoveInput()
    {
        if (tutorialInputLocked)
        {
            ReadTutorialTap();
            return;
        }
        // Moves are accepted while the mother or pipes are still animating:
        // MoveTo starts from her current position, so quick inputs chain without waiting.
        bool canMove = !LoadingManager.Ins.IsLoading
            && GameManager.IsState(GameState.Gameplay) && player != null;
        Vector2Int direction = inputReader.ReadDirection(canMove);
        if (direction != Vector2Int.zero)
        {
            feedback.ResetIdle();
            TryMove(direction);
        }
        else if (canMove && feedback.TickIdle(Time.deltaTime)) ShowMoveHint();
    }

    private bool AreTilesRotating()
    {
        foreach (OneWayPath path in oneWayPaths.Values)
        {
            if (path.Tile.IsRotating) return true;
        }
        return false;
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int nextCell = playerCell + direction;
        MoveBlockReason blockReason = BlockReason(playerCell, nextCell, direction);
        if (blockReason != MoveBlockReason.None)
        {
            PlayBlockedReaction(direction, blockReason);
            return;
        }

        bool isBacktracking = IsBacktrackingToPreviousCell(nextCell);
        Vector2Int buttonCell = isBacktracking ? playerCell : nextCell;
        bool rotatesTiles = rotateButtons.ContainsKey(buttonCell);
        if (!UpdateRoute(nextCell, isBacktracking))
        {
            player.PlayBlocked(direction);
            feedback.PlayBlocked();
            return;
        }

        playerCell = nextCell;
        player.MoveTo(playerCell, CellToLocalPosition(playerCell));
        LevelManager.Ins.NotifyPlayerStepCompleted();
        MarkCurrentCell();
        if (isBacktracking) feedback.PlayBacktrack();
        else feedback.PlayStep();
        LevelManager.Ins.NotifyPlayerMoved();
        if (!isBacktracking && rotateButtons.ContainsKey(playerCell))
            ActivateRotateButton();
        if (rotatesTiles)
        {
            SoundManager.Ins.PlaySound(GameConstants.ROTATE_SOUND);
            PlayRotateSwirl(buttonCell, isBacktracking);
        }
        RefreshFinish();
    }

    private void PlayBlockedReaction(Vector2Int direction,
        MoveBlockReason blockReason)
    {
        player.PlayBlocked(direction);
        feedback.PlayBlocked();
        HighlightBlocker(direction, blockReason);
        if (blockReason == MoveBlockReason.LockedFinish)
            LevelManager.Ins.NotifyMissingTargetsBlocked();
    }

    private bool UpdateRoute(Vector2Int nextCell, bool isBacktracking)
    {
        if (isBacktracking)
        {
            RestoreTargetAt(playerCell);
            if (rotateButtons.ContainsKey(playerCell)) RevertOneWayTiles();
            pathTrail.Remove(playerCell);
            currentRoute.RemoveAt(currentRoute.Count - 1);
            return true;
        }

        if (pathTrail.HasMark(nextCell)) return false;
        currentRoute.Add(nextCell);
        return true;
    }

    private MoveBlockReason BlockReason(Vector2Int fromCell,
        Vector2Int toCell, Vector2Int direction)
    {
        if (!tiles.ContainsKey(toCell) || blocks.ContainsKey(toCell))
            return MoveBlockReason.Obstacle;
        if (toCell == finishCell && !finishPoint.IsOpen)
            return MoveBlockReason.LockedFinish;
        if (IsFrozen(toCell)) return MoveBlockReason.Frozen;
        if (IsBacktrackingToPreviousCell(toCell)) return MoveBlockReason.None;
        return AllowsDirection(fromCell, direction)
            && AllowsDirection(toCell, -direction)
            ? MoveBlockReason.None : MoveBlockReason.Obstacle;
    }

    private bool AllowsDirection(Vector2Int cell, Vector2Int direction)
    {
        return !oneWayPaths.ContainsKey(cell)
            || oneWayPaths[cell].Allows(direction);
    }

    private void ActivateRotateButton()
    {
        foreach (OneWayPath path in oneWayPaths.Values) path.ToggleState(RippleDelay(path.Tile.Cell));
        rotateButtons[playerCell].Pulse();
    }

    private void ResetOneWayPaths()
    {
        foreach (OneWayPath path in oneWayPaths.Values) path.ResetState();
    }

    private void MarkCurrentCell()
    {
        pathTrail.Mark(playerCell, CellToLocalPosition(playerCell));
        if (targets.ContainsKey(playerCell)) CollectTarget(targets[playerCell]);
        NotifyTargetProgress();
        RefreshIce();
    }

    private void ResetTargets()
    {
        foreach (TargetPoint target in targets.Values) target.ResetTarget();
    }

    private void RestoreTargetAt(Vector2Int cell)
    {
        if (targets.ContainsKey(cell)) targets[cell].ResetTarget();
        NotifyTargetProgress();
        RefreshIce();
    }

    private void RevertOneWayTiles()
    {
        foreach (OneWayPath path in oneWayPaths.Values) path.ToggleBack(RippleDelay(path.Tile.Cell));
    }

    private void RefreshFinish()
    {
        if (finishPoint == null) return;
        bool wasOpen = finishPoint.IsOpen;
        finishPoint.SetOpen(AreAllTargetsCollected());
        if (!wasOpen && finishPoint.IsOpen) PlayFinishOpened();
        if (finishPoint.IsOpen && playerCell == finishCell)
            LevelManager.Ins.OnWin();
    }

    private bool AreAllTargetsCollected()
    {
        foreach (TargetPoint target in targets.Values)
        {
            if (!target.IsCollected) return false;
        }
        return true;
    }

    private void NotifyTargetProgress()
    {
        int collected = 0;
        foreach (TargetPoint target in targets.Values)
        {
            if (target.IsCollected) collected++;
        }
        LevelManager.Ins.NotifyTargetProgress(collected, targets.Count);
    }

    private bool IsBacktrackingToPreviousCell(Vector2Int nextCell)
    {
        return currentRoute.Count >= 2
            && currentRoute[currentRoute.Count - 2] == nextCell;
    }
}
