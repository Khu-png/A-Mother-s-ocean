using UnityEngine;

public class Settings : OptionsPanel
{
    public void OnClickClose()
    {
        CloseAnimated();
    }

    public void OnClickExit()
    {
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
