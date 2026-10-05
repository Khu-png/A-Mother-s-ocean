using UnityEngine;

public abstract class OptionsPanel : UICanvas
{
    [SerializeField] private GameObject musicOnIcon;
    [SerializeField] private GameObject musicOffIcon;
    [SerializeField] private GameObject soundOnIcon;
    [SerializeField] private GameObject soundOffIcon;
    [SerializeField] private GameObject vibrationOnIcon;
    [SerializeField] private GameObject vibrationOffIcon;

    protected virtual void OnEnable()
    {
        RefreshIcons();
    }

    public void OnClickMusic()
    {
        SoundManager.Ins.ToggleMusic();
        SoundManager.Ins.PlayButtonSound();
        RefreshIcons();
    }

    public void OnClickSound()
    {
        SoundManager.Ins.ToggleSound();
        RefreshIcons();
    }

    public void OnClickVibration()
    {
        VibrationSettings.Toggle();
        SoundManager.Ins.PlayButtonSound();
        RefreshIcons();
    }

    public void OnClickTutorial()
    {
        SoundManager.Ins.PlayButtonSound();
    }

    private void RefreshIcons()
    {
        SetIcons(musicOnIcon, musicOffIcon, SoundManager.Ins.IsMusicEnabled);
        SetIcons(soundOnIcon, soundOffIcon, SoundManager.Ins.IsSoundEnabled);
        SetIcons(vibrationOnIcon, vibrationOffIcon, VibrationSettings.IsEnabled);
    }

    private static void SetIcons(GameObject onIcon, GameObject offIcon, bool enabled)
    {
        if (onIcon != null) onIcon.SetActive(enabled);
        if (offIcon != null) offIcon.SetActive(!enabled);
    }
}
