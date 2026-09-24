using UnityEngine;
using UnityEngine.UI;

public class Gameplay : UICanvas
{
    [SerializeField] private Text levelText;

    private void OnEnable()
    {
        if (levelText != null)
            levelText.text = "<size=55%>LEVEL</size>\n" + LevelManager.SavedLevelNumber;
    }

    public void OnClickPause()
    {
        GameManager.Ins.OnPause();
    }

    public void OnClickRestart()
    {
        LevelManager.Ins.OnRestart();
    }
}
