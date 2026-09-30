using UnityEngine;
using TMPro;

public class Gameplay : UICanvas
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private SpriteRenderer oceanBackground;

    private void LateUpdate()
    {
        if (oceanBackground != null && LevelManager.Ins != null)
            LevelManager.Ins.FitGameplayBackground(oceanBackground);
    }

    private void OnEnable()
    {
        if (levelText != null)
            levelText.text = $"Level {LevelManager.Ins.CurrentLevelNumber:00}";
    }

    public void OnClickPause()
    {
        GameManager.Ins.OnPause();
    }

    public override void BackKey()
    {
        OnClickPause();
    }

    public void OnClickRestart()
    {
        LevelManager.Ins.OnRestart();
    }
}
