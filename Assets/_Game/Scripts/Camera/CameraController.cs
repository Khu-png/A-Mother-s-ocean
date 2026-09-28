using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private UnityEngine.Camera targetCamera;
    [SerializeField] private bool fitCameraOnPlay = false;
    [SerializeField] private float cameraPadding = 0.45f;

    public void FitBackground(SpriteRenderer background)
    {
        if (targetCamera == null || background == null || background.sprite == null) return;

        float height = targetCamera.orthographicSize * 2f;
        Vector2 spriteSize = background.sprite.bounds.size;
        float scale = Mathf.Max(height / spriteSize.y, height * targetCamera.aspect / spriteSize.x) * 1.02f;
        Transform view = background.transform;
        Vector3 worldScale = view.lossyScale;
        if (Mathf.Abs(worldScale.x) < 0.0001f || Mathf.Abs(worldScale.y) < 0.0001f) return;
        view.localScale = new Vector3(view.localScale.x * scale / Mathf.Abs(worldScale.x),
            view.localScale.y * scale / Mathf.Abs(worldScale.y), view.localScale.z);
        view.position = targetCamera.transform.position + targetCamera.transform.forward * 20f;
        view.rotation = targetCamera.transform.rotation;
    }

    public void FitToGrid(int gridWidth, int gridHeight, float cellSize)
    {
        if (!fitCameraOnPlay || targetCamera == null)
        {
            return;
        }

        cameraPadding = Mathf.Max(0f, cameraPadding);
        targetCamera.orthographic = true;
        transform.position = new Vector3(0f, 0f, -10f);

        float aspect = Mathf.Max(0.1f, targetCamera.aspect);
        float fitHeight = gridHeight * cellSize * 0.5f;
        float fitWidth = gridWidth * cellSize * 0.5f / aspect;
        targetCamera.orthographicSize = Mathf.Max(fitHeight, fitWidth) + cameraPadding;
    }
}
