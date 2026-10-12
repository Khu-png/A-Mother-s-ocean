using UnityEngine;

public enum GameState { MainMenu, Gameplay, Pause, Win }

public class GameManager : Singleton<GameManager>
{
    private static GameState gameState;
    private static float resumeTimeScale = 1f;

    void Awake()
    {
        RegisterSingleton(this);
#if ENABLE_LEGACY_INPUT_MANAGER
        Input.multiTouchEnabled = false;
#endif
        Application.targetFrameRate = GameConstants.TARGET_FRAME_RATE;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        OnInit();
    }

    void OnDestroy()
    {
        if (IsState(GameState.Pause)) Time.timeScale = resumeTimeScale;
    }

    public static void ChangeState(GameState state)
    {
        if (state == GameState.Pause && gameState != GameState.Pause)
        {
            resumeTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else if (gameState == GameState.Pause && state != GameState.Pause)
        {
            Time.timeScale = resumeTimeScale;
        }
        gameState = state;
        PlayStateMusic(state);
    }

    private static void PlayStateMusic(GameState state)
    {
        if (SoundManager.Ins == null) return;
        if (state == GameState.Gameplay) SoundManager.Ins.PlayMusic(GameConstants.GAMEPLAY_MUSIC);
    }

    public static bool IsState(GameState state) => gameState == state;

    public void OnInit()
    {
        LoadingManager.Ins.LoadMainMenu();
    }

    public void OnPlay()
    {
        if (LoadingManager.Ins.IsLoading) return;
        LevelManager.Ins.OnPlay();
    }

    internal void ShowGameplay()
    {
        ChangeState(GameState.Gameplay);
        UIManager.Ins.CloseAll();
        if (UIManager.Ins.OpenUI<Gameplay>() == null)
            throw new UnityException("Gameplay prefab is missing from UIManager.");
    }

    public void OnPause()
    {
        if (!IsState(GameState.Gameplay)) return;
        if (UIManager.Ins.OpenUI<Pause>() == null) return;
        ChangeState(GameState.Pause);
    }

    public void OnResume()
    {
        if (!IsState(GameState.Pause)) return;
        ChangeState(GameState.Gameplay);
        UIManager.Ins.CloseUI<Pause>();
    }

    public void OnFinish()
    {
        if (IsState(GameState.Win)) return;
        ChangeState(GameState.Win);
        SoundManager.Ins.PlaySound(GameConstants.WIN_SOUND);
        UIManager.Ins.OpenUI<Win>();
    }
}
