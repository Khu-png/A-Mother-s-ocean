using UnityEngine;

public class Pause : UICanvas
{
    public void OnClickResume()
    {
        GameManager.Ins.OnResume();
    }

    public void OnClickReplay()
    {
        Time.timeScale = 1f;
        LevelManager.Ins.OnReplay();
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Ins.OnInit();
    }
}
