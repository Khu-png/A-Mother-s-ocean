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
    [SerializeField] private Image icon;

    public Button BuyButton => buyButton;
    public Sprite IconSprite => icon.sprite;

    public void Display(MenuPowerUpOffer offer, bool purchasesEnabled)
    {
        quantity.text = $"x{offer.Amount}";
        price.text = offer.Price.ToString();
        // Stays tappable without enough coins so the tap can explain the shortage.
        buyButton.interactable = purchasesEnabled;
        buttonLabel.text = purchasesEnabled ? "Buy" : "Unavailable";
    }

    public void DisplayLife(int lifePrice, bool purchasesEnabled, int lives)
    {
        quantity.text = "+1";
        price.text = lifePrice.ToString();
        bool livesFull = lives >= MenuPlayerProgress.MaxLives;
        buyButton.interactable = purchasesEnabled && !livesFull;
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
    [SerializeField] private FeaturedSkinButtonView featuredButton;
    [SerializeField, Min(0)] private int featuredSkinIndex;
    [SerializeField] private MenuPowerUpOfferView[] powerUpViews;
    [SerializeField] private GameObject insufficientCoinsState;
    [SerializeField, Min(0f)] private float processingDuration = 0.25f;
    [SerializeField] private Button previousSkinButton;
    [SerializeField] private Button nextSkinButton;
    [SerializeField] private MenuSkinTransition skinTransition;
    [SerializeField] private CoinShortageFeedback coinShortage;
    [Header("Purchase Reward")]
    [SerializeField] private PurchaseRewardFlight rewardFlight;
    [SerializeField] private Image heartBar;
    [SerializeField] private Image skinTabIcon;
    [SerializeField] private MenuNewSkinBadge newSkinBadge;
    // Centre of the heart badge inside the heart bar art (0-1 of the sprite).
    [SerializeField] private Vector2 heartLandingPoint = new Vector2(0.236f, 0.531f);
    private MenuCatalog catalog;
    private Action purchaseCompleted;
    private bool processing;

    public void Initialize(MenuCatalog menuCatalog, Action onPurchaseCompleted)
    {
        catalog = menuCatalog;
        purchaseCompleted = onPurchaseCompleted;
        featuredButton.Button.onClick.AddListener(OnFeaturedButton);
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
        featuredButton.Display(skin, catalog.PurchasesEnabled);
        for (int i = 0; i < catalog.PowerUps.Length; i++)
            powerUpViews[i].Display(catalog.PowerUps[i], catalog.PurchasesEnabled);
        powerUpViews[catalog.PowerUps.Length].DisplayLife(catalog.HeartPrice,
            catalog.PurchasesEnabled, MenuPlayerProgress.Lives);
        insufficientCoinsState.SetActive(catalog.PurchasesEnabled && !CanAffordAnything());
    }

    void OnDestroy()
    {
        featuredButton.Button.onClick.RemoveListener(OnFeaturedButton);
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

    // The button is only tappable while the featured skin is not owned yet.
    private void OnFeaturedButton()
    {
        if (processing || skinTransition.IsPlaying) return;
        if (!HasCoins(catalog.Skins[featuredSkinIndex].Price, featuredButton.Button)) return;
        SoundManager.Ins.PlayButtonSound();
        StartCoroutine(ProcessSkinPurchase());
    }

    private void BuyPowerUp(int offerIndex)
    {
        if (processing) return;
        if (!HasCoins(catalog.PowerUps[offerIndex].Price, powerUpViews[offerIndex].BuyButton)) return;
        SoundManager.Ins.PlayButtonSound();
        StartCoroutine(ProcessPowerUpPurchase(offerIndex));
    }

    private void BuyLife()
    {
        if (processing) return;
        if (!HasCoins(catalog.HeartPrice, powerUpViews[catalog.PowerUps.Length].BuyButton)) return;
        SoundManager.Ins.PlayButtonSound();
        StartCoroutine(ProcessLifePurchase());
    }

    private IEnumerator ProcessSkinPurchase()
    {
        processing = true;
        featuredButton.SetProcessing();
        yield return new WaitForSecondsRealtime(processingDuration);
        MenuSkinOffer skin = catalog.Skins[featuredSkinIndex];
        // The new skin flies into the Inv tab; its red dot appears when the skin lands.
        if (MenuPlayerProgress.TryBuySkin(skin))
            rewardFlight.Play(skin.Portrait, "NEW!", skinTabIcon, new Vector2(0.5f, 0.5f), newSkinBadge.Refresh);
        EndProcessing();
    }

    private IEnumerator ProcessPowerUpPurchase(int offerIndex)
    {
        MenuPowerUpOfferView view = powerUpViews[offerIndex];
        processing = true;
        view.SetProcessing();
        yield return new WaitForSecondsRealtime(processingDuration);
        MenuPowerUpOffer offer = catalog.PowerUps[offerIndex];
        if (MenuPlayerProgress.TryBuyPowerUp(offer))
            rewardFlight.PlayInPlace(view.IconSprite, $"x{offer.Amount}");
        EndProcessing();
    }

    private IEnumerator ProcessLifePurchase()
    {
        MenuPowerUpOfferView lifeView = powerUpViews[catalog.PowerUps.Length];
        processing = true;
        lifeView.SetProcessing();
        yield return new WaitForSecondsRealtime(processingDuration);
        // The heart bar updates when the flying heart lands on it.
        if (MenuPlayerProgress.TryBuyLife(catalog.HeartPrice))
            rewardFlight.Play(lifeView.IconSprite, "+1", heartBar, heartLandingPoint, EndProcessing);
        else EndProcessing();
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

    private bool HasCoins(int price, Button buyButton)
    {
        if (MenuPlayerProgress.Coins >= price) return true;
        coinShortage.Play((RectTransform)buyButton.transform);
        return false;
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
}
