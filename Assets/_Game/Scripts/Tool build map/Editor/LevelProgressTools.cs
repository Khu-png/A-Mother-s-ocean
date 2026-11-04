using UnityEditor;
using UnityEngine;

public static class LevelProgressTools
{
    private const string RESET_LEVEL_MENU = "Tools/A Mother's Ocean/PlayerPrefs/Reset Saved Level to 1";
    private const string COIN_CHEAT_MENU = "Tools/A Mother's Ocean/Cheats/Set Coins to 9000";
    private const int CHEAT_COINS = 9000;

    [MenuItem(COIN_CHEAT_MENU)]
    public static void SetCheatCoins()
    {
        PlayerPrefs.SetInt(MenuPlayerProgress.CoinsKey, CHEAT_COINS);
        PlayerPrefs.Save();
        Debug.Log("Coins set to 9000. Enter Play Mode to use them in the shop.");
    }

    [MenuItem(COIN_CHEAT_MENU, true)]
    private static bool CanSetCheatCoins()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;
    }

    [MenuItem(RESET_LEVEL_MENU)]
    public static void ResetSavedLevel()
    {
        PlayerPrefs.SetInt(LevelManager.CurrentLevelKey, 1);
        PlayerPrefs.Save();
        Debug.Log("Saved level reset to 1. Coins, skins and other PlayerPrefs were preserved.");
    }

    [MenuItem(RESET_LEVEL_MENU, true)]
    private static bool CanResetSavedLevel()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;
    }
}
