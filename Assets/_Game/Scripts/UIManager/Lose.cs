using UnityEngine;

public class Lose : UICanvas
{
    public void OnClickReplay()
    {
        LevelManager.Ins.OnReplay();
    }

    public void OnClickMainMenu()
    {
        GameManager.Ins.OnInit();
    }
}
