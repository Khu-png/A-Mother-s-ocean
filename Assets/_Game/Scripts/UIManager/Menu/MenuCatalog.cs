using System;
using UnityEngine;

public enum MenuPowerUpId
{
    Undo,
    Hint,
    Shuffle
}

[Serializable]
public sealed class MenuSkinOffer
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField, Min(0)] private int price;
    [SerializeField] private Sprite portrait;
    [SerializeField] private Color tint = Color.white;

    public string Id => id;
    public string DisplayName => displayName;
    public int Price => price;
    public Sprite Portrait => portrait;
    public Color Tint => tint;
}

[Serializable]
public sealed class MenuPowerUpOffer
{
    [SerializeField] private MenuPowerUpId id;
    [SerializeField, Min(1)] private int amount = 1;
    [SerializeField, Min(0)] private int price;

    public MenuPowerUpId Id => id;
    public int Amount => amount;
    public int Price => price;
}

[Serializable]
public sealed class MenuGiftReward
{
    [SerializeField] private MenuPowerUpId id;
    [SerializeField, Min(1)] private int amount = 1;

    public MenuPowerUpId Id => id;
    public int Amount => amount;
}

[CreateAssetMenu(fileName = "MenuCatalog", menuName = "A Mother's Ocean/Menu Catalog")]
public sealed class MenuCatalog : ScriptableObject
{
    [SerializeField] private bool purchasesEnabled;
    [SerializeField] private bool giftRewardsEnabled;
    [SerializeField] private MenuSkinOffer[] skins;
    [SerializeField] private MenuPowerUpOffer[] powerUps;
    [SerializeField] private MenuGiftReward[] giftRewards;

    public bool PurchasesEnabled => purchasesEnabled;
    public bool GiftRewardsEnabled => giftRewardsEnabled;
    public MenuSkinOffer[] Skins => skins;
    public MenuPowerUpOffer[] PowerUps => powerUps;
    public MenuGiftReward[] GiftRewards => giftRewards;
    public string DefaultSkinId => skins[0].Id;
    public MenuSkinOffer GetSkin(string skinId)
    {
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].Id == skinId) return skins[i];
        }
        throw new UnityException($"Skin '{skinId}' is absent from the catalog.");
    }
}
