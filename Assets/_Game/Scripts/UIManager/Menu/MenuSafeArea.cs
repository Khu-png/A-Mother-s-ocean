using UnityEngine;

public sealed class MenuSafeArea : MonoBehaviour
{
    [SerializeField] private RectTransform safeAreaRoot;
    [SerializeField] private RectOffset padding;
    private Rect appliedArea;
    private Vector2Int appliedResolution;

    void OnEnable()
    {
        Apply();
    }

    void Update()
    {
        Vector2Int resolution = new Vector2Int(Screen.width, Screen.height);
        if (Screen.safeArea == appliedArea && resolution == appliedResolution) return;
        Apply();
    }

    private void Apply()
    {
        // The menu lives inside the portrait frame, so anchors are relative to that frame.
        Rect area = PortraitScreen.SafeArea;
        safeAreaRoot.anchorMin = PortraitScreen.ToFrameNormalized(
            new Vector2(area.xMin + padding.left, area.yMin + padding.bottom));
        safeAreaRoot.anchorMax = PortraitScreen.ToFrameNormalized(
            new Vector2(area.xMax - padding.right, area.yMax - padding.top));
        safeAreaRoot.anchoredPosition = Vector2.zero;
        safeAreaRoot.sizeDelta = Vector2.zero;
        appliedArea = Screen.safeArea;
        appliedResolution = new Vector2Int(Screen.width, Screen.height);
    }
}
