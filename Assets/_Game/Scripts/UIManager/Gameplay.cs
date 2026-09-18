using UnityEngine;

public class Gameplay : UICanvas
{
    public void OnPauseButton()
    {
        GameManager.Ins.OnPause();
    }

    public void OnReplayButton()
    {
        LevelManager.Ins.OnReplay();
    }
}
