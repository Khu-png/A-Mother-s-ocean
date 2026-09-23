using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bobHeight = 0.035f;
    [SerializeField] private float bobSpeed = 2.6f;

    private Vector3 basePosition;
    private Vector3 baseScale;
    private float timeOffset;
    public bool IsCollected { get; private set; }
    public Vector2Int Cell => cell;

    private void Update()
    {
        float wave = Mathf.Sin(Time.time * bobSpeed + timeOffset);
        transform.position = basePosition + Vector3.up * (wave * bobHeight);
        transform.localScale = baseScale * (1f + wave * 0.045f);
    }

    public void Setup(Vector2Int setupCell, Vector3 worldPosition, float cellSize, Sprite sprite, Color color)
    {
        cell = setupCell;
        basePosition = worldPosition;
        timeOffset = (cell.x * 0.73f + cell.y * 1.31f) % Mathf.PI;
        transform.position = basePosition;

        spriteRenderer = spriteRenderer != null ? spriteRenderer : gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = 3;
        baseScale = transform.localScale;
        transform.localScale = baseScale;
        ResetTarget();
    }

    public void Collect()
    {
        IsCollected = true;
        gameObject.SetActive(false);
    }

    public void ResetTarget()
    {
        IsCollected = false;
        gameObject.SetActive(true);
        transform.position = basePosition;
        transform.localScale = baseScale;
    }

}
