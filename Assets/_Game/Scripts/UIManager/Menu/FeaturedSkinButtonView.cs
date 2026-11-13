using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The single action button under the featured skin. It carries the price itself:
// coin + price (or "Free") while the skin can be bought, "Owned" once it is yours.
// The coin sits right beside the number and the pair stays centred on the button.
[Serializable]
public sealed class FeaturedSkinButtonView
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private RectTransform coinIcon;
    [SerializeField, Min(0f)] private float coinGap = 10f;
    // Nudges the "coin + price" group to the left of the button centre.
    [SerializeField] private float groupShiftLeft = 24f;
    private static readonly Vector4 TEXT_MARGIN = new Vector4(6f, 2f, 6f, 2f);

    public Button Button => button;

    public void Display(MenuSkinOffer skin, bool purchasesEnabled)
    {
        bool owned = MenuPlayerProgress.IsSkinOwned(skin.Id);
        button.interactable = !owned && purchasesEnabled;
        label.text = owned ? "Owned" : PriceText(skin, purchasesEnabled);
        ShowCoin(!owned && purchasesEnabled && skin.Price > 0);
    }

    public void SetProcessing()
    {
        button.interactable = false;
        label.text = "...";
        ShowCoin(false);
    }

    // Reserves the coin's width on the left so "coin + number" is centred as one group,
    // then places the coin just left of the rendered text. The mesh is rebuilt even while
    // the Shop page is hidden, so the position is right before the page is shown.
    private void ShowCoin(bool visible)
    {
        coinIcon.gameObject.SetActive(visible);
        float reserved = visible ? coinIcon.rect.width + coinGap : 0f;
        // Extra right margin moves the centred text (and the coin beside it) to the left.
        float shift = visible ? groupShiftLeft * 2f : 0f;
        label.margin = TEXT_MARGIN + new Vector4(reserved, 0f, shift, 0f);
        if (!visible) return;
        label.ForceMeshUpdate(true);
        float textLeft = label.textBounds.min.x;
        coinIcon.anchoredPosition = new Vector2(textLeft - coinGap - coinIcon.rect.width * 0.5f, 0f);
    }

    private static string PriceText(MenuSkinOffer skin, bool purchasesEnabled)
    {
        if (!purchasesEnabled) return "Unavailable";
        return skin.Price > 0 ? skin.Price.ToString() : "Free";
    }
}
