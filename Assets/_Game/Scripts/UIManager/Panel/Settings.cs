using UnityEngine;

public class Settings : UICanvas
{
    [SerializeField] private GameObject musicOnIcon;
    [SerializeField] private GameObject musicOffIcon;
    [SerializeField] private GameObject soundOnIcon;
    [SerializeField] private GameObject soundOffIcon;
    [SerializeField] private GameObject vibrationOnIcon;
    [SerializeField] private GameObject vibrationOffIcon;

    private bool isVibrationEnabled = true;

    private void OnEnable()
    {
        RefreshIcons();
    }

    public void OnClickMusic()
    {
        SoundManager.Ins.ToggleMusic();
        RefreshIcons();
    }

    public void OnClickSound()
    {
        SoundManager.Ins.ToggleSound();
        RefreshIcons();
    }

    public void OnClickVibration()
    {
        isVibrationEnabled = !isVibrationEnabled;
        RefreshIcons();
    }

    public void OnClickClose()
    {
        UIManager.Ins.CloseUI<Settings>();
    }

    public override void BackKey()
    {
        OnClickClose();
    }

    private void RefreshIcons()
    {
        SetIconState(musicOnIcon, musicOffIcon, SoundManager.Ins.IsMusicEnabled);
        SetIconState(soundOnIcon, soundOffIcon, SoundManager.Ins.IsSoundEnabled);
        SetIconState(vibrationOnIcon, vibrationOffIcon, isVibrationEnabled);
    }

    private void SetIconState(GameObject onIcon, GameObject offIcon, bool isOn)
    {
        if (offIcon == null)
        {
            if (onIcon != null) onIcon.SetActive(true);
            return;
        }

        if (onIcon != null) onIcon.SetActive(isOn);
        offIcon.SetActive(!isOn);
    }
}
