using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private UnityEngine.Camera targetCamera;
    [SerializeField] private bool fitCameraOnPlay = false;
    [SerializeField] private float cameraPadding = 0.45f;

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
