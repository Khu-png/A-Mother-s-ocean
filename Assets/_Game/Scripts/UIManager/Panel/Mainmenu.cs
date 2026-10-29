using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public sealed class Mainmenu : UICanvas
{
    private enum BackgroundMode
    {
        StaticImage,
        Video
    }

    [Header("Menu")]
    [SerializeField] private MenuCatalog catalog;
    [SerializeField] private MenuNavigationController navigation;
    [SerializeField] private MenuGiftPage giftPage;
    [SerializeField] private MenuShopPage shopPage;
    [SerializeField] private MenuSkinPage skinPage;
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text coinAmountText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;

    [Header("Optional Video Background")]
    [SerializeField] private BackgroundMode backgroundMode;
    [SerializeField] private VideoPlayer backgroundVideo;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private Texture backgroundFallback;
    private bool initialized;

    public VideoPlayer BackgroundVideo => backgroundMode == BackgroundMode.Video
        ? backgroundVideo
        : null;
    public MenuTabId CurrentTab => navigation.CurrentTab;

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    void OnEnable()
    {
        if (!initialized) Initialize();
    }

    void OnDestroy()
    {
        playButton.onClick.RemoveListener(OnClickPlay);
        settingsButton.onClick.RemoveListener(OnClickSettings);
    }

    public void ShowBackgroundFallback()
    {
        if (backgroundMode == BackgroundMode.Video) videoImage.texture = backgroundFallback;
    }

    public void ShowVideoBackground()
    {
        if (backgroundMode == BackgroundMode.Video) videoImage.texture = backgroundVideo.targetTexture;
    }

    public void RefreshCoins()
    {
        coinAmountText.text = MenuPlayerProgress.Coins.ToString();
    }

    public void OnClickPlay()
    {
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.OnPlay();
    }

    public void OnClickResetData()
    {
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.ResetProgress();
        Refresh();
    }

    public void OnClickSettings()
    {
        SoundManager.Ins.PlayButtonSound();
        UIManager.Ins.OpenUI<Settings>();
    }

    public void OnClickGift() => SelectTab(MenuTabId.Gift);
    public void OnClickShop() => SelectTab(MenuTabId.Shop);
    public void OnClickHome() => SelectTab(MenuTabId.Home);
    public void OnClickSkin() => SelectTab(MenuTabId.Skin);
    public void OnClickTank() => SelectTab(MenuTabId.Tank);

    private void Initialize()
    {
        initialized = true;
        MenuPlayerProgress.Initialize(catalog.DefaultSkinId);
        navigation.Initialize();
        giftPage.Initialize(catalog);
        shopPage.Initialize(catalog, RefreshAfterPurchase);
        skinPage.Initialize(catalog);
        playButton.onClick.AddListener(OnClickPlay);
        settingsButton.onClick.AddListener(OnClickSettings);
    }

    private void Refresh()
    {
        currentLevelText.text = $"LEVEL\n{LevelManager.SavedLevelNumber}";
        RefreshCoins();
        giftPage.Refresh();
        shopPage.Refresh();
        skinPage.Refresh();
    }

    private void SelectTab(MenuTabId tab)
    {
        SoundManager.Ins.PlayButtonSound();
        navigation.Select(tab);
    }

    private void RefreshAfterPurchase()
    {
        RefreshCoins();
        skinPage.Refresh();
    }
}
