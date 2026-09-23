using UnityEngine;

public class Gameplay : UICanvas
{
    public void OnClickPause()
    {
        GameManager.Ins.OnPause();
    }

    public void OnClickRestart()
    {
        LevelManager.Ins.OnRestart();
    }
}
