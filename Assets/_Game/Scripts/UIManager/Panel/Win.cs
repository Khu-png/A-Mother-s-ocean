using UnityEngine;

public class Win : UICanvas
{
    [SerializeField] private GameObject confettiPrefab;

    private GameObject confettiInstance;

    public override void Open()
    {
        base.Open();
        ClearConfetti();
        if (confettiPrefab == null) return;

        confettiInstance = Instantiate(confettiPrefab, transform, false);
        confettiInstance.SetActive(true);
    }

    private void OnDisable()
    {
        ClearConfetti();
    }

    private void ClearConfetti()
    {
        if (confettiInstance == null) return;

        confettiInstance.SetActive(false);
        Destroy(confettiInstance);
        confettiInstance = null;
    }

    public void OnClickNext()
    {
        LevelManager.Ins.OnNextLevel();
    }

    public void OnClickReplay()
    {
        LevelManager.Ins.OnReplay();
    }
}
