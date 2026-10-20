using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Vector2Int Cell => cell;

    public void Setup(Vector2Int setupCell, Vector3 localPosition, float cellSize, Sprite sprite, Color color)
    {
        cell = setupCell;
        transform.localPosition = localPosition;

        spriteRenderer = spriteRenderer != null ? spriteRenderer : gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = MapTileAssetLibrary.SpriteOrFallback("tile_start", sprite);
        spriteRenderer.color = spriteRenderer.sprite == sprite ? color : Color.white;
        spriteRenderer.sortingOrder = 0;
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }
}
