using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class Gameplay : UICanvas
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private SpriteRenderer oceanBackground;
    [FormerlySerializedAs("tutorial")]
    [SerializeField] private GameObject tutorialRoot;
    [SerializeField] private GameplayTutorial tutorialController;
    [SerializeField] private GameObject[] winHiddenElements;
    [SerializeField] private GameplayLifeWarning lifeWarning;
    [Header("Target Counter")]
    [SerializeField] private RectTransform targetCounterTransform;
    [SerializeField] private TextMeshProUGUI targetCounterText;
    [SerializeField] private Color targetCounterFlashColor = new Color(1f, 0.78f, 0.2f, 1f);
    [SerializeField, Min(0.01f)] private float targetCounterFlashDuration = 0.28f;
    [SerializeField] private float targetCounterFlashScale = 0.16f;
    private LevelManager levelManager;
    private Vector3 targetCounterBaseScale;
    private Color targetCounterBaseColor;
    private float targetCounterFlashTimer;

    void LateUpdate()
    {
        if (oceanBackground != null && LevelManager.Ins != null)
            LevelManager.Ins.FitGameplayBackground(oceanBackground);
        AnimateTargetCounter();
    }

    void OnEnable()
    {
        levelManager = LevelManager.Ins;
        levelManager.TargetProgressChanged += RefreshTargetCounter;
        levelManager.MissingTargetsBlocked += FlashTargetCounter;
        levelManager.TimeChanged += RefreshTimer;
        InitializeTargetCounter();
    }

    void OnDisable()
    {
        if (levelManager != null) levelManager.TargetProgressChanged -= RefreshTargetCounter;
        if (levelManager != null) levelManager.MissingTargetsBlocked -= FlashTargetCounter;
        if (levelManager != null) levelManager.TimeChanged -= RefreshTimer;
        ResetTargetCounter();
    }

    public override void Open()
    {
        base.Open();
        foreach (GameObject element in winHiddenElements) element.SetActive(true);
        RefreshTutorial();
    }

    public void HideWinHud()
    {
        foreach (GameObject element in winHiddenElements) element.SetActive(false);
        tutorialController.Hide();
    }

    private void RefreshTutorial()
    {
        if (levelText != null)
            levelText.text = $"Level {levelManager.CurrentLevelNumber:00}";
        tutorialController.Refresh();
        RefreshTargetCounter(levelManager.CollectedTargetCount, levelManager.TotalTargetCount);
        RefreshTimer(levelManager.RemainingTime);
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

    private void RefreshTimer(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        timerText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    public void OnClickPause()
    {
        SoundManager.Ins.PlayButtonSound();
        GameManager.Ins.OnPause();
    }

    public override void BackKey()
    {
        if (lifeWarning.IsVisible)
        {
            OnClickCancelRestart();
            return;
        }
        OnClickPause();
    }

    public void OnClickRestart()
    {
        SoundManager.Ins.PlayButtonSound();
        if (!levelManager.HasPlayerMoved)
        {
            levelManager.OnRestart();
            return;
        }
        GameManager.ChangeState(GameState.Pause);
        tutorialController.Hide();
        lifeWarning.ShowRestart();
    }

    public void ShowMainMenuWarning()
    {
        tutorialController.Hide();
        lifeWarning.ShowMainMenu();
    }

    public void OnClickConfirmRestart()
    {
        SoundManager.Ins.PlayButtonSound();
        lifeWarning.Confirm();
    }

    public void OnClickCancelRestart()
    {
        SoundManager.Ins.PlayButtonSound();
        lifeWarning.Cancel();
        tutorialController.Refresh();
    }
}
