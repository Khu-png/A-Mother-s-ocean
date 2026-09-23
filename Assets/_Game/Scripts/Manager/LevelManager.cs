using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public const string CurrentLevelKey = "CurrentLevel";
    public static int SavedLevelNumber => Mathf.Max(1, PlayerPrefs.GetInt(CurrentLevelKey, 1));
    [SerializeField] private GridMap[] levels;
    [SerializeField] private CameraController cameraController;
    private int currentLevelIndex;
    private GridMap currentLevel;

    private void Awake()
    {
        RegisterSingleton(this);
        currentLevelIndex = SavedLevelNumber - 1;
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
        if (currentLevel != null) Destroy(currentLevel.gameObject);
        currentLevel = null;
    }

    public void OnLoadLevel(int levelIndex)
    {
        if (!HasLevelPrefab(levelIndex)) throw new UnityException("Không có level hợp lệ.");
        currentLevel = Instantiate(levels[levelIndex], transform);
    }

    public void OnWin()
    {
        int nextLevel = currentLevelIndex + 1;
        if (!HasLevelPrefab(nextLevel))
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
        if (!HasLevelPrefab(nextLevel))
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

    private bool HasLevelPrefab(int index)
    {
        return levels != null && index >= 0 && index < levels.Length && levels[index] != null;
    }
}
