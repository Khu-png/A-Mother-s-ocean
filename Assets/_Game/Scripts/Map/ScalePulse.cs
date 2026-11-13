using UnityEngine;

// One-shot springy scale bump. A negative amount squashes before springing back.
public sealed class ScalePulse
{
    private const float WOBBLE_CYCLES = 1.25f;
    private float duration;
    private float amount;
    private float elapsed;

    public bool IsActive => elapsed < duration;
    public float Factor { get; private set; } = 1f;

    public void Play(float pulseDuration, float pulseAmount)
    {
        duration = pulseDuration;
        amount = pulseAmount;
        elapsed = 0f;
        Factor = 1f;
    }

    public void Stop()
    {
        duration = 0f;
        elapsed = 0f;
        Factor = 1f;
    }

    public void Advance(float deltaTime)
    {
        if (!IsActive) return;
        elapsed += deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        float wobble = Mathf.Sin(progress * Mathf.PI * 2f * WOBBLE_CYCLES);
        Factor = 1f + amount * wobble * (1f - progress);
    }
}
