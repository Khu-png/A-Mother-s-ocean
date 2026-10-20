using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MenuGiftPage : MonoBehaviour
{
    [SerializeField] private GameObject[] rewardStateElements;
    [SerializeField] private GameObject noRewardState;
    [SerializeField] private Button openButton;
    [SerializeField] private TMP_Text openButtonLabel;
    [SerializeField] private TMP_Text[] rewardQuantities;
    [SerializeField, Min(0f)] private float openingDuration = 0.45f;
    [SerializeField, Min(0f)] private float receivedDuration = 0.6f;
    private MenuCatalog catalog;
    public void Initialize(MenuCatalog menuCatalog)
    {
        catalog = menuCatalog;
        openButton.onClick.AddListener(OpenGift);
        Refresh();
    }

    public void Refresh()
    {
        bool available = MenuPlayerProgress.Chests > 0;
        for (int i = 0; i < rewardStateElements.Length; i++)
            rewardStateElements[i].SetActive(available);
        noRewardState.SetActive(!available);
        openButton.interactable = available && catalog.GiftRewardsEnabled;
        openButtonLabel.text = catalog.GiftRewardsEnabled
            ? $"Open x{MenuPlayerProgress.Chests}"
            : "Unavailable";
        for (int i = 0; i < rewardQuantities.Length; i++)
            rewardQuantities[i].text = catalog.GiftRewardsEnabled ? $"x{catalog.GiftRewards[i].Amount}" : "x--";
    }

    void OnDestroy()
    {
        openButton.onClick.RemoveListener(OpenGift);
    }

    private void OpenGift()
    {
        StartCoroutine(PlayOpening());
    }

    private IEnumerator PlayOpening()
    {
        openButton.interactable = false;
        openButtonLabel.text = "Opening...";
        yield return new WaitForSecondsRealtime(openingDuration);
        MenuPlayerProgress.ClaimGift(catalog.GiftRewards);
        openButtonLabel.text = "Received";
        yield return new WaitForSecondsRealtime(receivedDuration);
        Refresh();
    }
}
