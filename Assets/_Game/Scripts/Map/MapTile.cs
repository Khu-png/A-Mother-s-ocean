using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMesh label;

    public Vector2Int Cell => cell;

    public void Setup(Vector2Int setupCell, Vector3 worldPosition, float cellSize, Sprite sprite, Color color)
    {
        cell = setupCell;
        transform.position = worldPosition;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one * (cellSize * 0.92f);

        spriteRenderer = spriteRenderer != null ? spriteRenderer : gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = MapTileAssetLibrary.SpriteOrFallback("tile_empty", sprite);
        spriteRenderer.sortingOrder = -1;
        spriteRenderer.color = spriteRenderer.sprite == sprite ? color : Color.white;

        label = label != null ? label : new GameObject("Label").AddComponent<TextMesh>();
        label.transform.SetParent(transform);
        label.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        label.transform.localScale = Vector3.one * (0.18f / cellSize);
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontStyle = FontStyle.Bold;
        SetLabel(string.Empty, Color.white);
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void SetSprite(Sprite sprite)
    {
        if (sprite != null) spriteRenderer.sprite = sprite;
    }

    public void SetVisualRotation(float zDegrees)
    {
        transform.localEulerAngles = new Vector3(0f, 0f, zDegrees);
        label.transform.localEulerAngles = new Vector3(0f, 0f, -zDegrees);
    }

    public void SetLabel(string text, Color color)
    {
        label.text = text;
        label.color = color;
    }
}
