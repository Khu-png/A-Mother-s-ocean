using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MenuCatalog skinCatalog;
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

    void Awake()
    {
        cachedTransform = transform;
        ApplyEquippedSkin();
        moveDuration = Mathf.Max(0.01f, moveDuration);
    }

    void Update()
    {
        if (GameManager.IsState(GameState.Pause)) return;

        if (!IsMoving)
        {
            cachedTransform.localPosition = moveTarget + Vector3.up * (Mathf.Sin(Time.time * idleBobSpeed) * idleBobHeight);
            return;
        }

        moveTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(moveTimer / moveDuration);
        float easedProgress = progress * progress * (3f - 2f * progress);
        Vector3 arcOffset = Vector3.up * (Mathf.Sin(progress * Mathf.PI) * swimArcHeight);
        cachedTransform.localPosition = Vector3.Lerp(moveStart, moveTarget, easedProgress) + arcOffset;
        cachedTransform.localScale = baseScale * (1f + Mathf.Sin(progress * Mathf.PI) * 0.1f);

        if (progress >= 1f)
        {
            cachedTransform.localPosition = moveTarget;
            cachedTransform.localScale = baseScale;
            IsMoving = false;
        }
    }

    public void SetCell(Vector2Int cell, Vector3 localPosition, float cellSize)
    {
        CurrentCell = cell;
        cachedTransform.localPosition = localPosition;
        cachedTransform.localRotation = Quaternion.identity;
        spriteRenderer.flipX = false;
        baseScale = Vector3.one * FitSpriteScale(cellSize, 0.82f);
        cachedTransform.localScale = baseScale;
        moveTarget = localPosition;
        IsMoving = false;
    }

    public void MoveTo(Vector2Int cell, Vector3 localPosition)
    {
        FaceDirection(cell - CurrentCell);
        CurrentCell = cell;
        moveStart = cachedTransform.localPosition;
        moveTarget = localPosition;
        moveTimer = 0f;
        IsMoving = true;
    }

    public void ApplyLayout(Vector3 localPosition)
    {
        moveStart = localPosition;
        moveTarget = localPosition;
        cachedTransform.localPosition = localPosition;
        cachedTransform.localScale = baseScale;
        IsMoving = false;
    }

    private void ApplyEquippedSkin()
    {
        MenuPlayerProgress.Initialize(skinCatalog.DefaultSkinId);
        MenuSkinOffer skin = skinCatalog.GetSkin(MenuPlayerProgress.EquippedSkin);
        spriteRenderer.sprite = skin.Portrait;
        spriteRenderer.color = skin.Tint;
    }

    private float FitSpriteScale(float cellSize, float fill)
    {
        Vector2 size = spriteRenderer.sprite.bounds.size;
        float longestSide = Mathf.Max(size.x, size.y);
        return cellSize * fill / longestSide;
    }

    private void FaceDirection(Vector2Int direction)
    {
        if (direction.x != 0) spriteRenderer.flipX = direction.x < 0;
        cachedTransform.localRotation = Quaternion.identity;
    }

}
