using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color closedColor;
    [SerializeField] private Color openColor;
    private Sprite closedSprite;
    private Sprite openSprite;

    public bool IsOpen { get; private set; }
    public Vector2Int Cell => cell;

    public void Setup(Vector2Int setupCell, Vector3 worldPosition, float cellSize, Sprite sprite, Color setupClosedColor, Color setupOpenColor)
    {
        cell = setupCell;
        closedColor = setupClosedColor;
        openColor = setupOpenColor;
        closedSprite = MapTileAssetLibrary.Sprite("tile_finish_closed");
        openSprite = MapTileAssetLibrary.Sprite("tile_finish_open");
        transform.position = worldPosition;

        spriteRenderer = spriteRenderer != null ? spriteRenderer : gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite != null ? closedSprite : sprite;
        spriteRenderer.sortingOrder = 2;
        SetOpen(false);
    }

    public void SetOpen(bool isOpen)
    {
        IsOpen = isOpen;
        if (isOpen && openSprite != null) spriteRenderer.sprite = openSprite;
        else if (!isOpen && closedSprite != null) spriteRenderer.sprite = closedSprite;
        spriteRenderer.color = SpriteAssetLoaded() ? Color.white : IsOpen ? openColor : closedColor;
    }

    private bool SpriteAssetLoaded()
    {
        return IsOpen ? openSprite != null : closedSprite != null;
    }
}
