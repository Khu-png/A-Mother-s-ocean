using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Vector2Int Cell => cell;

    public void Setup(Vector2Int setupCell, Vector3 localPosition, Color color)
    {
        cell = setupCell;
        transform.localPosition = localPosition;

        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = 0;
    }

    public void ConfigureRenderer(SpriteRenderer setupRenderer)
    {
        spriteRenderer = setupRenderer;
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }
}
