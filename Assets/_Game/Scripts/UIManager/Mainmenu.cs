using UnityEngine;

public class Mainmenu : UICanvas
{
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
