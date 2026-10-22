using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color closedColor;
    [SerializeField] private Color openColor;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    public bool IsOpen { get; private set; }
    public Vector2Int Cell => cell;

    public void Setup(Vector2Int setupCell, Vector3 localPosition, Color setupClosedColor, Color setupOpenColor)
    {
        cell = setupCell;
        closedColor = setupClosedColor;
        openColor = setupOpenColor;
        transform.localPosition = localPosition;

        spriteRenderer.sprite = closedSprite;
        spriteRenderer.sortingOrder = 2;
        SetOpen(false);
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }

    public void SetOpen(bool isOpen)
    {
        IsOpen = isOpen;
        spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
        spriteRenderer.color = isOpen ? openColor : closedColor;
    }

    public void ConfigureVisuals(SpriteRenderer setupRenderer, Sprite setupClosedSprite, Sprite setupOpenSprite)
    {
        spriteRenderer = setupRenderer;
        closedSprite = setupClosedSprite;
        openSprite = setupOpenSprite;
    }
}
