using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Lose : UICanvas
{
    [SerializeField] private Button replayButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text replayLabel;
    [SerializeField] private MenuCatalog catalog;

    public override void Open()
    {
        base.Open();
        replayButton.interactable = true;
        bool hasLife = MenuPlayerProgress.Lives > 0;
        titleText.text = hasLife ? "OUT OF TIME" : "NO HEARTS LEFT";
        replayLabel.text = hasLife ? "REPLAY  -1"
            : CanBuyHeart ? $"BUY HEART  {catalog.HeartPrice}" : "MAIN MENU";
    }

    // With no hearts, replay sells one at the shop price when it is affordable.
    private bool CanBuyHeart => catalog.PurchasesEnabled
        && MenuPlayerProgress.Coins >= catalog.HeartPrice;

    public void OnClickReplay()
    {
        SoundManager.Ins.PlayButtonSound();
        if (MenuPlayerProgress.Lives == 0 && CanBuyHeart)
            MenuPlayerProgress.TryBuyLife(catalog.HeartPrice);
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
