using UnityEngine;

public class Lose : UICanvas
{
    public void OnClickReplay()
    {
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.OnReplay();
    }

    public void OnClickMainMenu()
    {
        SoundManager.Ins.PlayButtonSound();
        GameManager.Ins.OnInit();
    }
}
