using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Lose : UICanvas
{
    [SerializeField] private Button replayButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text replayLabel;

    public override void Open()
    {
        base.Open();
        replayButton.interactable = true;
        titleText.text = MenuPlayerProgress.Lives > 0
            ? "OUT OF TIME"
            : "NO HEARTS LEFT";
        replayLabel.text = MenuPlayerProgress.Lives > 0
            ? "REPLAY  -1"
            : "MAIN MENU";
    }

    public void OnClickReplay()
    {
        SoundManager.Ins.PlayButtonSound();
        if (MenuPlayerProgress.Lives == 0)
        {
            GameManager.Ins.OnInit();
            return;
        }
        if (!MenuPlayerProgress.TrySpendLife()) return;
        LevelManager.Ins.OnReplay();
    }

    public void OnClickMainMenu()
    {
        SoundManager.Ins.PlayButtonSound();
        GameManager.Ins.OnInit();
    }
}
