using UnityEngine;

public class Win : UICanvas
{
    public void OnNextButton()
    {
        LevelManager.Ins.OnNextLevel();
    }

    public void OnReplayButton()
    {
        LevelManager.Ins.OnReplay();
    }
}
