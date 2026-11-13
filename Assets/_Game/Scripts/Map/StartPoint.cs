using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Color winFadeColor;
    private Vector3 entranceBaseScale;

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

    public void BeginEntrance()
    {
        entranceBaseScale = transform.localScale;
    }

    public void ApplyEntranceScale(float factor)
    {
        transform.localScale = entranceBaseScale * factor;
    }

    public void BeginWinFade()
    {
        winFadeColor = spriteRenderer.color;
    }

    public void ApplyWinFade(float progress)
    {
        Color color = winFadeColor;
        color.a *= 1f - Mathf.SmoothStep(0f, 1f, progress);
        spriteRenderer.color = color;
    }
}
