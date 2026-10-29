using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Win : UICanvas
{
    [SerializeField] private Animator sequenceAnimator;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text claimText;
    [SerializeField] private TMP_Text walletText;
    private static readonly int INTRO_STATE = Animator.StringToHash("Intro");
    private static readonly int CLAIM_STATE = Animator.StringToHash("Claim");
    private bool readyToClaim;
    private bool claimed;
    private Coroutine sequence;

    void OnDisable()
    {
        if (sequence != null) StopCoroutine(sequence);
        sequence = null;
        readyToClaim = false;
    }

    public override void Open()
    {
        base.Open();
        if (sequence != null) StopCoroutine(sequence);
        claimed = false;
        readyToClaim = false;
        claimButton.interactable = false;
        int reward = LevelManager.Ins.PendingWinCoins;
        rewardText.text = $"+{reward}";
        claimText.text = reward > 0 ? $"CLAIM {reward}" : "CONTINUE";
        walletText.text = MenuPlayerProgress.Coins.ToString();
        sequenceAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        sequence = StartCoroutine(ShowReward());
    }

    private IEnumerator ShowReward()
    {
        sequenceAnimator.Play(INTRO_STATE, 0, 0f);
        sequenceAnimator.Update(0f);
        do { yield return null; }
        while (sequenceAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
        readyToClaim = true;
        claimButton.interactable = true;
        sequence = null;
    }

    public void OnClickNext()
    {
        if (!readyToClaim || claimed) return;
        claimed = true;
        claimButton.interactable = false;
        SoundManager.Ins.PlayButtonSound();
        int startingCoins = MenuPlayerProgress.Coins;
        LevelManager.Ins.ClaimWinReward();
        claimText.text = "CLAIMED!";
        sequence = StartCoroutine(ShowClaim(startingCoins, MenuPlayerProgress.Coins));
    }

    private IEnumerator ShowClaim(int startingCoins, int finalCoins)
    {
        sequenceAnimator.Play(CLAIM_STATE, 0, 0f);
        sequenceAnimator.Update(0f);
        do
        {
            float progress = sequenceAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            float coinProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.7f));
            walletText.text = Mathf.RoundToInt(Mathf.Lerp(startingCoins, finalCoins, coinProgress)).ToString();
            yield return null;
        }
        while (sequenceAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
        walletText.text = finalCoins.ToString();
        sequence = null;
        LevelManager.Ins.ContinueAfterWin();
    }

    public void OnClickReplay()
    {
        if (claimed) return;
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.OnReplay();
    }
}
