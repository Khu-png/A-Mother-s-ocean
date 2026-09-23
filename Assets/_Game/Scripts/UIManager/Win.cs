using UnityEngine;

public class Win : UICanvas
{
    public void OnClickNext()
    {
        LevelManager.Ins.OnNextLevel();
    }

    public void OnClickReplay()
    {
        LevelManager.Ins.OnReplay();
    }
}
