using UnityEngine;

// Darkens the menu and shows a slowly turning golden halo behind a freshly bought item,
// so the reward stands out and no text underneath competes with it.
// Runs on unscaled time so it also works while the game is paused.
public sealed class RewardCelebrationBackdrop : MonoBehaviour
{
    [SerializeField] private CanvasGroup dimGroup;
    [SerializeField] private RectTransform halo;
    [SerializeField] private CanvasGroup haloGroup;
    [SerializeField, Min(0.01f)] private float appearDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float vanishDuration = 0.3f;
    [SerializeField] private float haloSpinDegrees = 40f;
    [SerializeField, Range(0f, 0.2f)] private float haloPulse = 0.05f;
    private float elapsed;
    private float showDuration;
    private bool isShowing;

    void OnEnable()
    {
        Hide();
    }

    void Update()
    {
        if (!isShowing) return;
        elapsed += Time.unscaledDeltaTime;
        float appear = Mathf.Clamp01(elapsed / appearDuration);
        float vanish = Mathf.Clamp01((elapsed - showDuration) / vanishDuration);
        float alpha = Mathf.SmoothStep(0f, 1f, appear) * (1f - vanish);
        dimGroup.alpha = alpha;
        haloGroup.alpha = alpha;
        halo.localRotation = Quaternion.Euler(0f, 0f, -elapsed * haloSpinDegrees);
        float pulse = 1f + Mathf.Sin(elapsed * 6f) * haloPulse;
        halo.localScale = Vector3.one * (Mathf.Lerp(0.6f, 1f, appear) * pulse);
        if (vanish >= 1f) Hide();
    }

    // Shows for visibleDuration seconds, then fades away by itself.
    public void Play(float visibleDuration)
    {
        showDuration = visibleDuration;
        elapsed = 0f;
        isShowing = true;
    }

    public void Hide()
    {
        isShowing = false;
        dimGroup.alpha = 0f;
        haloGroup.alpha = 0f;
    }
}
