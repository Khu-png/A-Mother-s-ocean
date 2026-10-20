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
        Rect area = Screen.safeArea;
        float width = Screen.width;
        float height = Screen.height;
        safeAreaRoot.anchorMin = new Vector2(
            (area.xMin + padding.left) / width,
            (area.yMin + padding.bottom) / height);
        safeAreaRoot.anchorMax = new Vector2(
            (area.xMax - padding.right) / width,
            (area.yMax - padding.top) / height);
        safeAreaRoot.anchoredPosition = Vector2.zero;
        safeAreaRoot.sizeDelta = Vector2.zero;
        appliedArea = area;
        appliedResolution = new Vector2Int(Screen.width, Screen.height);
    }
}
