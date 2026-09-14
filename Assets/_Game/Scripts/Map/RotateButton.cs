using UnityEngine;

public class RotateButton : MonoBehaviour
{
    [SerializeField] private MapTile tile;
    [SerializeField] private Color buttonColor = new Color(1f, 0.48f, 0.12f, 1f);
    private Vector3 baseScale;
    private float pulseTimer;

    public MapTile Tile => tile;

    public void BindTile(MapTile setupTile)
    {
        tile = setupTile;
    }

    private void Update()
    {
        if (pulseTimer <= 0f) return;
        pulseTimer = Mathf.Max(0f, pulseTimer - Time.deltaTime * 5f);
        float pulse = Mathf.Sin(pulseTimer * Mathf.PI) * 0.14f;
        transform.localScale = baseScale * (1f + pulse);
    }

    public void Setup(MapTile setupTile, Color color)
    {
        tile = setupTile;
        buttonColor = color;
        baseScale = transform.localScale;
        Sprite sprite = MapTileAssetLibrary.Sprite("tile_rotate");
        tile.SetSprite(sprite);
        tile.SetColor(sprite != null ? Color.white : buttonColor);
        tile.SetLabel(sprite != null ? string.Empty : "@", Color.white);
    }

    public void Pulse()
    {
        pulseTimer = 1f;
    }
}
