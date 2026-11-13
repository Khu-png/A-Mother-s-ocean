using TMPro;
using UnityEngine;

// Last-seconds pressure: the timer beats once per second and a red edge glow grows stronger.
public sealed class GameplayTimeWarning : MonoBehaviour
{
    [SerializeField] private RectTransform timerTransform;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CanvasGroup edgeGlowGroup;
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.25f, 1f);
    [SerializeField, Min(1)] private int warningSeconds = 10;
    [SerializeField, Min(0.01f)] private float beatDuration = 0.35f;
    [SerializeField, Range(0f, 0.6f)] private float beatScale = 0.22f;
    [SerializeField, Range(0f, 1f)] private float minimumGlow = 0.25f;
    [SerializeField, Range(0f, 1f)] private float beatGlow = 0.45f;
    [SerializeField, Range(1f, 2f)] private float tickPitch = 1.5f;
    [SerializeField, Range(0f, 1f)] private float tickVolume = 0.45f;
    private Vector3 timerBaseScale;
    private Color timerBaseColor;
    private float beatTimer;
    private float urgency;
    private bool isWarning;

    void Awake()
    {
        timerBaseScale = timerTransform.localScale;
        timerBaseColor = timerText.color;
    }

    void Update()
    {
        if (!isWarning) return;
        beatTimer = Mathf.Max(0f, beatTimer - Time.unscaledDeltaTime);
        float beat = Mathf.Sin(beatTimer / beatDuration * Mathf.PI);
        timerTransform.localScale = timerBaseScale * (1f + beat * beatScale);
        timerText.color = Color.Lerp(timerBaseColor, warningColor, 0.5f + beat * 0.5f);
        edgeGlowGroup.alpha = Mathf.Lerp(minimumGlow, 1f, urgency) * (1f - beatGlow + beat * beatGlow);
    }

    // Called by the HUD whenever the displayed second changes.
    public void Refresh(float remainingSeconds)
    {
        int second = Mathf.CeilToInt(remainingSeconds);
        bool shouldWarn = second > 0 && second <= warningSeconds;
        if (!shouldWarn)
        {
            Hide();
            return;
        }
        isWarning = true;
        urgency = 1f - (second - 1f) / warningSeconds;
        beatTimer = beatDuration;
        SoundManager.Ins.PlaySound(GameConstants.BUTTON_SOUND, tickPitch, tickVolume);
    }

    public void Hide()
    {
        isWarning = false;
        beatTimer = 0f;
        timerTransform.localScale = timerBaseScale;
        timerText.color = timerBaseColor;
        edgeGlowGroup.alpha = 0f;
    }
}
