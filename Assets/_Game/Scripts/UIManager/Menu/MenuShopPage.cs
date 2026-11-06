using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public sealed class MenuPowerUpOfferView
{
    [SerializeField] private TMP_Text quantity;
    [SerializeField] private TMP_Text price;
    [SerializeField] private TMP_Text buttonLabel;
    [SerializeField] private Button buyButton;

    public Button BuyButton => buyButton;

    public void Display(MenuPowerUpOffer offer, bool purchasesEnabled, int coins)
    {
        quantity.text = $"x{offer.Amount}";
        price.text = offer.Price.ToString();
        buyButton.interactable = purchasesEnabled && coins >= offer.Price;
        buttonLabel.text = purchasesEnabled ? "Buy" : "Unavailable";
    }

    public void DisplayLife(int lifePrice, bool purchasesEnabled, int coins, int lives)
    {
        quantity.text = "+1";
        price.text = lifePrice.ToString();
        bool livesFull = lives >= MenuPlayerProgress.MaxLives;
        buyButton.interactable = purchasesEnabled && !livesFull && coins >= lifePrice;
        buttonLabel.text = !purchasesEnabled ? "Unavailable" : livesFull ? "Full" : "Buy";
    }

    public void SetProcessing()
    {
        buyButton.interactable = false;
        buttonLabel.text = "Processing...";
    }
}

public sealed class MenuShopPage : MonoBehaviour
{
    [SerializeField] private Image featuredSkinPreview;
    [SerializeField] private TMP_Text featuredSkinName;
    [SerializeField] private TMP_Text featuredSkinPrice;
    [SerializeField] private TMP_Text featuredButtonLabel;
    [SerializeField] private Button featuredBuyButton;
    [SerializeField, Min(0)] private int featuredSkinIndex;
    [SerializeField] private MenuPowerUpOfferView[] powerUpViews;
    [SerializeField] private GameObject insufficientCoinsState;
    [SerializeField, Min(0f)] private float processingDuration = 0.25f;
    [SerializeField] private Button previousSkinButton;
    [SerializeField] private Button nextSkinButton;
    [SerializeField] private MenuSkinTransition skinTransition;
    private MenuCatalog catalog;
    private Action purchaseCompleted;
    private bool processing;

    public void Initialize(MenuCatalog menuCatalog, Action onPurchaseCompleted)
    {
        catalog = menuCatalog;
        purchaseCompleted = onPurchaseCompleted;
        featuredBuyButton.onClick.AddListener(BuyFeaturedSkin);
        for (int i = 0; i < catalog.PowerUps.Length; i++)
        {
            int offerIndex = i;
            powerUpViews[i].BuyButton.onClick.AddListener(() => BuyPowerUp(offerIndex));
        }
        powerUpViews[catalog.PowerUps.Length].BuyButton.onClick.AddListener(BuyLife);
        previousSkinButton.onClick.AddListener(SelectPreviousSkin);
        nextSkinButton.onClick.AddListener(SelectNextSkin);
        Refresh();
    }

    public void Refresh()
    {
        MenuSkinOffer skin = catalog.Skins[featuredSkinIndex];
        featuredSkinPreview.sprite = skin.Portrait;
        featuredSkinPreview.color = skin.Tint;
        featuredSkinName.text = skin.DisplayName;
        featuredSkinPrice.text = skin.Price.ToString();
        bool owned = MenuPlayerProgress.IsSkinOwned(skin.Id);
        featuredBuyButton.interactable = catalog.PurchasesEnabled && !owned && MenuPlayerProgress.Coins >= skin.Price;
        featuredButtonLabel.text = ButtonText(owned);
        for (int i = 0; i < catalog.PowerUps.Length; i++)
            powerUpViews[i].Display(catalog.PowerUps[i],
                catalog.PurchasesEnabled, MenuPlayerProgress.Coins);
        powerUpViews[catalog.PowerUps.Length].DisplayLife(catalog.HeartPrice,
            catalog.PurchasesEnabled, MenuPlayerProgress.Coins, MenuPlayerProgress.Lives);
        insufficientCoinsState.SetActive(catalog.PurchasesEnabled && !CanAffordAnything());
    }

    void OnDestroy()
    {
        featuredBuyButton.onClick.RemoveListener(BuyFeaturedSkin);
        powerUpViews[catalog.PowerUps.Length].BuyButton.onClick.RemoveListener(BuyLife);
        previousSkinButton.onClick.RemoveListener(SelectPreviousSkin);
        nextSkinButton.onClick.RemoveListener(SelectNextSkin);
    }

    private void SelectPreviousSkin()
    {
        SelectSkin((featuredSkinIndex - 1 + catalog.Skins.Length)
            % catalog.Skins.Length, -1);
    }

    private void SelectNextSkin()
    {
        SelectSkin((featuredSkinIndex + 1) % catalog.Skins.Length, 1);
    }

    private void SelectSkin(int index, int direction)
    {
        if (processing || skinTransition.IsPlaying) return;
        SoundManager.Ins.PlayButtonSound();
        skinTransition.Play(direction, () => ApplySelectedSkin(index));
    }

    private void BuyFeaturedSkin()
    {
        if (processing || skinTransition.IsPlaying) return;
        SoundManager.Ins.PlayButtonSound();
        StartCoroutine(ProcessSkinPurchase());
    }

    private void BuyPowerUp(int offerIndex)
    {
        if (processing) return;
        SoundManager.Ins.PlayButtonSound();
        StartCoroutine(ProcessPowerUpPurchase(offerIndex));
    }

    private void BuyLife()
    {
        if (processing) return;
        SoundManager.Ins.PlayButtonSound();
        StartCoroutine(ProcessLifePurchase());
    }

    private IEnumerator ProcessSkinPurchase()
    {
        BeginProcessing(featuredButtonLabel, featuredBuyButton);
        yield return new WaitForSecondsRealtime(processingDuration);
        MenuPlayerProgress.TryBuySkin(catalog.Skins[featuredSkinIndex]);
        EndProcessing();
    }

    private IEnumerator ProcessPowerUpPurchase(int offerIndex)
    {
        MenuPowerUpOfferView view = powerUpViews[offerIndex];
        processing = true;
        view.SetProcessing();
        yield return new WaitForSecondsRealtime(processingDuration);
        MenuPlayerProgress.TryBuyPowerUp(catalog.PowerUps[offerIndex]);
        EndProcessing();
    }

    private IEnumerator ProcessLifePurchase()
    {
        MenuPowerUpOfferView lifeView = powerUpViews[catalog.PowerUps.Length];
        processing = true;
        lifeView.SetProcessing();
        yield return new WaitForSecondsRealtime(processingDuration);
        MenuPlayerProgress.TryBuyLife(catalog.HeartPrice);
        EndProcessing();
    }

    private void BeginProcessing(TMP_Text label, Button button)
    {
        processing = true;
        button.interactable = false;
        label.text = "Processing...";
    }

    private void EndProcessing()
    {
        processing = false;
        purchaseCompleted();
        Refresh();
    }

    private void ApplySelectedSkin(int index)
    {
        featuredSkinIndex = index;
        Refresh();
    }

    private bool CanAffordAnything()
    {
        for (int i = 0; i < catalog.PowerUps.Length; i++)
        {
            if (MenuPlayerProgress.Coins >= catalog.PowerUps[i].Price) return true;
        }
        if (MenuPlayerProgress.Lives < MenuPlayerProgress.MaxLives
            && MenuPlayerProgress.Coins >= catalog.HeartPrice) return true;
        return MenuPlayerProgress.Coins >= catalog.Skins[featuredSkinIndex].Price;
    }

    private string ButtonText(bool owned)
    {
        if (owned) return "Owned";
        return catalog.PurchasesEnabled ? "Buy" : "Unavailable";
    }
}
