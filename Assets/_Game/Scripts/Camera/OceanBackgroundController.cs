using UnityEngine;

public class OceanBackgroundController : MonoBehaviour
{
    [SerializeField] private UnityEngine.Camera targetCamera;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float coverPadding = 1.18f;
    [SerializeField] private float driftAmount = 0.16f;
    [SerializeField] private float driftSpeed = 0.35f;
    [SerializeField] private float breathingScale = 0.018f;

    private Vector3 baseScale;

    private void Start()
    {
        FitToCamera();
    }

    private void LateUpdate()
    {
        if (targetCamera == null || spriteRenderer == null)
        {
            return;
        }

        FitToCamera();

        float wave = Time.time * driftSpeed;
        Vector3 drift = new Vector3(
            Mathf.Sin(wave) * driftAmount,
            Mathf.Cos(wave * 0.73f) * driftAmount,
            0f);

        Vector3 cameraPosition = targetCamera.transform.position;
        transform.position = new Vector3(cameraPosition.x, cameraPosition.y, transform.position.z) + drift;
        transform.localScale = baseScale * (1f + Mathf.Sin(wave * 1.7f) * breathingScale);
    }

    private void FitToCamera()
    {
        if (targetCamera == null || spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        float height = targetCamera.orthographicSize * 2f;
        float width = height * targetCamera.aspect;
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        float fitScale = Mathf.Max(width / spriteSize.x, height / spriteSize.y) * coverPadding;
        baseScale = new Vector3(fitScale, fitScale, 1f);
    }
}
