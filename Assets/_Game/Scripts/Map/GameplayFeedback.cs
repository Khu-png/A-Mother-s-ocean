using UnityEngine;

// Sound, haptic and timing tuning for moment-to-moment gameplay feedback.
[System.Serializable]
public sealed class GameplayFeedback
{
    [Header("Step Sound")]
    [SerializeField, Range(0f, 0.1f)] private float stepPitchIncrease = 0.035f;
    [SerializeField, Min(1)] private int maxStepCombo = 8;
    [SerializeField, Range(0.3f, 1f)] private float backtrackPitch = 0.8f;
    [SerializeField, Range(0f, 1f)] private float backtrackVolume = 0.55f;
    [SerializeField, Range(0.3f, 1f)] private float blockedPitch = 0.55f;
    [SerializeField, Range(1f, 2f)] private float finishOpenPitch = 1.45f;
    [SerializeField, Min(0f)] private float vibrationCooldown = 0.6f;

    [Header("Highlights")]
    [SerializeField] private Color blockedFlashColor = new Color(1f, 0.45f, 0.4f, 1f);
    [SerializeField] private Color hintFlashColor = new Color(0.55f, 1f, 0.95f, 1f);
    [SerializeField, Min(0.01f)] private float flashDuration = 0.32f;
    [SerializeField, Min(0f)] private float idleHintDelay = 6f;
    [SerializeField, Min(0.1f)] private float idleHintInterval = 1.6f;

    [Header("Bubbles")]
    [SerializeField] private Color bubbleColor = new Color(0.75f, 0.95f, 1f, 0.75f);
    [SerializeField, Min(1)] private int rescueBubbleCount = 8;
    [SerializeField, Min(1)] private int finishBubbleCount = 12;
    [SerializeField, Min(0f)] private float rescueBubbleSpread = 1.6f;
    [SerializeField, Min(0f)] private float winBubbleSpread = 2.4f;
    [SerializeField, Min(0.01f)] private float bubbleSize = 0.19f;

    [Header("Rotate Swirl")]
    [SerializeField, Min(1)] private int swirlBubbleCount = 10;
    [SerializeField, Min(0f)] private float swirlRadius = 0.6f;
    [SerializeField] private float swirlTurnsPerSecond = 1.2f;

    [Header("Tile Ambience")]
    [SerializeField, Min(0.2f)] private float tileBubbleInterval = 2.4f;
    [SerializeField, Min(1)] private int whirlpoolBubbleCount = 4;
    [SerializeField, Min(1)] private int rockBubbleCount = 2;

    [Header("Camera")]
    [SerializeField, Min(0.01f)] private float loseImpulseDuration = 0.15f;
    [SerializeField, Min(0f)] private float loseImpulseStrength = 0.025f;
    [SerializeField, Min(0.01f)] private float impactDuration = 0.22f;
    [SerializeField, Min(0f)] private float impactStrength = 0.07f;

    [Header("Motion")]
    [SerializeField, Min(0f)] private float rotateRippleDelayPerCell = 0.05f;
    [SerializeField, Range(0f, 0.6f)] private float motherCatchPulse = 0.22f;
    [SerializeField, Range(0f, 0.6f)] private float finishOpenPulse = 0.35f;
    [SerializeField, Range(-0.4f, 0f)] private float finishDeniedPulse = -0.14f;
    [SerializeField, Min(0.01f)] private float pulseDuration = 0.45f;
    [SerializeField, Min(0f)] private float winHitStop = 0.09f;
    private int stepCombo;
    private float lastVibrationTime = float.NegativeInfinity;
    private float idleTime;

    public Color BlockedFlashColor => blockedFlashColor;
    public Color HintFlashColor => hintFlashColor;
    public float FlashDuration => flashDuration;
    public Color BubbleColor => bubbleColor;
    public int RescueBubbleCount => rescueBubbleCount;
    public int FinishBubbleCount => finishBubbleCount;
    public float RescueBubbleSpread => rescueBubbleSpread;
    public float WinBubbleSpread => winBubbleSpread;
    public float BubbleSize => bubbleSize;
    public int SwirlBubbleCount => swirlBubbleCount;
    public float SwirlRadius => swirlRadius;
    public float SwirlTurnsPerSecond => swirlTurnsPerSecond;
    public float TileBubbleInterval => tileBubbleInterval;
    public int WhirlpoolBubbleCount => whirlpoolBubbleCount;
    public int RockBubbleCount => rockBubbleCount;
    public float LoseImpulseDuration => loseImpulseDuration;
    public float LoseImpulseStrength => loseImpulseStrength;
    public float ImpactDuration => impactDuration;
    public float ImpactStrength => impactStrength;
    public float RotateRippleDelayPerCell => rotateRippleDelayPerCell;
    public float MotherCatchPulse => motherCatchPulse;
    public float FinishOpenPulse => finishOpenPulse;
    public float FinishDeniedPulse => finishDeniedPulse;
    public float PulseDuration => pulseDuration;
    public float WinHitStop => winHitStop;

    public void PlayStep()
    {
        stepCombo = Mathf.Min(stepCombo + 1, maxStepCombo);
        float pitch = 1f + (stepCombo - 1) * stepPitchIncrease;
        SoundManager.Ins.PlaySound(GameConstants.MOVE_SOUND, pitch, 1f);
    }

    public void PlayBacktrack()
    {
        stepCombo = 0;
        SoundManager.Ins.PlaySound(GameConstants.MOVE_SOUND, backtrackPitch, backtrackVolume);
    }

    public void PlayBlocked()
    {
        stepCombo = 0;
        SoundManager.Ins.PlaySound(GameConstants.MOVE_SOUND, blockedPitch, 1f);
        Vibrate();
    }

    public void PlayFinishOpened()
    {
        SoundManager.Ins.PlaySound(GameConstants.COLLECT_SOUND, finishOpenPitch, 1f);
        Vibrate();
    }

    public void ResetCombo() => stepCombo = 0;

    public void ResetIdle() => idleTime = 0f;

    // Returns true each time a hint should be shown while the player stays idle.
    public bool TickIdle(float deltaTime)
    {
        idleTime += deltaTime;
        if (idleTime < idleHintDelay) return false;
        idleTime = idleHintDelay - idleHintInterval;
        return true;
    }

    // Handheld.Vibrate is a long buzz, so repeated triggers are rate-limited.
    public void Vibrate()
    {
        if (Time.unscaledTime - lastVibrationTime < vibrationCooldown) return;
        lastVibrationTime = Time.unscaledTime;
        VibrationSettings.Vibrate();
    }
}
