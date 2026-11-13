using UnityEditor;

// Editor shortcut for testing the Shop from scratch: only the default skin stays owned
// (and equipped), every power-up count goes back to zero and the "new skin" dot is cleared.
// Coins, hearts and level progress are left untouched.
public static class InventoryResetMenu
{
    private const string CATALOG_PATH = "Assets/_Game/Data/MenuCatalog.asset";

    [MenuItem("Tools/A Mother's Ocean/Cheats/Reset Skins And Power-Ups")]
    private static void ResetInventory()
    {
        bool confirmed = EditorUtility.DisplayDialog("Reset skins and power-ups",
            "Remove every bought skin (only the default skin stays) and set all power-ups to 0?\n"
            + "Coins, hearts and level progress are kept.", "Reset", "Cancel");
        if (!confirmed) return;
        MenuCatalog catalog = AssetDatabase.LoadAssetAtPath<MenuCatalog>(CATALOG_PATH);
        MenuPlayerProgress.ResetInventory(catalog);
        EditorUtility.DisplayDialog("Reset skins and power-ups",
            "Done. If the game is running, reopen the menu to see the change.", "OK");
    }
}
