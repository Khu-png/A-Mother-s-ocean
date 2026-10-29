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
    public int CurrentLevelNumber => currentLevelIndex + 1;
    public bool HasPlayerMoved { get; private set; }
    public int CollectedTargetCount { get; private set; }
    public int TotalTargetCount { get; private set; }
    public event System.Action PlayerMoved;
    public event System.Action<int, int> TargetProgressChanged;
    public event System.Action MissingTargetsBlocked;

    void Awake()
    {
        RegisterSingleton(this);
        currentLevelIndex = levels != null && levels.Length > 0
            ? Mathf.Clamp(SavedLevelNumber - 1, 0, levels.Length - 1)
            : 0;
    }

    public void NotifyPlayerMoved()
    {
        if (HasPlayerMoved) return;
        HasPlayerMoved = true;
        PlayerMoved?.Invoke();
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
        currentLevel = levelMap;
        currentLevel.SetCameraController(cameraController);
        currentLevel.Initialize(levels[levelIndex]);
        currentLevelIndex = levelIndex;
        HasPlayerMoved = false;
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
}
