using System.Collections;
using AssetKits.ParticleImage;
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
    [SerializeField] private ParticleImage[] confettiEffects;
    [Header("Coin Sound")]
    // One tick per coin; every coinsPerTier ticks the pitch steps up the scale.
    [SerializeField, Min(1)] private int coinsPerTier = 10;
    [SerializeField, Min(0.1f)] private float maxCountSeconds = 2f;
    [SerializeField, Min(0.005f)] private float minTickInterval = 0.02f;
    [SerializeField, Min(0.005f)] private float maxTickInterval = 0.06f;
    [SerializeField, Range(0f, 1f)] private float coinTickVolume = 0.6f;
    [Header("Firework Sync")]
    // Attack time of each bang in Win.mp3; the confetti bursts fire on these beats.
    [SerializeField] private float[] fireworkTimes = { 0.19f, 0.44f, 0.69f, 0.94f, 1.45f, 1.9f };
    private static readonly int INTRO_STATE = Animator.StringToHash("Intro");
    private static readonly int CLAIM_STATE = Animator.StringToHash("Claim");
    private bool readyToClaim;
    private bool claimed;
    private Coroutine sequence;
    private Coroutine fireworks;

    void OnDisable()
    {
        if (sequence != null) StopCoroutine(sequence);
        if (fireworks != null) StopCoroutine(fireworks);
        sequence = null;
        fireworks = null;
        readyToClaim = false;
    }

    public override void Open()
    {
        base.Open();
        if (sequence != null) StopCoroutine(sequence);
        claimed = false;
        readyToClaim = false;
        foreach (ParticleImage confettiEffect in confettiEffects) confettiEffect.Stop(true);
        claimButton.interactable = false;
        int reward = LevelManager.Ins.PendingWinCoins;
        rewardText.text = $"+{reward}";
        claimText.text = reward > 0 ? $"CLAIM {reward}" : "CONTINUE";
        walletText.text = MenuPlayerProgress.Coins.ToString();
        sequenceAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        sequence = StartCoroutine(ShowReward());
        fireworks = StartCoroutine(PlayFireworks());
    }

    // The sound starts with the screen, so each confetti burst is timed to a bang in it.
    private IEnumerator PlayFireworks()
    {
        SoundManager.Ins.PlaySound(GameConstants.WIN_SOUND);
        float elapsed = 0f;
        for (int bang = 0; bang < fireworkTimes.Length; bang++)
        {
            while (elapsed < fireworkTimes[bang])
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            confettiEffects[bang % confettiEffects.Length].Play();
        }
        fireworks = null;
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
        int reward = finalCoins - startingCoins;
        float interval = Mathf.Clamp(maxCountSeconds / Mathf.Max(1, reward), minTickInterval, maxTickInterval);
        int counted = 0;
        float elapsed = 0f;
        sequenceAnimator.Play(CLAIM_STATE, 0, 0f);
        sequenceAnimator.Update(0f);
        while (counted < reward || sequenceAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            elapsed += Time.unscaledDeltaTime;
            int due = Mathf.Min(reward, Mathf.FloorToInt(elapsed / interval));
            // Every coin that enters the wallet gets its own tick, in step with the number.
            for (; counted < due; counted++) PlayCoinTick(counted);
            walletText.text = (startingCoins + counted).ToString();
            rewardText.text = $"+{reward - counted}";
            yield return null;
        }
        sequence = null;
        LevelManager.Ins.ContinueAfterWin();
    }

    private void PlayCoinTick(int coinIndex)
    {
        SoundManager.Ins.PlaySound(GameConstants.COIN_SOUND,
            CoinBeatSound.Pitch(coinIndex / coinsPerTier), coinTickVolume);
    }

    public void OnClickReplay()
    {
        if (claimed) return;
        SoundManager.Ins.PlayButtonSound();
        LevelManager.Ins.OnReplay();
    }
}
