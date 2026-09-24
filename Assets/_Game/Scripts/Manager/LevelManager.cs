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

    private void Awake()
    {
        RegisterSingleton(this);
        currentLevelIndex = levels != null && levels.Length > 0
            ? Mathf.Clamp(SavedLevelNumber - 1, 0, levels.Length - 1)
            : 0;
    }

    private void Start()
    {
    }

    public void OnInit()
    {
        if (currentLevel == null) OnLoadLevel(currentLevelIndex);

        GameManager.Ins.OnPlay();
        if (currentLevel != null)
        {
            currentLevel.SetCameraController(cameraController);
            currentLevel.ResetPlayerToStart();
        }
    }

    public void OnPlay()
    {
        GameManager.Ins.OnPlay();
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
        currentLevel.Initialize(levels[levelIndex]);
    }

    public void OnWin()
    {
        int nextLevel = currentLevelIndex + 1;
        if (!HasLevelData(nextLevel))
        {
            Debug.LogError("không có level tiếp theo");
            throw new UnityException("không có level tiếp theo");
        }

        PlayerPrefs.SetInt(CurrentLevelKey, nextLevel + 1);
        PlayerPrefs.Save();
        GameManager.Ins.OnFinish();
    }

    public void OnLose()
    {
        GameManager.Ins.OnLose();
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
            Debug.LogError("không có level tiếp theo");
            throw new UnityException("không có level tiếp theo");
        }

        OnDespawn();
        currentLevelIndex = nextLevel;
        OnLoadLevel(currentLevelIndex);
        OnInit();
    }

    public void OnRestart() => OnReplay();

    private bool HasLevelData(int index)
    {
        return levels != null && index >= 0 && index < levels.Length && levels[index] != null;
    }
}
