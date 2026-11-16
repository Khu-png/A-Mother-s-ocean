using System;
using TMPro;
using UnityEngine;

public sealed class GameplayLifeWarning : MonoBehaviour
{
    private enum WarningAction { Restart, MainMenu, BuyPowerUp }

    [SerializeField] private RestartWarningMotion motion;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text confirmLabel;
    [SerializeField] private MenuCatalog catalog;
    [SerializeField] private GameObject coinPanel;
    [SerializeField] private TMP_Text coinText;
    private WarningAction pendingAction;
    private MenuPowerUpOffer pendingOffer;
    private Action powerUpPurchased;

    public bool IsVisible => motion.IsVisible;

    public void ShowRestart()
    {
        pendingAction = WarningAction.Restart;
        Show("RESTART LEVEL?");
    }

    public void ShowMainMenu()
    {
        pendingAction = WarningAction.MainMenu;
        Show("RETURN TO MENU?");
    }

    // Offers the shop's price for one pack; false when the shop is closed.
    public bool TryShowPowerUpPurchase(MenuPowerUpId id, Action onPurchased)
    {
        if (!catalog.PurchasesEnabled) return false;
        pendingAction = WarningAction.BuyPowerUp;
        pendingOffer = catalog.GetPowerUp(id);
        powerUpPurchased = onPurchased;
        bool canAfford = MenuPlayerProgress.Coins >= pendingOffer.Price;
        title.text = $"BUY {pendingOffer.Id.ToString().ToUpperInvariant()} x{pendingOffer.Amount}?";
        message.text = canAfford ? $"COST {pendingOffer.Price} COINS" : "NOT ENOUGH COINS";
        confirmLabel.text = canAfford ? "BUY" : "OK";
        ShowPopup(true);
        return true;
    }

    public void Confirm()
    {
        HidePopup();
        if (pendingAction == WarningAction.BuyPowerUp)
        {
            CompletePowerUpPurchase();
            return;
        }
        if (OffersHeart) MenuPlayerProgress.TryBuyLife(catalog.HeartPrice);
        if (MenuPlayerProgress.Lives == 0 || !MenuPlayerProgress.TrySpendLife())
        {
            GameManager.Ins.OnInit();
            return;
        }

        if (pendingAction == WarningAction.Restart)
            LevelManager.Ins.OnRestart();
        else
            GameManager.Ins.OnInit();
    }

    public void Cancel()
    {
        HidePopup();
        if (pendingAction == WarningAction.MainMenu)
            UIManager.Ins.OpenUI<Pause>();
        else
            GameManager.ChangeState(GameState.Gameplay);
    }

    // Restarting with no hearts left sells one heart at the shop price first.
    private bool OffersHeart => pendingAction == WarningAction.Restart
        && MenuPlayerProgress.Lives == 0 && catalog.PurchasesEnabled
        && MenuPlayerProgress.Coins >= catalog.HeartPrice;

    private void CompletePowerUpPurchase()
    {
        bool purchased = MenuPlayerProgress.TryBuyPowerUp(pendingOffer);
        GameManager.ChangeState(GameState.Gameplay);
        if (purchased) powerUpPurchased();
    }

    private void Show(string warningTitle)
    {
        bool hasLife = MenuPlayerProgress.Lives > 0;
        title.text = warningTitle;
        message.text = hasLife ? "THIS WILL COST 1 HEART"
            : OffersHeart ? $"BUY 1 HEART FOR {catalog.HeartPrice} COINS" : "NO HEARTS LEFT";
        confirmLabel.text = pendingAction == WarningAction.Restart && (hasLife || OffersHeart)
            ? (hasLife ? "RESTART" : "BUY HEART")
            : "MAIN MENU";
        ShowPopup(pendingAction == WarningAction.Restart && !hasLife && catalog.PurchasesEnabled);
    }

    // The wallet sits above the popup whenever the player is being asked to spend coins.
    private void ShowPopup(bool showCoins)
    {
        coinPanel.SetActive(showCoins);
        coinText.text = MenuPlayerProgress.Coins.ToString();
        motion.Show();
    }

    private void HidePopup()
    {
        coinPanel.SetActive(false);
        motion.Hide();
    }
}
