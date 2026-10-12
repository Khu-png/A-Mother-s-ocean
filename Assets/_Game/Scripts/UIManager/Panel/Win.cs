using UnityEngine;

public class Win : UICanvas
{
    [SerializeField] private GameObject confettiPrefab;

    private GameObject confettiInstance;

    void OnDisable()
    {
        ClearConfetti();
    }

    public override void Open()
    {
        base.Open();
        ClearConfetti();
        if (confettiPrefab == null) return;

        if (confettiInstance == null) confettiInstance = Instantiate(confettiPrefab, transform, false);
        confettiInstance.SetActive(true);
    }

    private void ClearConfetti()
    {
        if (confettiInstance == null) return;

        confettiInstance.SetActive(false);
    }

    public void OnClickNext()
    {
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.OnNextLevel();
    }

    public void OnClickReplay()
    {
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.OnReplay();
    }
}
