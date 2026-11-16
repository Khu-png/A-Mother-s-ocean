using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameplayTutorial : MonoBehaviour
{
    private enum TutorialStep
    {
        Move,
        FindBaby,
        Finish,
        Rocks,
        OneWay,
        Whirlpool
    }

    private const int TUTORIAL_LEVEL = 1;
    private const int ROCKS_TUTORIAL_LEVEL = 2;
    private const int PIPES_TUTORIAL_LEVEL = 3;
    [SerializeField] private GameObject tutorialRoot;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image handIcon;
    [SerializeField] private Image focusIcon;
    [SerializeField] private Vector2 handOffset = new Vector2(-20f, -80f);
    [SerializeField, Min(0f)] private float swipeDistance = 190f;
    [SerializeField, Min(0.01f)] private float animationDuration = 1.2f;
    [SerializeField, Min(0f)] private float repeatDelay = 0.3f;
    [SerializeField, Min(0f)] private float focusPulse = 0.08f;
    private LevelManager levelManager;
    private TutorialStep step;
    private Color handColor;
    private float animationTime;
    private bool finishHintDismissed;
    private Vector2 defaultFocusSize;

    void OnEnable()
    {
        levelManager = LevelManager.Ins;
        levelManager.PlayerMoved += HandlePlayerMoved;
        levelManager.PlayerStepCompleted += HandlePlayerStepCompleted;
        levelManager.TargetProgressChanged += HandleTargetProgress;
        handColor = handIcon.color;
        defaultFocusSize = focusIcon.rectTransform.sizeDelta;
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
        if (IsPipesIntro() && levelManager.CurrentLevel.ConsumeTutorialTap())
        {
            AdvancePipesIntro();
            return;
        }
        RefreshFocus();
        AnimateFocus();
        AnimateHand();
    }

    public void Refresh()
    {
        levelManager.CurrentLevel.SetTutorialLock(false);
        if (levelManager.CurrentLevelNumber == ROCKS_TUTORIAL_LEVEL)
        {
            RefreshRocksTutorial();
            return;
        }
        if (levelManager.CurrentLevelNumber == PIPES_TUTORIAL_LEVEL)
        {
            RefreshPipesTutorial();
            return;
        }
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
        levelManager.CurrentLevel.SetTutorialLock(false);
    }

    private void RefreshRocksTutorial()
    {
        bool shouldShow = !levelManager.HasPlayerMoved;
        tutorialRoot.SetActive(shouldShow);
        if (shouldShow) ShowStep(TutorialStep.Rocks, "You can't pass through these rocks!");
    }

    // Two taps of intro before the first move: one-way pipes, then the whirlpool.
    private void RefreshPipesTutorial()
    {
        bool shouldShow = !levelManager.HasPlayerMoved;
        tutorialRoot.SetActive(shouldShow);
        if (!shouldShow) return;
        levelManager.CurrentLevel.SetTutorialLock(true);
        ShowOneWay();
    }

    private void ShowOneWay()
    {
        ShowStep(TutorialStep.OneWay,
            "Pipes only let you pass one way!\n<size=55%>Tap to continue</size>");
    }

    private void ShowWhirlpool()
    {
        ShowStep(TutorialStep.Whirlpool,
            "Step on a whirlpool to turn every pipe!\n<size=55%>Tap to start</size>");
    }

    private bool IsPipesIntro()
    {
        return step == TutorialStep.OneWay || step == TutorialStep.Whirlpool;
    }

    private void AdvancePipesIntro()
    {
        if (step == TutorialStep.OneWay) ShowWhirlpool();
        else Hide();
    }

    private void HandlePlayerMoved()
    {
        if (!tutorialRoot.activeSelf) return;
        if (step == TutorialStep.Move) ShowFindBaby();
        else if (step == TutorialStep.Rocks) tutorialRoot.SetActive(false);
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
        Vector2 anchor = PortraitScreen.ToFrameNormalized(focus.ScreenPosition);
        TutorialHandMotion.PlaceAtAnchor(handIcon.rectTransform, anchor, handOffset);
        TutorialHandMotion.PlaceAtAnchor(focusIcon.rectTransform, anchor, Vector2.zero);
        focusIcon.rectTransform.sizeDelta = FocusSize(focus);
        focusIcon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, focus.RotationDegrees);
        focusIcon.sprite = focus.Sprite;
        focusIcon.color = focus.Color;
    }

    private TutorialFocusKind FocusKind()
    {
        if (step == TutorialStep.Move) return TutorialFocusKind.Player;
        if (step == TutorialStep.Rocks) return TutorialFocusKind.Rock;
        if (step == TutorialStep.OneWay) return TutorialFocusKind.OneWay;
        if (step == TutorialStep.Whirlpool) return TutorialFocusKind.Whirlpool;
        return step == TutorialStep.FindBaby
            ? TutorialFocusKind.Target : TutorialFocusKind.Finish;
    }

    private void AnimateFocus()
    {
        float wave = (Mathf.Sin(Time.unscaledTime * 4f) + 1f) * 0.5f;
        focusIcon.rectTransform.localScale = Vector3.one * (1f + wave * focusPulse);
    }

    private void AnimateHand()
    {
        animationTime += Time.unscaledDeltaTime;
        if (step == TutorialStep.Move)
            TutorialHandMotion.Swipe(handIcon, handColor, handOffset, swipeDistance,
                animationTime, animationDuration, repeatDelay);
        else TutorialHandMotion.Point(handIcon, handColor, handOffset, animationTime);
    }

    // Tiles report their size in screen pixels; convert to canvas units.
    private Vector2 FocusSize(TutorialFocusData focus)
    {
        if (focus.ScreenSize == Vector2.zero) return defaultFocusSize;
        float canvasPerPixel = ((RectTransform)tutorialRoot.transform).rect.width
            / PortraitScreen.FrameRect.width;
        return focus.ScreenSize * canvasPerPixel;
    }
}
