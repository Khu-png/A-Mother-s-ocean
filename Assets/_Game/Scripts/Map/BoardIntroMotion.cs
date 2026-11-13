using UnityEngine;

// Timing for the board entrance: the board zooms in, tiles pop out from the center in a wave,
// then the start, finish, targets and player pop in once the tiles have landed.
[System.Serializable]
public sealed class BoardIntroMotion
{
    [Header("Board Zoom")]
    [SerializeField, Range(0.1f, 1f)] private float startScale = 0.82f;
    [SerializeField, Range(1f, 1.3f)] private float overshootScale = 1.04f;
    [SerializeField, Min(0.01f)] private float duration = 0.45f;
    [SerializeField, Range(0.1f, 0.9f)] private float overshootPoint = 0.65f;

    [Header("Tile Wave")]
    [SerializeField, Min(0f)] private float tileWaveDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float tilePopDuration = 0.28f;
    [SerializeField, Range(1f, 1.4f)] private float popOvershootScale = 1.12f;

    [Header("Pieces")]
    [SerializeField, Min(0f)] private float pieceDelay = 0.5f;
    [SerializeField, Min(0.01f)] private float piecePopDuration = 0.3f;
    private float elapsed;

    public bool IsPlaying { get; private set; }
    public float CurrentFactor { get; private set; } = 1f;
    public float PieceFactor => PopFactor((elapsed - pieceDelay) / piecePopDuration);
    private float TotalDuration => Mathf.Max(duration,
        tileWaveDuration + tilePopDuration, pieceDelay + piecePopDuration);

    public void Begin()
    {
        elapsed = 0f;
        IsPlaying = true;
        CurrentFactor = startScale;
    }

    public void Stop()
    {
        IsPlaying = false;
        elapsed = TotalDuration;
        CurrentFactor = 1f;
    }

    public void Advance(float deltaTime)
    {
        elapsed += deltaTime;
        CurrentFactor = Overshoot(Mathf.Clamp01(elapsed / duration), startScale, overshootScale);
        if (elapsed >= TotalDuration) Stop();
    }

    // waveOrder is 0 for the first tile to appear and 1 for the last.
    public float TileFactor(float waveOrder)
    {
        float delay = waveOrder * tileWaveDuration;
        return PopFactor((elapsed - delay) / tilePopDuration);
    }

    private float PopFactor(float progress)
    {
        return Overshoot(Mathf.Clamp01(progress), 0f, popOvershootScale);
    }

    private float Overshoot(float progress, float from, float peak)
    {
        if (progress < overshootPoint)
        {
            float grow = progress / overshootPoint;
            float easedGrow = 1f - (1f - grow) * (1f - grow);
            return Mathf.Lerp(from, peak, easedGrow);
        }
        float settle = (progress - overshootPoint) / (1f - overshootPoint);
        return Mathf.Lerp(peak, 1f, Mathf.SmoothStep(0f, 1f, settle));
    }
}
