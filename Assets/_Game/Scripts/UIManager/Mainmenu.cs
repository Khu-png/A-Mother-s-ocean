using UnityEngine;
using TMPro;

public class Mainmenu : UICanvas
{
    public const string CoinsKey = "Coins";
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text coinAmountText;

    private void OnEnable()
    {
        if (currentLevelText != null)
            currentLevelText.text = "<size=55%>LEVEL</size>\n" + LevelManager.SavedLevelNumber;
        RefreshCoins();
    }

    public void RefreshCoins()
    {
        if (coinAmountText != null)
            coinAmountText.text = Mathf.Max(0, PlayerPrefs.GetInt(CoinsKey, 0)).ToString();
    }

    public void OnPlayButton()
    {
        LevelManager.Ins.OnReplay();
    }

    public void OnSettingsButton()
    {
        Debug.Log("Main menu settings button");
    }

    public void OnGiftButton()
    {
        Debug.Log("Main menu gift button");
    }

    public void OnShopButton()
    {
        Debug.Log("Main menu shop button");
    }

    public void OnHomeButton()
    {
        Debug.Log("Main menu home button");
    }

    public void OnSkinButton()
    {
        Debug.Log("Main menu skin button");
    }

    public void OnTankButton()
    {
        Debug.Log("Main menu tank button");
    }
}
