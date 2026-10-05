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
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.OnPlay();
    }

    public void OnClickResetData()
    {
        SoundManager.Ins.PlayButtonSound();
        PlayerPrefs.DeleteKey(LevelManager.CurrentLevelKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.Save();
        RefreshCoins();
        RefreshLevel();
    }

    public void OnClickSettings()
    {
        SoundManager.Ins.PlayButtonSound();
        UIManager.Ins.OpenUI<Settings>();
    }

    public void OnClickGift()
    {
        SoundManager.Ins.PlayButtonSound();
        Debug.Log("Main menu gift button");
    }

    public void OnClickShop()
    {
        SoundManager.Ins.PlayButtonSound();
        Debug.Log("Main menu shop button");
    }

    public void OnClickHome()
    {
        SoundManager.Ins.PlayButtonSound();
        Debug.Log("Main menu home button");
    }

    public void OnClickSkin()
    {
        SoundManager.Ins.PlayButtonSound();
        Debug.Log("Main menu skin button");
    }

    public void OnClickTank()
    {
        SoundManager.Ins.PlayButtonSound();
        Debug.Log("Main menu tank button");
    }
}
