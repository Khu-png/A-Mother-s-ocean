using System;
using System.Collections;
using UnityEngine;

public partial class GridMap
{
    [Header("Win Transition")]
    [SerializeField, Min(0.01f)] private float winPlayerFadeDuration = 0.4f;
    [SerializeField, Min(0.01f)] private float winDetailFadeDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float winTileDuration = 0.55f;
    [SerializeField, Min(0f)] private float winWaveDuration = 0.7f;
    private Coroutine winRoutine;
    private bool isClearingBoard;

    public void PlayWinTransition(Action completed)
    {
        isClearingBoard = true;
        winRoutine = StartCoroutine(PlayWinSequence(completed));
    }

    private IEnumerator PlayWinSequence(Action completed)
    {
        while (player.IsBusy) yield return null;
        BeginWinSequence();
        yield return FadePlayer();
        yield return FadeDetails();
        gridVisualRoot.gameObject.SetActive(false);
        yield return ClearTileBases();
        CompleteWinSequence(completed);
    }

    private IEnumerator FadePlayer()
    {
        float elapsed = 0f;
        while (elapsed < winPlayerFadeDuration)
        {
            player.ApplyWinFade(Mathf.Clamp01(elapsed / winPlayerFadeDuration));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        player.ApplyWinFade(1f);
    }

    private IEnumerator FadeDetails()
    {
        float elapsed = 0f;
        while (elapsed < winDetailFadeDuration)
        {
            ApplyDetailFade(Mathf.Clamp01(elapsed / winDetailFadeDuration));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        ApplyDetailFade(1f);
    }

    private IEnumerator ClearTileBases()
    {
        float duration = winWaveDuration + winTileDuration;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            ApplyBaseWave(elapsed);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        ApplyBaseWave(duration);
    }

    private void BeginWinSequence()
    {
        foreach (RotateButton button in rotateButtons.Values) button.StopPulse();
        startPoint.BeginWinFade();
        finishPoint.BeginWinFade();
        foreach (MapTile tile in tiles.Values) tile.BeginWinDetailFade();
    }

    private void ApplyDetailFade(float progress)
    {
        startPoint.ApplyWinFade(progress);
        finishPoint.ApplyWinFade(progress);
        pathTrail.ApplyWinFade(progress);
        foreach (MapTile tile in tiles.Values) tile.ApplyWinDetailFade(progress);
    }

    private void ApplyBaseWave(float elapsed)
    {
        float diagonalLength = Mathf.Max(1, width + height - 2);
        foreach (MapTile tile in tiles.Values)
        {
            float diagonal = tile.Cell.x + height - 1 - tile.Cell.y;
            float delay = diagonal / diagonalLength * winWaveDuration;
            float progress = Mathf.Clamp01((elapsed - delay) / winTileDuration);
            tile.ApplyWinBaseClear(progress);
        }
    }

    private void CompleteWinSequence(Action completed)
    {
        runtimeRoot.gameObject.SetActive(false);
        winRoutine = null;
        completed();
    }

    private void StopWinTransition()
    {
        if (winRoutine != null) StopCoroutine(winRoutine);
        winRoutine = null;
        isClearingBoard = false;
    }
}
