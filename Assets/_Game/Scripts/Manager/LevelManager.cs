using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] private Level[] levels;
    [SerializeField] private Level sceneLevel;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private int level = 0;
    private Level currentLevel;

    private void Awake()
    {
        RegisterSingleton(this);
    }

    private void Start()
    {
    }

    public void OnInit()
    {
        if (currentLevel == null)
        {
            OnLoadLevel(level);
        }

        GameManager.Ins.OnPlay();
        if (currentLevel != null)
        {
            currentLevel.OnInit(cameraController);
        }
    }

    public void OnPlay()
    {
        GameManager.Ins.OnPlay();
    }

    public void OnDespawn()
    {
        if (currentLevel != null && currentLevel != sceneLevel)
        {
            currentLevel.OnDespawn();
        }

        currentLevel = null;
    }

    public void OnLoadLevel(int levelIndex)
    {
        if (HasLevelPrefab(levelIndex))
        {
            currentLevel = Instantiate(levels[levelIndex], transform);
        }
        else
        {
            currentLevel = sceneLevel;
        }
    }

    public void OnWin()
    {
        GameManager.Ins.OnFinish();
    }

    public void OnLose()
    {
        GameManager.Ins.OnLose();
    }

    public void OnReplay()
    {
        OnDespawn();
        OnLoadLevel(level);
        OnInit();
    }

    public void OnNextLevel()
    {
        OnDespawn();
        OnLoadLevel(++level);
        OnInit();
    }

    private bool HasLevelPrefab(int index)
    {
        return levels != null && index >= 0 && index < levels.Length && levels[index] != null;
    }
}
