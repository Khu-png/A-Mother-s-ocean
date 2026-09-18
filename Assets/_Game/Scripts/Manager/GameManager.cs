using UnityEngine;

public enum GameState { MainMenu, Gameplay, Pause, Win, Lose }

public class GameManager : Singleton<GameManager>
{
    private static GameState gameState;

    public static void ChangeState(GameState state)
    {
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
        Time.timeScale = 1f;
        ChangeState(GameState.Gameplay);
        UIManager.Ins.CloseAll();
        UIManager.Ins.OpenUI<Gameplay>();
    }

    public void OnPause()
    {
        Time.timeScale = 0f;
        ChangeState(GameState.Pause);
        UIManager.Ins.OpenUI<Pause>();
    }

    public void OnResume()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.Gameplay);
        UIManager.Ins.CloseUI<Pause>();
    }

    public void OnFinish()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.Win);
        UIManager.Ins.OpenUI<Win>();
    }

    public void OnLose()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.Lose);
        UIManager.Ins.OpenUI<Lose>();
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
}
