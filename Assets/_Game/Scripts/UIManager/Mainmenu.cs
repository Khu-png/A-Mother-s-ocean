using UnityEngine;

public class Mainmenu : UICanvas
{
    public void OnPlayButton()
    {
        LevelManager.Ins.OnReplay();
    }
}
