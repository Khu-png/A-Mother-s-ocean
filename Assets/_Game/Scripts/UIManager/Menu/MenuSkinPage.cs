using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public sealed class MenuSkinSlotView
{
    [SerializeField] private Button button;
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text skinName;
    [SerializeField] private TMP_Text ownershipState;

    public Button Button => button;

    public void Display(MenuSkinOffer offer, bool selected)
    {
        portrait.sprite = offer.Portrait;
        portrait.color = offer.Tint;
        skinName.text = offer.DisplayName;
        ownershipState.text = OwnershipText(offer);
        button.interactable = !selected;
    }

    public void SetVisible(bool visible)
    {
        button.gameObject.SetActive(visible);
    }

    private string OwnershipText(MenuSkinOffer offer)
    {
        if (MenuPlayerProgress.EquippedSkin == offer.Id) return "Equipped";
        return MenuPlayerProgress.IsSkinOwned(offer.Id) ? "Owned" : "Locked";
    }
}

public sealed class MenuSkinPage : MonoBehaviour
{
    [SerializeField] private Image selectedPreview;
    [SerializeField] private TMP_Text selectedName;
    [SerializeField] private TMP_Text equipButtonLabel;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private MenuSkinSlotView[] slots;
    private MenuCatalog catalog;
    private readonly List<int> OWNED_SKINS = new List<int>();
    private int selectedIndex;

    public void Initialize(MenuCatalog menuCatalog)
    {
        catalog = menuCatalog;
        previousButton.onClick.AddListener(SelectPrevious);
        nextButton.onClick.AddListener(SelectNext);
        equipButton.onClick.AddListener(EquipSelected);
        for (int i = 0; i < slots.Length; i++)
        {
            int slotIndex = i;
            slots[i].Button.onClick.AddListener(() => Select(slotIndex));
        }
        RebuildOwnedSkins();
        selectedIndex = OWNED_SKINS.IndexOf(EquippedSkinIndex());
        Refresh();
    }

    public void Refresh()
    {
        int selectedCatalogIndex = OWNED_SKINS[selectedIndex];
        RebuildOwnedSkins();
        selectedIndex = OWNED_SKINS.IndexOf(selectedCatalogIndex);
        MenuSkinOffer selected = catalog.Skins[OWNED_SKINS[selectedIndex]];
        selectedPreview.sprite = selected.Portrait;
        selectedPreview.color = selected.Tint;
        selectedName.text = selected.DisplayName;
        bool owned = MenuPlayerProgress.IsSkinOwned(selected.Id);
        bool equipped = MenuPlayerProgress.EquippedSkin == selected.Id;
        equipButton.interactable = owned && !equipped;
        equipButtonLabel.text = equipped ? "Equipped" : owned ? "Equip" : "Locked";
        previousButton.interactable = OWNED_SKINS.Count > 1;
        nextButton.interactable = OWNED_SKINS.Count > 1;
        for (int i = 0; i < slots.Length; i++)
        {
            bool visible = i < OWNED_SKINS.Count;
            slots[i].SetVisible(visible);
            if (visible) slots[i].Display(catalog.Skins[OWNED_SKINS[i]], i == selectedIndex);
        }
    }

    void OnDestroy()
    {
        previousButton.onClick.RemoveListener(SelectPrevious);
        nextButton.onClick.RemoveListener(SelectNext);
        equipButton.onClick.RemoveListener(EquipSelected);
    }

    private void SelectPrevious()
    {
        Select((selectedIndex - 1 + OWNED_SKINS.Count) % OWNED_SKINS.Count);
    }

    private void SelectNext()
    {
        Select((selectedIndex + 1) % OWNED_SKINS.Count);
    }

    private void Select(int index)
    {
        SoundManager.Ins.PlayButtonSound();
        selectedIndex = index;
        Refresh();
    }

    private void EquipSelected()
    {
        SoundManager.Ins.PlayButtonSound();
        MenuPlayerProgress.EquipSkin(catalog.Skins[OWNED_SKINS[selectedIndex]].Id);
        Refresh();
    }

    private void RebuildOwnedSkins()
    {
        OWNED_SKINS.Clear();
        for (int i = 0; i < catalog.Skins.Length; i++)
        {
            if (MenuPlayerProgress.IsSkinOwned(catalog.Skins[i].Id)) OWNED_SKINS.Add(i);
        }
    }

    private int EquippedSkinIndex()
    {
        for (int i = 0; i < catalog.Skins.Length; i++)
        {
            if (catalog.Skins[i].Id == MenuPlayerProgress.EquippedSkin) return i;
        }
        throw new UnityException("Equipped skin is absent from the menu catalog.");
    }
}
