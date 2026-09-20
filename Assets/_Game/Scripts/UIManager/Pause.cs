using UnityEngine;

public class Pause : UICanvas
{
    public void OnResumeButton()
    {
        GameManager.Ins.OnResume();
    }

    public void OnReplayButton()
    {
        Time.timeScale = 1f;
        LevelManager.Ins.OnReplay();
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        GameManager.Ins.OnInit();
    }
}
