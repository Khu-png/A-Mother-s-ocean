using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameplayPowerUps : MonoBehaviour
{
    private const float FREEZE_DURATION = 30f;
    private const float HAMMER_DIM_ALPHA = 0.82f;
    [SerializeField] private Button freezeButton;
    [SerializeField] private Button hammerButton;
    [SerializeField] private TMP_Text freezeCountText;
    [SerializeField] private TMP_Text hammerCountText;
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private RectTransform freezeIconHome;
    [SerializeField] private RectTransform freezeIcon;
    [SerializeField] private Image freezeIconImage;
    [SerializeField] private RectTransform hammerIconHome;
    [SerializeField] private RectTransform hammerIcon;
    [SerializeField] private Image hammerIconImage;
    [SerializeField] private PowerUpFeedbackGraphic feedbackGraphic;
    [SerializeField] private HammerImpactGraphic hammerImpactGraphic;
    [SerializeField] private CanvasGroup hammerSelectionDim;
    [SerializeField] private HammerSelectionOverlayGraphic hammerSelectionOverlay;
    private readonly List<Vector2> hammerTargetPositions = new();
    private PowerUpIconMotion.HomeState freezeHomeState;
    private PowerUpIconMotion.HomeState hammerHomeState;
    private Coroutine freezeIntroRoutine;
    private Coroutine hammerRoutine;
    private bool hammerSelecting;
    private bool freezeVisualActive;

    void Awake()
    {
        freezeHomeState = PowerUpIconMotion.Capture(freezeIcon, freezeIconImage);
        hammerHomeState = PowerUpIconMotion.Capture(hammerIcon, hammerIconImage);
        feedbackGraphic.raycastTarget = false;
    }

    void OnEnable()
    {
        LevelManager.Ins.FreezeChanged += OnFreezeChanged;
        LevelManager.Ins.FreezeEnded += OnFreezeEnded;
        LevelManager.Ins.HammerTargetSelected += OnHammerTargetSelected;
        freezeVisualActive = false;
        hammerSelectionDim.alpha = 0f;
        hammerSelectionOverlay.Hide();
        feedbackGraphic.ResetFeedback();
        hammerImpactGraphic.ResetImpact();
        RestoreIcons();
        RefreshButtons();
    }

    void Update()
    {
        if (hammerSelecting && !GameManager.IsState(GameState.Gameplay)) CancelHammerSelection();
        UpdateHammerSelectionDim();
    }

    void OnDisable()
    {
        LevelManager.Ins.FreezeChanged -= OnFreezeChanged;
        LevelManager.Ins.FreezeEnded -= OnFreezeEnded;
        LevelManager.Ins.HammerTargetSelected -= OnHammerTargetSelected;
        LevelManager.Ins.CancelHammerSelection();
        if (freezeIntroRoutine != null) StopCoroutine(freezeIntroRoutine);
        if (hammerRoutine != null) StopCoroutine(hammerRoutine);
        freezeIntroRoutine = null;
        hammerRoutine = null;
        hammerSelecting = false;
        freezeVisualActive = false;
        hammerSelectionDim.alpha = 0f;
        hammerSelectionOverlay.Hide();
        RestoreIcons();
        feedbackGraphic.ResetFeedback();
        hammerImpactGraphic.ResetImpact();
    }

    public void OnClickFreeze()
    {
        SoundManager.Ins.PlayButtonSound();
        if (!CanUse(MenuPowerUpId.Freeze) || !LevelManager.Ins.CanActivateFreeze()) return;
        SoundManager.Ins.PlaySound(GameConstants.FREEZE_SOUND);
        DisableButtons();
        freezeIntroRoutine = StartCoroutine(PlayFreezeIntro());
    }

    public void OnClickHammer()
    {
        SoundManager.Ins.PlayButtonSound();
        if (!CanUse(MenuPowerUpId.Hammer) || !LevelManager.Ins.BeginHammerSelection()) return;
        hammerSelecting = true;
        ShowHammerTargets();
        DisableButtons();
    }

    private IEnumerator PlayFreezeIntro()
    {
        yield return PowerUpIconMotion.FreezeIntro(freezeIcon, freezeIconImage, uiRoot);
        if (!GameManager.IsState(GameState.Gameplay))
        {
            RestoreFreezeIcon();
            freezeIntroRoutine = null;
            yield break;
        }
        MenuPlayerProgress.TryUsePowerUp(MenuPowerUpId.Freeze);
        LevelManager.Ins.ActivateFreeze(FREEZE_DURATION);
        RestoreFreezeIcon();
        freezeIntroRoutine = null;
        RefreshButtons();
    }

    private void OnFreezeChanged(float remaining, float duration)
    {
        if (!freezeVisualActive)
        {
            freezeVisualActive = true;
            feedbackGraphic.BeginFreeze();
        }
        feedbackGraphic.SetFreezeProgress(remaining / duration);
    }

    private void OnFreezeEnded()
    {
        freezeVisualActive = false;
        feedbackGraphic.EndFreeze();
        RefreshButtons();
    }

    private void OnHammerTargetSelected(BlockTile target, Vector2 screenPosition)
    {
        hammerSelecting = false;
        hammerSelectionOverlay.Hide();
        MenuPlayerProgress.TryUsePowerUp(MenuPowerUpId.Hammer);
        RefreshCounts();
        hammerRoutine = StartCoroutine(PlayHammer(target, screenPosition));
    }

    private IEnumerator PlayHammer(BlockTile target, Vector2 screenPosition)
    {
        Vector2 destination = PowerUpIconMotion.LocalPoint(uiRoot, screenPosition);
        SoundManager.Ins.PlaySound(GameConstants.HAMMER_SOUND);
        yield return PowerUpIconMotion.HammerStrike(hammerIcon, uiRoot, destination);
        hammerImpactGraphic.Play(destination);
        LevelManager.Ins.ResolveHammerTarget(target);
        yield return PowerUpIconMotion.HammerRetreat(hammerIcon);
        RestoreHammerIcon();
        hammerRoutine = null;
        RefreshButtons();
    }

    private void CancelHammerSelection()
    {
        hammerSelecting = false;
        hammerSelectionOverlay.Hide();
        LevelManager.Ins.CancelHammerSelection();
        RefreshButtons();
    }

    private void DisableButtons()
    {
        freezeButton.interactable = false;
        hammerButton.interactable = false;
    }

    private void UpdateHammerSelectionDim()
    {
        float target = hammerSelecting ? HAMMER_DIM_ALPHA : 0f;
        hammerSelectionDim.alpha = Mathf.MoveTowards(hammerSelectionDim.alpha,
            target, Time.unscaledDeltaTime * 5f);
    }

    private void ShowHammerTargets()
    {
        LevelManager.Ins.FillHammerTargetScreenPositions(hammerTargetPositions);
        for (int i = 0; i < hammerTargetPositions.Count; i++)
            hammerTargetPositions[i] = PowerUpIconMotion.LocalPoint(uiRoot,
                hammerTargetPositions[i]);
        hammerSelectionOverlay.Show(hammerTargetPositions);
    }

    private void RefreshButtons()
    {
        RefreshCounts();
        bool free = GameManager.IsState(GameState.Gameplay)
            && freezeIntroRoutine == null && hammerRoutine == null && !hammerSelecting;
        freezeButton.interactable = free && !LevelManager.Ins.IsTimeFrozen
            && CanUse(MenuPowerUpId.Freeze);
        hammerButton.interactable = free && CanUse(MenuPowerUpId.Hammer);
    }

    private void RefreshCounts()
    {
        freezeCountText.text = MenuPlayerProgress.PowerUpCount(MenuPowerUpId.Freeze).ToString();
        hammerCountText.text = MenuPlayerProgress.PowerUpCount(MenuPowerUpId.Hammer).ToString();
    }

    private static bool CanUse(MenuPowerUpId id)
    {
        return MenuPlayerProgress.PowerUpCount(id) > 0;
    }

    private void RestoreIcons()
    {
        RestoreFreezeIcon();
        RestoreHammerIcon();
    }

    private void RestoreFreezeIcon()
    {
        PowerUpIconMotion.Restore(freezeIcon, freezeIconImage,
            freezeIconHome, freezeHomeState);
    }

    private void RestoreHammerIcon()
    {
        PowerUpIconMotion.Restore(hammerIcon, hammerIconImage,
            hammerIconHome, hammerHomeState);
    }
}
