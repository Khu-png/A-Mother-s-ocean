using UnityEngine;

public class RotateButton : MonoBehaviour
{
    [SerializeField] private MapTile tile;
    [SerializeField] private Color buttonColor = new Color(1f, 0.48f, 0.12f, 1f);
    private Vector3 baseScale;
    private float pulseTimer;
    private Transform cachedTransform;

    public MapTile Tile => tile;

    void Awake()
    {
        cachedTransform = transform;
    }

    void Update()
    {
        if (pulseTimer <= 0f) return;
        pulseTimer = Mathf.Max(0f, pulseTimer - Time.deltaTime * 5f);
        float pulse = Mathf.Sin(pulseTimer * Mathf.PI) * 0.14f;
        cachedTransform.localScale = baseScale * (1f + pulse);
    }

    public void BindTile(MapTile setupTile)
    {
        tile = setupTile;
    }

    public void Setup(MapTile setupTile, Color color)
    {
        tile = setupTile;
        buttonColor = color;
        cachedTransform = transform;
        baseScale = cachedTransform.localScale;
        tile.SetColor(buttonColor);
        tile.SetLabel(string.Empty, Color.white);
    }

    public void Pulse()
    {
        pulseTimer = 1f;
    }

    public void StopPulse()
    {
        pulseTimer = 0f;
        cachedTransform.localScale = baseScale;
    }
}
