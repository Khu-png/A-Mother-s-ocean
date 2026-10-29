using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Gameplay : UICanvas
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private SpriteRenderer oceanBackground;
    [SerializeField] private GameObject tutorial;
    [SerializeField] private GameObject[] winHiddenElements;
    [Header("Target Counter")]
    [SerializeField] private RectTransform targetCounterTransform;
    [SerializeField] private TextMeshProUGUI targetCounterText;
    [SerializeField] private Color targetCounterFlashColor = new Color(1f, 0.78f, 0.2f, 1f);
    [SerializeField, Min(0.01f)] private float targetCounterFlashDuration = 0.28f;
    [SerializeField] private float targetCounterFlashScale = 0.16f;
    [Header("Swipe Tutorial")]
    [SerializeField] private Image swipeIcon;
    [SerializeField, Min(0f)] private float swipeDistance = 70f;
    [SerializeField, Min(0.01f)] private float swipeDuration = 0.65f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.35f;
    [SerializeField, Min(0f)] private float repeatDelay = 0.25f;
    private LevelManager levelManager;
    private Vector2 swipeStartPosition;
    private Color swipeStartColor;
    private float swipeTimer;
    private bool swipeInitialized;
    private RectTransform swipeTransform;
    private Vector3 targetCounterBaseScale;
    private Color targetCounterBaseColor;
    private float targetCounterFlashTimer;

    void LateUpdate()
    {
        if (oceanBackground != null && LevelManager.Ins != null)
            LevelManager.Ins.FitGameplayBackground(oceanBackground);
        AnimateSwipeIcon();
        AnimateTargetCounter();
    }

    void OnEnable()
    {
        levelManager = LevelManager.Ins;
        levelManager.PlayerMoved += HideTutorial;
        levelManager.TargetProgressChanged += RefreshTargetCounter;
        levelManager.MissingTargetsBlocked += FlashTargetCounter;
        InitializeTargetCounter();
        RefreshTutorial();
    }

    void OnDisable()
    {
        if (levelManager != null) levelManager.PlayerMoved -= HideTutorial;
        if (levelManager != null) levelManager.TargetProgressChanged -= RefreshTargetCounter;
        if (levelManager != null) levelManager.MissingTargetsBlocked -= FlashTargetCounter;
        ResetSwipeIcon();
        ResetTargetCounter();
    }

    public override void Open()
    {
        base.Open();
        foreach (GameObject element in winHiddenElements) element.SetActive(true);
        RefreshTutorial();
        if (tutorial == null)
            GameDiagnostics.LogWarning("Gameplay: hãy gán object Tutorial vào field Tutorial trong prefab.", this);
        if (swipeIcon == null)
            GameDiagnostics.LogWarning("Gameplay: hãy gán Image bàn tay vào field Swipe Icon trong prefab.", this);
    }

    public void HideWinHud()
    {
        foreach (GameObject element in winHiddenElements) element.SetActive(false);
        tutorial.SetActive(false);
    }

    private void RefreshTutorial()
    {
        if (levelText != null)
            levelText.text = $"Level {levelManager.CurrentLevelNumber:00}";
        InitializeSwipeIcon();
        if (tutorial != null)
            tutorial.SetActive(levelManager.CurrentLevelNumber == 1 && !levelManager.HasPlayerMoved);
        RefreshTargetCounter(levelManager.CollectedTargetCount, levelManager.TotalTargetCount);
    }

    private void HideTutorial()
    {
        if (tutorial != null) tutorial.SetActive(false);
        ResetSwipeIcon();
    }

    private void InitializeSwipeIcon()
    {
        if (swipeIcon == null) return;
        if (!swipeInitialized)
        {
            swipeTransform = swipeIcon.rectTransform;
            swipeStartPosition = swipeTransform.anchoredPosition;
            swipeStartColor = swipeIcon.color;
            swipeInitialized = true;
        }
        swipeIcon.raycastTarget = false;
        ResetSwipeIcon();
    }

    private void AnimateSwipeIcon()
    {
        if (!swipeInitialized || swipeIcon == null || tutorial == null) return;
        if (!tutorial.activeInHierarchy || !GameManager.IsState(GameState.Gameplay)) return;

        float moveTime = Mathf.Max(0.01f, swipeDuration);
        float fadeTime = Mathf.Max(0.01f, fadeDuration);
        float cycleTime = moveTime + fadeTime + Mathf.Max(0f, repeatDelay);
        swipeTimer = (swipeTimer + Time.deltaTime) % cycleTime;
        float movement = Mathf.SmoothStep(0f, 1f, swipeTimer / moveTime);
        swipeTransform.anchoredPosition = swipeStartPosition + Vector2.right * swipeDistance * movement;
        Color color = swipeStartColor;
        color.a *= 1f - Mathf.Clamp01((swipeTimer - moveTime) / fadeTime);
        swipeIcon.color = color;
    }

    private void ResetSwipeIcon()
    {
        swipeTimer = 0f;
        if (!swipeInitialized || swipeIcon == null) return;
        swipeTransform.anchoredPosition = swipeStartPosition;
        swipeIcon.color = swipeStartColor;
    }

    private void InitializeTargetCounter()
    {
        targetCounterBaseScale = targetCounterTransform.localScale;
        targetCounterBaseColor = targetCounterText.color;
        ResetTargetCounter();
    }

    private void RefreshTargetCounter(int collected, int total)
    {
        targetCounterText.text = $"{collected}/{total}";
    }

    private void FlashTargetCounter()
    {
        targetCounterFlashTimer = targetCounterFlashDuration;
    }

    private void AnimateTargetCounter()
    {
        if (targetCounterFlashTimer <= 0f) return;
        targetCounterFlashTimer = Mathf.Max(0f,
            targetCounterFlashTimer - Time.unscaledDeltaTime);
        float progress = 1f - targetCounterFlashTimer / targetCounterFlashDuration;
        float pulse = Mathf.Sin(progress * Mathf.PI);
        targetCounterTransform.localScale = targetCounterBaseScale
            * (1f + pulse * targetCounterFlashScale);
        targetCounterText.color = Color.Lerp(targetCounterBaseColor,
            targetCounterFlashColor, pulse);
        if (targetCounterFlashTimer <= 0f) ResetTargetCounter();
    }

    private void ResetTargetCounter()
    {
        targetCounterFlashTimer = 0f;
        targetCounterTransform.localScale = targetCounterBaseScale;
        targetCounterText.color = targetCounterBaseColor;
    }

    public void OnClickPause()
    {
        SoundManager.Ins.PlayButtonSound();
        GameManager.Ins.OnPause();
    }

    public override void BackKey()
    {
        OnClickPause();
    }

    public void OnClickRestart()
    {
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.OnRestart();
    }
}
