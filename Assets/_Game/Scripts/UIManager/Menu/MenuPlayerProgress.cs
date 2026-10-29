using UnityEngine;

public static class MenuPlayerProgress
{
    public const string CoinsKey = "Coins";
    private const string CHESTS_KEY = "GiftChests";
    private const string EQUIPPED_SKIN_KEY = "EquippedSkin";
    private const string OWNED_SKIN_PREFIX = "OwnedSkin_";
    private const string POWER_UP_PREFIX = "PowerUp_";
    private const string CLAIMED_CHEST_PREFIX = "ClaimedGiftLevel_";

    public static int Coins => Mathf.Max(0, PlayerPrefs.GetInt(CoinsKey, 0));
    public static int Chests => Mathf.Max(0, PlayerPrefs.GetInt(CHESTS_KEY, 0));
    public static string EquippedSkin => PlayerPrefs.GetString(EQUIPPED_SKIN_KEY);

    public static void Initialize(string defaultSkinId)
    {
        if (!PlayerPrefs.HasKey(OwnedSkinKey(defaultSkinId)))
            PlayerPrefs.SetInt(OwnedSkinKey(defaultSkinId), 1);
        if (!PlayerPrefs.HasKey(EQUIPPED_SKIN_KEY))
            PlayerPrefs.SetString(EQUIPPED_SKIN_KEY, defaultSkinId);
        PlayerPrefs.Save();
    }

    public static bool IsSkinOwned(string skinId)
    {
        return PlayerPrefs.GetInt(OwnedSkinKey(skinId), 0) == 1;
    }

    public static bool TryBuySkin(MenuSkinOffer offer)
    {
        if (IsSkinOwned(offer.Id) || Coins < offer.Price) return false;
        SpendCoins(offer.Price);
        PlayerPrefs.SetInt(OwnedSkinKey(offer.Id), 1);
        PlayerPrefs.Save();
        return true;
    }

    public static bool TryBuyPowerUp(MenuPowerUpOffer offer)
    {
        if (Coins < offer.Price) return false;
        SpendCoins(offer.Price);
        AddPowerUp(offer.Id, offer.Amount);
        PlayerPrefs.Save();
        return true;
    }

    public static int PowerUpCount(MenuPowerUpId id)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(POWER_UP_PREFIX + id, 0));
    }

    public static bool TryUsePowerUp(MenuPowerUpId id)
    {
        int count = PowerUpCount(id);
        if (count == 0) return false;
        PlayerPrefs.SetInt(POWER_UP_PREFIX + id, count - 1);
        PlayerPrefs.Save();
        return true;
    }

    public static void EquipSkin(string skinId)
    {
        if (!IsSkinOwned(skinId)) throw new UnityException("Cannot equip an unowned skin.");
        PlayerPrefs.SetString(EQUIPPED_SKIN_KEY, skinId);
        PlayerPrefs.Save();
    }

    public static void ClaimGift(MenuGiftReward[] rewards)
    {
        if (Chests <= 0) throw new UnityException("No gift chest is available.");
        PlayerPrefs.SetInt(CHESTS_KEY, Chests - 1);
        for (int i = 0; i < rewards.Length; i++) AddPowerUp(rewards[i].Id, rewards[i].Amount);
        PlayerPrefs.Save();
    }

    public static void GrantChestForLevel(int levelNumber)
    {
        string claimKey = CLAIMED_CHEST_PREFIX + levelNumber;
        if (PlayerPrefs.GetInt(claimKey, 0) == 1) return;
        PlayerPrefs.SetInt(claimKey, 1);
        PlayerPrefs.SetInt(CHESTS_KEY, Chests + 1);
        PlayerPrefs.Save();
    }

    public static void GrantCompletionCoins(int amount)
    {
        PlayerPrefs.SetInt(CoinsKey, Coins + amount);
        PlayerPrefs.Save();
    }

    private static void SpendCoins(int amount)
    {
        PlayerPrefs.SetInt(CoinsKey, Coins - amount);
    }

    private static void AddPowerUp(MenuPowerUpId id, int amount)
    {
        string key = POWER_UP_PREFIX + id;
        PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + amount);
    }

    private static string OwnedSkinKey(string skinId) => OWNED_SKIN_PREFIX + skinId;
}
