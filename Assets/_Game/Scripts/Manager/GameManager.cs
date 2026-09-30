using UnityEngine;

public enum GameState { MainMenu, Gameplay, Pause, Win }

public class GameManager : Singleton<GameManager>
{
    private static GameState gameState;
    private static float resumeTimeScale = 1f;

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
    }

    public static bool IsState(GameState state) => gameState == state;

    public void OnInit()
    {
        ChangeState(GameState.MainMenu);
        UIManager.Ins.CloseAll();
        UIManager.Ins.OpenUI<Mainmenu>();
    }

    public void OnPlay()
    {
        ChangeState(GameState.Gameplay);
        UIManager.Ins.CloseAll();
        UIManager.Ins.OpenUI<Gameplay>();
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
        ChangeState(GameState.Win);
        UIManager.Ins.OpenUI<Win>();
    }

    private void Awake()
    {
        RegisterSingleton(this);
#if ENABLE_LEGACY_INPUT_MANAGER
        //tranh viec nguoi choi cham da diem vao man hinh
        Input.multiTouchEnabled = false;
#endif
        //target frame rate ve 60 fps
        Application.targetFrameRate = 60;
        //tranh viec tat man hinh
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //xu tai tho
        int maxScreenHeight = 1280;
        float ratio = (float)Screen.currentResolution.width / (float)Screen.currentResolution.height;
        if (Screen.currentResolution.height > maxScreenHeight)
        {
            Screen.SetResolution(Mathf.RoundToInt(ratio * (float)maxScreenHeight), maxScreenHeight, true);
        }
    }

    private void Start()
    {
        OnInit();
        //UIManager.Ins.OpenUI<UIMainMenu>();
    }

    private void OnDestroy()
    {
        if (IsState(GameState.Pause)) ChangeState(GameState.MainMenu);
    }
}
