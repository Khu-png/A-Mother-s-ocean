using UnityEngine;

public class Lose : UICanvas
{
    public void OnReplayButton()
    {
        LevelManager.Ins.OnReplay();
    }

    public void OnMainMenuButton()
    {
        GameManager.ChangeState(GameState.MainMenu);
    }
}
