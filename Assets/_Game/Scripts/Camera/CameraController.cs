using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private UnityEngine.Camera targetCamera;
    [SerializeField] private SafeArenaSettings safeArena = new SafeArenaSettings();
    private Transform cameraTransform;
    private SpriteRenderer cachedBackground;
    private Transform backgroundTransform;
    private int inspectorLayoutVersion;

    public int LayoutRevision
    {
        get
        {
            unchecked
            {
                int revision = Screen.width;
                revision = revision * 31 + Screen.height;
                revision = revision * 31 + Screen.safeArea.GetHashCode();
                revision = revision * 31 + inspectorLayoutVersion;
                if (targetCamera == null) return revision;
                revision = revision * 31 + targetCamera.orthographicSize.GetHashCode();
                revision = revision * 31 + targetCamera.aspect.GetHashCode();
                revision = revision * 31 + CameraTransform().position.GetHashCode();
                return revision * 31 + CameraTransform().rotation.GetHashCode();
            }
        }
    }

    void Awake()
    {
        if (targetCamera != null) cameraTransform = targetCamera.transform;
    }

    void OnValidate()
    {
        inspectorLayoutVersion++;
    }

    public void FitBackground(SpriteRenderer background)
    {
        if (targetCamera == null || background == null || background.sprite == null) return;
        if (cameraTransform == null) cameraTransform = targetCamera.transform;
        if (cachedBackground != background)
        {
            cachedBackground = background;
            backgroundTransform = background.transform;
        }

        float height = targetCamera.orthographicSize * 2f;
        Vector2 spriteSize = background.sprite.bounds.size;
        float scale = Mathf.Max(height / spriteSize.y, height * targetCamera.aspect / spriteSize.x) * 1.02f;
        Transform view = backgroundTransform;
        Vector3 worldScale = view.lossyScale;
        if (Mathf.Abs(worldScale.x) < 0.0001f || Mathf.Abs(worldScale.y) < 0.0001f) return;
        view.localScale = new Vector3(view.localScale.x * scale / Mathf.Abs(worldScale.x),
            view.localScale.y * scale / Mathf.Abs(worldScale.y), view.localScale.z);
        view.position = cameraTransform.position + cameraTransform.forward * 20f;
        view.rotation = cameraTransform.rotation;
    }

    public SafeArenaWorldLayout CalculateGridLayout(int columns, int rows, float boardZ)
    {
        if (targetCamera == null) return default;
        SafeArenaScreenLayout screenLayout = SafeArenaCalculator.Calculate(
            Screen.safeArea, safeArena, columns, rows);
        Vector3 worldCenter = ScreenToBoard(screenLayout.BoardCenter, boardZ);
        float samplePixels = Mathf.Max(1f, screenLayout.CellSize);
        Vector3 worldRight = ScreenToBoard(
            screenLayout.BoardCenter + Vector2.right * samplePixels, boardZ);
        Vector3 worldUp = ScreenToBoard(
            screenLayout.BoardCenter + Vector2.up * samplePixels, boardZ);
        float worldCellSize = Mathf.Min(
            Vector3.Distance(worldCenter, worldRight),
            Vector3.Distance(worldCenter, worldUp));
        float worldPerPixel = worldCellSize / samplePixels;
        return new SafeArenaWorldLayout(worldCenter, worldCellSize,
            screenLayout.Spacing * worldPerPixel);
    }

    private Transform CameraTransform()
    {
        if (cameraTransform == null && targetCamera != null) cameraTransform = targetCamera.transform;
        return cameraTransform;
    }

    private Vector3 ScreenToBoard(Vector2 screenPoint, float boardZ)
    {
        Ray ray = targetCamera.ScreenPointToRay(screenPoint);
        float distance = (boardZ - ray.origin.z) / ray.direction.z;
        return ray.GetPoint(distance);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Application.isPlaying || targetCamera == null || !safeArena.ShowEditorPreview) return;
        SafeArenaScreenLayout layout = SafeArenaCalculator.Calculate(Screen.safeArea, safeArena, 1, 1);
        DrawScreenRect(layout.Arena, safeArena.PreviewColor);
        Color usableColor = safeArena.PreviewColor;
        usableColor.a *= 0.55f;
        DrawScreenRect(layout.UsableArea, usableColor);
    }

    private void DrawScreenRect(Rect screenRect, Color color)
    {
        Vector3 bottomLeft = ScreenToBoard(screenRect.min, 0f);
        Vector3 bottomRight = ScreenToBoard(
            new Vector2(screenRect.xMax, screenRect.yMin), 0f);
        Vector3 topRight = ScreenToBoard(screenRect.max, 0f);
        Vector3 topLeft = ScreenToBoard(
            new Vector2(screenRect.xMin, screenRect.yMax), 0f);
        Gizmos.color = color;
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
#endif
}
