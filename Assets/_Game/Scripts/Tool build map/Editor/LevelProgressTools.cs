using UnityEditor;
using UnityEngine;

public static class LevelProgressTools
{
    private const string RESET_LEVEL_MENU = "Tools/A Mother's Ocean/PlayerPrefs/Reset Saved Level to 1";

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
