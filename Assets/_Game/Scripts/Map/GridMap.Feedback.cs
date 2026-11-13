using System.Collections;
using UnityEngine;

public partial class GridMap
{
    private const int BUBBLE_CAPACITY = 32;
    private static readonly Vector2Int[] MOVE_DIRECTIONS =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    [Header("Feedback")]
    [SerializeField] private GameplayFeedback feedback = new GameplayFeedback();
    private readonly ScalePulse motherPulse = new ScalePulse();
    private BubbleBurst bubbles;
    private System.Action<TargetPoint> targetArrivedHandler;
    private float hitStopRestoreTimeScale = 1f;
    private float tileBubbleTimer;
    private bool isHitStopping;

    private void BuildFeedback()
    {
        bubbles = new BubbleBurst(runtimeRoot, cellSize, BUBBLE_CAPACITY);
        targetArrivedHandler = HandleTargetArrived;
        motherPulse.Stop();
        feedback.ResetCombo();
        feedback.ResetIdle();
    }

    // Now and then a few bubbles get pulled into a whirlpool or escape from a rock,
    // so special tiles keep feeling alive between moves.
    private void TickTileAmbience()
    {
        tileBubbleTimer += Time.deltaTime;
        if (tileBubbleTimer < feedback.TileBubbleInterval) return;
        tileBubbleTimer = 0f;
        if (rotateButtons.Count > 0)
            bubbles.PlaySwirl(CellToLocalPosition(RandomCell(rotateButtons.Keys)), feedback.WhirlpoolBubbleCount,
                feedback.SwirlRadius * 0.7f, feedback.SwirlTurnsPerSecond, feedback.BubbleSize * 0.6f,
                feedback.BubbleColor);
        if (blocks.Count > 0)
            bubbles.Play(CellToLocalPosition(RandomCell(blocks.Keys)) + Vector3.up * (cellSize * 0.2f),
                feedback.RockBubbleCount, feedback.RescueBubbleSpread * 0.4f, feedback.BubbleSize * 0.5f,
                feedback.BubbleColor);
    }

    private static Vector2Int RandomCell(System.Collections.Generic.ICollection<Vector2Int> cells)
    {
        int pick = Random.Range(0, cells.Count);
        foreach (Vector2Int cell in cells)
        {
            if (pick-- == 0) return cell;
        }
        throw new System.InvalidOperationException("RandomCell needs a non-empty collection.");
    }

    private void TickMotherPulse()
    {
        if (!motherPulse.IsActive) return;
        motherPulse.Advance(Time.deltaTime);
        player.SetRootScale(motherPulse.Factor);
    }

    // The baby has dived into the mother: bubbles burst and the mother swells to catch it.
    private void HandleTargetArrived(TargetPoint target)
    {
        bubbles.Play(CellToLocalPosition(target.Cell), feedback.RescueBubbleCount,
            feedback.RescueBubbleSpread, feedback.BubbleSize, feedback.BubbleColor);
        motherPulse.Play(feedback.PulseDuration, feedback.MotherCatchPulse);
        feedback.Vibrate();
    }

    private void PlayFinishOpened()
    {
        finishPoint.PlayPulse(feedback.PulseDuration, feedback.FinishOpenPulse);
        bubbles.Play(CellToLocalPosition(finishCell), feedback.FinishBubbleCount,
            feedback.RescueBubbleSpread, feedback.BubbleSize, feedback.BubbleColor);
        feedback.PlayFinishOpened();
    }

    // Shows the player what stopped the move: the block, the pipe, or the locked finish.
    private void HighlightBlocker(Vector2Int direction, MoveBlockReason reason)
    {
        Vector2Int nextCell = playerCell + direction;
        if (reason == MoveBlockReason.LockedFinish)
            finishPoint.PlayPulse(feedback.PulseDuration, feedback.FinishDeniedPulse);
        else if (blocks.ContainsKey(nextCell)) blocks[nextCell].PulseHighlight();
        else
        {
            Vector2Int pipeCell = AllowsDirection(playerCell, direction) ? nextCell : playerCell;
            if (oneWayPaths.ContainsKey(pipeCell))
                tiles[pipeCell].Flash(feedback.BlockedFlashColor, feedback.FlashDuration);
        }
    }

    // Lights up neighbouring cells the player can still enter, without revealing the solution.
    private void ShowMoveHint()
    {
        foreach (Vector2Int direction in MOVE_DIRECTIONS)
        {
            Vector2Int nextCell = playerCell + direction;
            if (pathTrail.HasMark(nextCell)) continue;
            if (BlockReason(playerCell, nextCell, direction) != MoveBlockReason.None) continue;
            tiles[nextCell].Flash(feedback.HintFlashColor, feedback.FlashDuration * 2f);
        }
    }

    private float RippleDelay(Vector2Int cell)
    {
        return Vector2Int.Distance(cell, playerCell) * feedback.RotateRippleDelayPerCell;
    }

    // Pressing spins the swirl one way; stepping back off the button spins it the other way.
    private void PlayRotateSwirl(Vector2Int buttonCell, bool isReverting)
    {
        float turns = isReverting ? -feedback.SwirlTurnsPerSecond : feedback.SwirlTurnsPerSecond;
        bubbles.PlaySwirl(CellToLocalPosition(buttonCell), feedback.SwirlBubbleCount,
            feedback.SwirlRadius, turns, feedback.BubbleSize, feedback.BubbleColor);
    }

    private void PlayLoseImpulse()
    {
        cameraController.PlayImpactImpulse(feedback.LoseImpulseDuration,
            feedback.LoseImpulseStrength);
    }

    // Arrival at the finish: a big bubble burst, a camera kick and a brief hit-stop.
    private IEnumerator PlayWinImpact()
    {
        bubbles.Play(CellToLocalPosition(finishCell), feedback.FinishBubbleCount,
            feedback.WinBubbleSpread, feedback.BubbleSize, feedback.BubbleColor);
        finishPoint.PlayPulse(feedback.PulseDuration, feedback.FinishOpenPulse);
        cameraController.PlayImpactImpulse(feedback.ImpactDuration, feedback.ImpactStrength);
        feedback.Vibrate();
        hitStopRestoreTimeScale = Time.timeScale;
        isHitStopping = true;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(feedback.WinHitStop);
        EndHitStop();
    }

    // Also called when the transition is cancelled so time never stays frozen.
    private void EndHitStop()
    {
        if (!isHitStopping) return;
        isHitStopping = false;
        Time.timeScale = hitStopRestoreTimeScale;
    }
}
