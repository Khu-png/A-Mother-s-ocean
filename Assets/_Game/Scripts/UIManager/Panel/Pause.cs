using UnityEngine;

public class Pause : OptionsPanel
{
    public override void BackKey()
    {
        OnClickResume();
    }

    public void OnClickResume()
    {
        CloseAnimated(GameManager.Ins.OnResume);
    }

    public void OnClickReplay()
    {
        CloseAnimated(LevelManager.Ins.OnReplay);
    }

    public void OnClickMainMenu()
    {
        CloseAnimated(GameManager.Ins.OnInit);
    }
}
