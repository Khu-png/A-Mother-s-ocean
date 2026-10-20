using UnityEngine;

public sealed class MenuTabButton : MonoBehaviour
{
    [SerializeField] private MenuTabId tab;
    [SerializeField] private RectTransform selectionTarget;
    [SerializeField] private RectTransform icon;
    [SerializeField] private float selectedScale = 1.14f;
    [SerializeField] private float selectedOffset = 10f;
    private Vector2 restingPosition;
    private Vector2 animationStartPosition;
    private Vector2 animationTargetPosition;
    private Vector3 animationStartScale;
    private Vector3 animationTargetScale;

    public MenuTabId Tab => tab;
    public float AnchorCenter => (selectionTarget.anchorMin.x + selectionTarget.anchorMax.x) * 0.5f;

    public void Initialize(bool selected)
    {
        restingPosition = icon.anchoredPosition;
        SetImmediate(selected);
    }

    public void BeginSelection(bool selected)
    {
        animationStartPosition = icon.anchoredPosition;
        animationStartScale = icon.localScale;
        animationTargetPosition = restingPosition + Vector2.up * (selected ? selectedOffset : 0f);
        animationTargetScale = selected ? Vector3.one * selectedScale : Vector3.one;
    }

    public void ApplySelection(float progress)
    {
        icon.anchoredPosition = Vector2.LerpUnclamped(
            animationStartPosition, animationTargetPosition, progress);
        icon.localScale = Vector3.LerpUnclamped(
            animationStartScale, animationTargetScale, progress);
    }

    private void SetImmediate(bool selected)
    {
        icon.anchoredPosition = restingPosition + Vector2.up * (selected ? selectedOffset : 0f);
        icon.localScale = selected ? Vector3.one * selectedScale : Vector3.one;
    }
}
