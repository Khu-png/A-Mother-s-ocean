using System.Collections;
using UnityEngine;

public sealed class MenuNavigationController : MonoBehaviour
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform selectionIndicator;
    [SerializeField] private MenuPageBinding[] pages;
    [SerializeField] private MenuTabButton[] tabs;
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.25f;
    [SerializeField] private MenuTabId initialTab = MenuTabId.Home;
    private Coroutine transition;
    private MenuTabId currentTab;
    private float indicatorHalfWidth;
    private bool initialized;

    public MenuTabId CurrentTab => currentTab;

    public void Initialize()
    {
        if (initialized) return;
        initialized = true;
        currentTab = initialTab;
        indicatorHalfWidth = (selectionIndicator.anchorMax.x - selectionIndicator.anchorMin.x) * 0.5f;
        for (int i = 0; i < tabs.Length; i++)
            tabs[i].Initialize(tabs[i].Tab == currentTab);
        SetIndicatorCenter(TabFor(currentTab).AnchorCenter);
        ShowOnly(currentTab);
    }

    public void Select(MenuTabId targetTab)
    {
        if (targetTab == currentTab) return;
        if (transition != null) StopCoroutine(transition);
        MenuTabId previousTab = currentTab;
        currentTab = targetTab;
        transition = StartCoroutine(SlideTo(previousTab, targetTab));
    }

    private IEnumerator SlideTo(MenuTabId previousTab, MenuTabId targetTab)
    {
        RectTransform previous = PageFor(previousTab);
        RectTransform target = PageFor(targetTab);
        float direction = targetTab > previousTab ? 1f : -1f;
        float width = viewport.rect.width;
        Vector2 previousStart = previous.anchoredPosition;
        Vector2 targetStart = PrepareTarget(previous, target, direction * width);
        float indicatorStart = IndicatorCenter();
        float indicatorTarget = TabFor(targetTab).AnchorCenter;
        BeginTabAnimation(targetTab);
        yield return Animate(previous, target, previousStart, targetStart,
            direction, width, indicatorStart, indicatorTarget);
        CompleteTransition(previous, target, indicatorTarget);
    }

    private Vector2 PrepareTarget(RectTransform previous, RectTransform target, float startX)
    {
        bool targetWasActive = target.gameObject.activeSelf;
        Vector2 targetStart = targetWasActive ? target.anchoredPosition : Vector2.right * startX;
        HideOtherPages(previous, target);
        target.gameObject.SetActive(true);
        target.anchoredPosition = targetStart;
        return targetStart;
    }

    private IEnumerator Animate(RectTransform previous, RectTransform target,
        Vector2 previousStart, Vector2 targetStart, float direction, float width,
        float indicatorStart, float indicatorTarget)
    {
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionDuration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            previous.anchoredPosition = Vector2.LerpUnclamped(
                previousStart, Vector2.left * direction * width, eased);
            target.anchoredPosition = Vector2.LerpUnclamped(targetStart, Vector2.zero, eased);
            SetIndicatorCenter(Mathf.LerpUnclamped(indicatorStart, indicatorTarget, eased));
            ApplyTabAnimation(eased);
            yield return null;
        }
    }

    private void HideOtherPages(RectTransform previous, RectTransform target)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            RectTransform page = pages[i].Page;
            if (page != previous && page != target) page.gameObject.SetActive(false);
        }
    }

    private void CompleteTransition(RectTransform previous, RectTransform target, float indicatorTarget)
    {
        previous.gameObject.SetActive(false);
        target.anchoredPosition = Vector2.zero;
        SetIndicatorCenter(indicatorTarget);
        ApplyTabAnimation(1f);
        transition = null;
    }

    private void ShowOnly(MenuTabId visibleTab)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            bool visible = pages[i].Tab == visibleTab;
            pages[i].Page.gameObject.SetActive(visible);
            pages[i].Page.anchoredPosition = Vector2.zero;
        }
    }

    private void BeginTabAnimation(MenuTabId targetTab)
    {
        for (int i = 0; i < tabs.Length; i++) tabs[i].BeginSelection(tabs[i].Tab == targetTab);
    }

    private void ApplyTabAnimation(float progress)
    {
        for (int i = 0; i < tabs.Length; i++) tabs[i].ApplySelection(progress);
    }

    private float IndicatorCenter()
    {
        return (selectionIndicator.anchorMin.x + selectionIndicator.anchorMax.x) * 0.5f;
    }

    private void SetIndicatorCenter(float center)
    {
        Vector2 minimum = selectionIndicator.anchorMin;
        Vector2 maximum = selectionIndicator.anchorMax;
        minimum.x = center - indicatorHalfWidth;
        maximum.x = center + indicatorHalfWidth;
        selectionIndicator.anchorMin = minimum;
        selectionIndicator.anchorMax = maximum;
    }

    private RectTransform PageFor(MenuTabId tab)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i].Tab == tab) return pages[i].Page;
        }
        throw new UnityException($"Menu page is not configured for {tab}.");
    }

    private MenuTabButton TabFor(MenuTabId tab)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].Tab == tab) return tabs[i];
        }
        throw new UnityException($"Menu tab is not configured for {tab}.");
    }
}
