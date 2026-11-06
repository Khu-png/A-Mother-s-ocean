using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameplayTutorial : MonoBehaviour
{
    private enum TutorialStep
    {
        Move,
        FindBaby,
        Finish
    }

    private const int TUTORIAL_LEVEL = 1;
    [SerializeField] private GameObject tutorialRoot;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image handIcon;
    [SerializeField] private Image focusGlow;
    [SerializeField] private Image focusIcon;
    [SerializeField] private Vector2 handOffset = new Vector2(-55f, -105f);
    [SerializeField, Min(0f)] private float swipeDistance = 190f;
    [SerializeField, Min(0.01f)] private float animationDuration = 1.2f;
    [SerializeField, Min(0f)] private float repeatDelay = 0.3f;
    [SerializeField, Min(0f)] private float focusPulse = 0.08f;
    private LevelManager levelManager;
    private TutorialStep step;
    private Color handColor;
    private float animationTime;
    private bool finishHintDismissed;

    void OnEnable()
    {
        levelManager = LevelManager.Ins;
        levelManager.PlayerMoved += HandlePlayerMoved;
        levelManager.PlayerStepCompleted += HandlePlayerStepCompleted;
        levelManager.TargetProgressChanged += HandleTargetProgress;
        handColor = handIcon.color;
    }

    void OnDisable()
    {
        levelManager.PlayerMoved -= HandlePlayerMoved;
        levelManager.PlayerStepCompleted -= HandlePlayerStepCompleted;
        levelManager.TargetProgressChanged -= HandleTargetProgress;
    }

    void LateUpdate()
    {
        if (!tutorialRoot.activeSelf) return;
        RefreshFocus();
        AnimateFocus();
        AnimateHand();
    }

    public void Refresh()
    {
        if (levelManager.CollectedTargetCount == 0) finishHintDismissed = false;
        bool shouldShow = levelManager.CurrentLevelNumber == TUTORIAL_LEVEL
            && !finishHintDismissed;
        tutorialRoot.SetActive(shouldShow);
        if (!shouldShow) return;

        if (levelManager.CollectedTargetCount > 0) ShowFinish();
        else if (levelManager.HasPlayerMoved) ShowFindBaby();
        else ShowMove();
    }

    public void Hide()
    {
        tutorialRoot.SetActive(false);
    }

    private void HandlePlayerMoved()
    {
        if (step == TutorialStep.Move) ShowFindBaby();
    }

    private void HandlePlayerStepCompleted()
    {
        if (step != TutorialStep.Finish) return;
        finishHintDismissed = true;
        tutorialRoot.SetActive(false);
    }

    private void HandleTargetProgress(int collected, int total)
    {
        if (total > 0 && collected == total) ShowFinish();
    }

    private void ShowMove()
    {
        ShowStep(TutorialStep.Move, "Swipe right to move");
    }

    private void ShowFindBaby()
    {
        ShowStep(TutorialStep.FindBaby, "Find the baby shark");
    }

    private void ShowFinish()
    {
        ShowStep(TutorialStep.Finish, "Reach the finish line");
    }

    private void ShowStep(TutorialStep nextStep, string message)
    {
        step = nextStep;
        dialogueText.text = message;
        animationTime = 0f;
        handIcon.color = handColor;
        RefreshFocus();
    }

    private void RefreshFocus()
    {
        TutorialFocusData focus = levelManager.GetTutorialFocus(FocusKind());
        Vector2 anchor = new Vector2(focus.ScreenPosition.x / Screen.width,
            focus.ScreenPosition.y / Screen.height);
        PlaceAtAnchor(focusIcon.rectTransform, anchor, Vector2.zero);
        PlaceAtAnchor(focusGlow.rectTransform, anchor, Vector2.zero);
        PlaceAtAnchor(handIcon.rectTransform, anchor, handOffset);
        focusIcon.sprite = focus.Sprite;
        focusIcon.color = focus.Color;
        focusGlow.sprite = focus.Sprite;
    }

    private TutorialFocusKind FocusKind()
    {
        if (step == TutorialStep.Move) return TutorialFocusKind.Player;
        return step == TutorialStep.FindBaby
            ? TutorialFocusKind.Target : TutorialFocusKind.Finish;
    }

    private void AnimateFocus()
    {
        float wave = (Mathf.Sin(Time.unscaledTime * 4f) + 1f) * 0.5f;
        focusIcon.rectTransform.localScale = Vector3.one * (1f + wave * focusPulse);
        focusGlow.rectTransform.localScale = Vector3.one * (1.12f + wave * focusPulse);
    }

    private void AnimateHand()
    {
        animationTime += Time.unscaledDeltaTime;
        if (step == TutorialStep.Move) AnimateSwipe();
        else AnimatePoint();
    }

    private void AnimateSwipe()
    {
        float cycle = animationDuration + repeatDelay;
        float progress = Mathf.Clamp01(animationTime % cycle / animationDuration);
        float movement = Mathf.SmoothStep(0f, 1f, progress);
        handIcon.rectTransform.anchoredPosition = handOffset
            + Vector2.right * (swipeDistance * movement);
        Color color = handColor;
        color.a *= 1f - Mathf.Clamp01((progress - 0.72f) / 0.28f);
        handIcon.color = color;
    }

    private void AnimatePoint()
    {
        float pulse = (Mathf.Sin(animationTime * 4.5f) + 1f) * 0.5f;
        handIcon.rectTransform.anchoredPosition = handOffset + Vector2.up * (pulse * 12f);
        handIcon.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1.05f, pulse);
        handIcon.color = handColor;
    }

    private static void PlaceAtAnchor(RectTransform rect, Vector2 anchor,
        Vector2 offset)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = offset;
    }
}
