using UnityEngine;

// Keeps the UI frame, the side bars and the game camera's viewport matched to
// PortraitScreen.FrameRect whenever the screen size changes.
public sealed class PortraitFrame : MonoBehaviour
{
    [SerializeField] private RectTransform frame;
    [SerializeField] private RectTransform leftBar;
    [SerializeField] private RectTransform rightBar;
    [SerializeField] private Camera gameCamera;
    private Vector2Int appliedResolution;

    void Awake()
    {
        Apply();
    }

    void Update()
    {
        if (new Vector2Int(Screen.width, Screen.height) != appliedResolution) Apply();
    }

    private void Apply()
    {
        appliedResolution = new Vector2Int(Screen.width, Screen.height);
        Rect pixels = PortraitScreen.FrameRect;
        float left = pixels.xMin / Screen.width;
        float right = pixels.xMax / Screen.width;
        SetAnchors(frame, left, right);
        SetAnchors(leftBar, 0f, left);
        SetAnchors(rightBar, right, 1f);
        bool hasBars = left > 0f;
        leftBar.gameObject.SetActive(hasBars);
        rightBar.gameObject.SetActive(hasBars);
        gameCamera.rect = new Rect(left, 0f, right - left, 1f);
    }

    private static void SetAnchors(RectTransform target, float left, float right)
    {
        target.anchorMin = new Vector2(left, 0f);
        target.anchorMax = new Vector2(right, 1f);
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
    }
}
