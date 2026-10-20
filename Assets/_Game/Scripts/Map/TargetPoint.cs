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
    private Transform cachedTransform;
    public bool IsCollected { get; private set; }
    public Vector2Int Cell => cell;

    void Awake()
    {
        cachedTransform = transform;
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * bobSpeed + timeOffset);
        cachedTransform.localPosition = basePosition + Vector3.up * (wave * bobHeight);
        cachedTransform.localScale = baseScale * (1f + wave * 0.045f);
    }

    public void Setup(Vector2Int setupCell, Vector3 localPosition, float cellSize, Sprite sprite, Color color)
    {
        cell = setupCell;
        cachedTransform = transform;
        basePosition = localPosition;
        timeOffset = (cell.x * 0.73f + cell.y * 1.31f) % Mathf.PI;
        cachedTransform.localPosition = basePosition;

        spriteRenderer = spriteRenderer != null ? spriteRenderer : gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = 3;
        baseScale = cachedTransform.localScale;
        cachedTransform.localScale = baseScale;
        ResetTarget();
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        basePosition = localPosition;
        cachedTransform.localPosition = localPosition;
    }

    public void Collect()
    {
        if (IsCollected) return;
        IsCollected = true;
        SoundManager.Ins.PlaySound(GameConstants.COLLECT_SOUND);
        gameObject.SetActive(false);
    }

    public void ResetTarget()
    {
        IsCollected = false;
        gameObject.SetActive(true);
        cachedTransform.localPosition = basePosition;
        cachedTransform.localScale = baseScale;
    }

}
