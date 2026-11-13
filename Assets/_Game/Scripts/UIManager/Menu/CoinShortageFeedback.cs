using UnityEngine;

// "Not enough coins": the wallet bar bumps and the pressed buy button shakes sideways.
// Runs on unscaled time so it also works while the game is paused.
public sealed class CoinShortageFeedback : MonoBehaviour
{
    [SerializeField] private RectTransform walletBar;
    [SerializeField, Min(0.01f)] private float duration = 0.4f;
    [SerializeField, Range(0f, 0.5f)] private float walletBump = 0.18f;
    [SerializeField, Min(0f)] private float shakeDistance = 14f;
    [SerializeField, Min(1f)] private float shakeCycles = 3.5f;
    [SerializeField, Range(0.3f, 1f)] private float deniedPitch = 0.6f;
    private Vector3 walletRestScale;
    private RectTransform shakingButton;
    private Vector2 buttonRestPosition;
    private float elapsed;
    private bool isPlaying;

    void Awake()
    {
        walletRestScale = walletBar.localScale;
    }

    void OnDisable()
    {
        Finish();
    }

    void Update()
    {
        if (!isPlaying) return;
        elapsed += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        float fade = 1f - progress;
        walletBar.localScale = walletRestScale * (1f + walletBump * Mathf.Sin(progress * Mathf.PI));
        float shake = Mathf.Sin(progress * Mathf.PI * 2f * shakeCycles) * shakeDistance * fade;
        shakingButton.anchoredPosition = buttonRestPosition + Vector2.right * shake;
        if (progress >= 1f) Finish();
    }

    public void Play(RectTransform buyButton)
    {
        Finish();
        shakingButton = buyButton;
        buttonRestPosition = buyButton.anchoredPosition;
        elapsed = 0f;
        isPlaying = true;
        SoundManager.Ins.PlaySound(GameConstants.BUTTON_SOUND, deniedPitch, 1f);
        VibrationSettings.Vibrate();
    }

    private void Finish()
    {
        if (!isPlaying) return;
        isPlaying = false;
        walletBar.localScale = walletRestScale;
        shakingButton.anchoredPosition = buttonRestPosition;
    }
}
