using UnityEngine;

public class Settings : OptionsPanel
{
    public void OnClickClose()
    {
        SoundManager.Ins.PlayButtonSound();
        CloseAnimated();
    }

    public void OnClickExit()
    {
        SoundManager.Ins.PlayButtonSound();
        CloseAnimated(ExitGame);
    }

    private void ExitGame()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void BackKey()
    {
        OnClickClose();
    }

}
