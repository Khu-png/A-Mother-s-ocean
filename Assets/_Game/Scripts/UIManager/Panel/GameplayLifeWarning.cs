using TMPro;
using UnityEngine;

public sealed class GameplayLifeWarning : MonoBehaviour
{
    private enum WarningAction { Restart, MainMenu }

    [SerializeField] private RestartWarningMotion motion;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text confirmLabel;
    private WarningAction pendingAction;

    public bool IsVisible => motion.IsVisible;

    public void ShowRestart()
    {
        pendingAction = WarningAction.Restart;
        Show("RESTART LEVEL?");
    }

    public void ShowMainMenu()
    {
        pendingAction = WarningAction.MainMenu;
        Show("RETURN TO MENU?");
    }

    public void Confirm()
    {
        motion.Hide();
        if (MenuPlayerProgress.Lives == 0 || !MenuPlayerProgress.TrySpendLife())
        {
            GameManager.Ins.OnInit();
            return;
        }

        if (pendingAction == WarningAction.Restart)
            LevelManager.Ins.OnRestart();
        else
            GameManager.Ins.OnInit();
    }

    public void Cancel()
    {
        motion.Hide();
        if (pendingAction == WarningAction.Restart)
            GameManager.ChangeState(GameState.Gameplay);
        else
            UIManager.Ins.OpenUI<Pause>();
    }

    private void Show(string warningTitle)
    {
        bool hasLife = MenuPlayerProgress.Lives > 0;
        title.text = warningTitle;
        message.text = hasLife ? "THIS WILL COST 1 HEART" : "NO HEARTS LEFT";
        confirmLabel.text = pendingAction == WarningAction.Restart && hasLife
            ? "RESTART"
            : "MAIN MENU";
        motion.Show();
    }
}
