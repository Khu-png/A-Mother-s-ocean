using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private GridMap gridMap;

    public GridMap GridMap => gridMap;

    public void OnInit(CameraController cameraController)
    {
        if (gridMap == null) return;
        gridMap.SetCameraController(cameraController);
        gridMap.ResetPlayerToStart();
    }

    public void OnDespawn()
    {
        Destroy(gameObject);
    }
}
