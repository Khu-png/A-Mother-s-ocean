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

    public void OnClickPlay()
    {
        LevelManager.Ins.OnReplay();
    }

    public void OnClickRestart()
    {
        LevelManager.Ins.OnRestart();
    }

    public void OnClickSettings()
    {
        Debug.Log("Main menu settings button");
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
