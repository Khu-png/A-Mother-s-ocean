using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public const string CurrentLevelKey = "CurrentLevel";
    public static int SavedLevelNumber => Mathf.Max(1, PlayerPrefs.GetInt(CurrentLevelKey, 1));
    [SerializeField] private LevelData[] levels;
    [SerializeField] private GridMap levelMap;
    [SerializeField] private CameraController cameraController;
    private int currentLevelIndex;
    private GridMap currentLevel;
    private float remainingTime;
    private float freezeRemaining;
    private float freezeDuration;
    private int displayedSecond;
    public int CurrentLevelNumber => currentLevelIndex + 1;
    public bool HasPlayerMoved { get; private set; }
    public int CollectedTargetCount { get; private set; }
    public int TotalTargetCount { get; private set; }
    public float RemainingTime => remainingTime;
    public float FreezeRemaining => freezeRemaining;
    public bool IsTimeFrozen => freezeRemaining > 0f;
    public event System.Action PlayerMoved;
    public event System.Action PlayerStepCompleted;
    public event System.Action<int, int> TargetProgressChanged;
    public event System.Action MissingTargetsBlocked;
    public event System.Action<float> TimeChanged;
    public event System.Action<float, float> FreezeChanged;
    public event System.Action FreezeEnded;
    public event System.Action<BlockTile, Vector2> HammerTargetSelected;

    void Awake()
    {
        RegisterSingleton(this);
        currentLevelIndex = levels != null && levels.Length > 0
            ? Mathf.Clamp(SavedLevelNumber - 1, 0, levels.Length - 1)
            : 0;
    }

    void Update()
    {
        if (!GameManager.IsState(GameState.Gameplay)) return;
        float levelDelta = ApplyFreeze(Time.deltaTime);
        if (!HasPlayerMoved) return;
        remainingTime = Mathf.Max(0f, remainingTime - levelDelta);
        int currentSecond = Mathf.CeilToInt(remainingTime);
        if (currentSecond != displayedSecond)
        {
            displayedSecond = currentSecond;
            TimeChanged?.Invoke(remainingTime);
        }
        if (remainingTime <= 0f) GameManager.Ins.OnLose();
    }

    public void NotifyPlayerMoved()
    {
        if (HasPlayerMoved) return;
        HasPlayerMoved = true;
        PlayerMoved?.Invoke();
    }

    public void NotifyPlayerStepCompleted()
    {
        PlayerStepCompleted?.Invoke();
    }

    public bool CanActivateFreeze()
    {
        return GameManager.IsState(GameState.Gameplay)
            && remainingTime > 0f && !IsTimeFrozen
            && !currentLevel.IsPowerUpBusy;
    }

    public void ActivateFreeze(float duration)
    {
        freezeDuration = duration;
        freezeRemaining = duration;
        FreezeChanged?.Invoke(freezeRemaining, freezeDuration);
    }

    public bool BeginHammerSelection()
    {
        if (!GameManager.IsState(GameState.Gameplay)) return false;
        return currentLevel.BeginHammerSelection(NotifyHammerTargetSelected);
    }

    public void FillHammerTargetScreenPositions(List<Vector2> positions)
    {
        currentLevel.FillHammerTargetScreenPositions(positions);
    }

    public void ResolveHammerTarget(BlockTile target)
    {
        currentLevel.BreakBlock(target);
    }

    public void CancelHammerSelection()
    {
        if (currentLevel != null) currentLevel.CancelHammerSelection();
    }

    public void NotifyTargetProgress(int collected, int total)
    {
        CollectedTargetCount = collected;
        TotalTargetCount = total;
        TargetProgressChanged?.Invoke(collected, total);
    }

    public void NotifyMissingTargetsBlocked()
    {
        MissingTargetsBlocked?.Invoke();
    }

    public void FitGameplayBackground(SpriteRenderer background)
    {
        if (cameraController != null) cameraController.FitBackground(background);
    }

    public TutorialFocusData GetTutorialFocus(TutorialFocusKind kind)
    {
        return currentLevel.GetTutorialFocus(kind);
    }

    public void OnInit()
    {
        if (currentLevel == null) OnLoadLevel(currentLevelIndex);

        GameManager.Ins.ShowGameplay();
        if (currentLevel != null)
        {
            currentLevel.SetCameraController(cameraController);
            currentLevel.ResetPlayerToStart();
        }
    }

    public void OnPlay()
    {
        int savedLevelIndex = SavedLevelNumber - 1;
        if (!HasLevelData(savedLevelIndex))
            throw new UnityException("Không có dữ liệu level đã lưu.");

        LoadingManager.Ins.LoadGameplay(savedLevelIndex);
    }

    public void OnDespawn()
    {
        if (currentLevel != null) currentLevel.ClearRuntimeMap();
    }

    public void OnLoadLevel(int levelIndex)
    {
        if (!HasLevelData(levelIndex)) throw new UnityException("Không có dữ liệu level hợp lệ.");
        if (levelMap == null) throw new UnityException("Scene Grid Map chưa được gán.");
        currentLevelIndex = levelIndex;
        currentLevel = levelMap;
        currentLevel.SetCameraController(cameraController);
        currentLevel.Initialize(levels[levelIndex]);
        HasPlayerMoved = false;
        freezeRemaining = 0f;
        freezeDuration = 0f;
        remainingTime = levels[levelIndex].TimeLimitSeconds;
        displayedSecond = Mathf.CeilToInt(remainingTime);
        TimeChanged?.Invoke(remainingTime);
    }

    public void OnWin()
    {
        GameManager.Ins.OnFinish();
    }

    public int PendingWinCoins => levels[currentLevelIndex].CompletionCoinReward;

    public void ClaimWinReward()
    {
        LevelData completedLevel = levels[currentLevelIndex];
        MenuPlayerProgress.GrantCompletionCoins(completedLevel.CompletionCoinReward);
        if (completedLevel.GrantsGiftChest) MenuPlayerProgress.GrantChestForLevel(CurrentLevelNumber);
        int nextIndex = currentLevelIndex + 1;
        if (HasLevelData(nextIndex)) PlayerPrefs.SetInt(CurrentLevelKey, nextIndex + 1);
        PlayerPrefs.Save();
    }

    public void ContinueAfterWin()
    {
        if (HasLevelData(currentLevelIndex + 1)) LoadingManager.Ins.LoadGameplay(currentLevelIndex + 1);
        else LoadingManager.Ins.LoadMainMenu();
    }

    public void PlayWinTransition(System.Action completed)
    {
        currentLevel.PlayWinTransition(completed);
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt(CurrentLevelKey, 1);
        PlayerPrefs.Save();
        currentLevelIndex = 0;
    }

    public void OnReplay()
    {
        OnDespawn();
        OnLoadLevel(currentLevelIndex);
        OnInit();
    }

    public void OnNextLevel()
    {
        int nextLevel = SavedLevelNumber - 1;
        if (!HasLevelData(nextLevel))
        {
            GameDiagnostics.LogError("không có level tiếp theo");
            throw new UnityException("không có level tiếp theo");
        }

        OnDespawn();
        OnLoadLevel(nextLevel);
        OnInit();
    }

    public void OnRestart() => OnReplay();

    private bool HasLevelData(int index)
    {
        return levels != null && index >= 0 && index < levels.Length && levels[index] != null;
    }

    private float ApplyFreeze(float delta)
    {
        if (freezeRemaining <= 0f) return delta;
        float frozenDelta = Mathf.Min(freezeRemaining, delta);
        freezeRemaining -= frozenDelta;
        FreezeChanged?.Invoke(freezeRemaining, freezeDuration);
        if (freezeRemaining <= 0f) FreezeEnded?.Invoke();
        return delta - frozenDelta;
    }

    private void NotifyHammerTargetSelected(BlockTile target, Vector2 screenPosition)
    {
        HammerTargetSelected?.Invoke(target, screenPosition);
    }
}
