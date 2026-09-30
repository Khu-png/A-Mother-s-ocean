using UnityEngine;
using TMPro;

public class Mainmenu : UICanvas
{
    public const string CoinsKey = "Coins";
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text coinAmountText;
    private string levelTextTemplate;

    private void OnEnable()
    {
        RefreshLevel();
        RefreshCoins();
    }

    private void RefreshLevel()
    {
        if (currentLevelText == null) return;

        // Keep the prefab's rich-text styling; replace only the level placeholder.
        if (levelTextTemplate == null) levelTextTemplate = currentLevelText.text;
        currentLevelText.text = levelTextTemplate.Replace("{level}", LevelManager.SavedLevelNumber.ToString());
    }

    public void RefreshCoins()
    {
        if (coinAmountText != null)
            coinAmountText.text = Mathf.Max(0, PlayerPrefs.GetInt(CoinsKey, 0)).ToString();
    }

    public void OnClickPlay()
    {
        LevelManager.Ins.OnReplay();
    }

    public void OnClickResetData()
    {
        PlayerPrefs.DeleteKey(LevelManager.CurrentLevelKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.Save();
        RefreshCoins();
        RefreshLevel();
    }

    public void OnClickSettings()
    {
        UIManager.Ins.OpenUI<Settings>();
    }

    public void OnClickGift()
    {
        Debug.Log("Main menu gift button");
    }

    public void OnClickShop()
    {
        Debug.Log("Main menu shop button");
    }

    public void OnClickHome()
    {
        Debug.Log("Main menu home button");
    }

    public void OnClickSkin()
    {
        Debug.Log("Main menu skin button");
    }

    public void OnClickTank()
    {
        Debug.Log("Main menu tank button");
    }
}
