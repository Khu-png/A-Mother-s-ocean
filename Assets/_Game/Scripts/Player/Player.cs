using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color playerColor = new Color(1f, 0.78f, 0.22f, 1f);
    [SerializeField] private float moveDuration = 0.16f;
    [SerializeField] private float swimArcHeight = 0.08f;
    [SerializeField] private float idleBobHeight = 0.035f;
    [SerializeField] private float idleBobSpeed = 3f;

    private Transform cachedTransform;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private Vector3 baseScale;
    private float moveTimer;

    public bool IsMoving { get; private set; }
    public Vector2Int CurrentCell { get; private set; }

    private void Awake()
    {
        cachedTransform = transform;
        EnsureRenderer();
        moveDuration = Mathf.Max(0.01f, moveDuration);
    }

    private void Update()
    {
        if (!IsMoving)
        {
            cachedTransform.position = moveTarget + Vector3.up * (Mathf.Sin(Time.time * idleBobSpeed) * idleBobHeight);
            return;
        }

        moveTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(moveTimer / moveDuration);
        float easedProgress = progress * progress * (3f - 2f * progress);
        Vector3 arcOffset = Vector3.up * (Mathf.Sin(progress * Mathf.PI) * swimArcHeight);
        cachedTransform.position = Vector3.Lerp(moveStart, moveTarget, easedProgress) + arcOffset;
        cachedTransform.localScale = baseScale * (1f + Mathf.Sin(progress * Mathf.PI) * 0.1f);

        if (progress >= 1f)
        {
            cachedTransform.position = moveTarget;
            cachedTransform.localScale = baseScale;
            IsMoving = false;
        }
    }

    public void SetCell(Vector2Int cell, Vector3 worldPosition, float cellSize)
    {
        CurrentCell = cell;
        cachedTransform.position = worldPosition;
        baseScale = Vector3.one * FitSpriteScale(cellSize, 0.82f);
        cachedTransform.localScale = baseScale;
        moveTarget = worldPosition;
        IsMoving = false;
    }

    public void MoveTo(Vector2Int cell, Vector3 worldPosition)
    {
        CurrentCell = cell;
        moveStart = cachedTransform.position;
        moveTarget = worldPosition;
        moveTimer = 0f;
        IsMoving = true;
        spriteRenderer.flipX = worldPosition.x < moveStart.x;
    }

    private void EnsureRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = CreateSquareSprite();
        }

        spriteRenderer.color = playerColor;
        spriteRenderer.sortingOrder = 5;
    }

    private float FitSpriteScale(float cellSize, float fill)
    {
        Vector2 size = spriteRenderer.sprite.bounds.size;
        float longestSide = Mathf.Max(size.x, size.y);
        return longestSide > 0f ? cellSize * fill / longestSide : cellSize * fill;
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
