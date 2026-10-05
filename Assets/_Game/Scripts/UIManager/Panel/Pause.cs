using UnityEngine;

public class Pause : OptionsPanel
{
    public override void BackKey()
    {
        OnClickResume();
    }

    public void OnClickResume()
    {
        SoundManager.Ins.PlayButtonSound();
        CloseAnimated(GameManager.Ins.OnResume);
    }

    public void OnClickReplay()
    {
        SoundManager.Ins.PlayButtonSound();
        CloseAnimated(LevelManager.Ins.OnReplay);
    }

    public void OnClickMainMenu()
    {
        SoundManager.Ins.PlayButtonSound();
        CloseAnimated(GameManager.Ins.OnInit);
    }
}
