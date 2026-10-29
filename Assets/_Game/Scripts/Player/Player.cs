using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform shadowTransform;
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private MenuCatalog skinCatalog;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.16f;
    [SerializeField] private float moveLiftHeight = 0.1f;
    [SerializeField] private float moveScaleBoost = 0.1f;
    [SerializeField] private float moveTiltAngle = 7f;

    [Header("Blocked Reaction")]
    [SerializeField, Min(0.01f)] private float blockedDuration = 0.24f;
    [SerializeField] private float blockedPushDistance = 0.1f;
    [SerializeField] private float blockedShakeDistance = 0.025f;
    [SerializeField] private float blockedShakeCycles = 2.5f;

    [Header("Idle")]
    [SerializeField] private float idleBobHeight = 0.035f;
    [SerializeField] private float idleBobSpeed = 3f;

    [Header("Shadow")]
    [SerializeField] private float shadowWidth = 0.56f;
    [SerializeField] private float shadowHeight = 0.16f;
    [SerializeField] private float shadowVerticalOffset = 0.24f;
    [SerializeField] private float shadowExpansion = 0.16f;
    [SerializeField, Range(0f, 1f)] private float liftedShadowOpacity = 0.55f;

    private Transform cachedTransform;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private Vector3 baseVisualScale;
    private Vector3 baseShadowScale;
    private Vector3 baseShadowPosition;
    private Color baseSpriteColor;
    private Color baseShadowColor;
    private float winOpacity = 1f;
    private float moveTimer;
    private float tiltDirection;
    private float blockedTimer;
    private Vector2 blockedDirection;
    private bool isBlocked;

    public bool IsMoving { get; private set; }
    public bool IsBusy => IsMoving || isBlocked;
    public Vector2Int CurrentCell { get; private set; }

    void Awake()
    {
        cachedTransform = transform;
        baseShadowColor = shadowRenderer.color;
        ApplyEquippedSkin();
        baseSpriteColor = spriteRenderer.color;
        moveDuration = Mathf.Max(0.01f, moveDuration);
    }

    void Update()
    {
        if (GameManager.IsState(GameState.Pause)) return;
        if (isBlocked) UpdateBlockedReaction();
        else if (IsMoving) UpdateMovement();
        else UpdateIdleVisual();
    }

    public void SetCell(Vector2Int cell, Vector3 localPosition, float cellSize)
    {
        CurrentCell = cell;
        cachedTransform.localPosition = localPosition;
        cachedTransform.localRotation = Quaternion.identity;
        cachedTransform.localScale = Vector3.one;
        spriteRenderer.flipX = false;
        baseVisualScale = Vector3.one * FitSpriteScale(cellSize, 0.82f);
        ConfigureShadow(cellSize);
        moveTarget = localPosition;
        IsMoving = false;
        isBlocked = false;
        winOpacity = 1f;
        spriteRenderer.color = baseSpriteColor;
        ResetDepthVisual();
    }

    public void MoveTo(Vector2Int cell, Vector3 localPosition)
    {
        Vector2Int direction = cell - CurrentCell;
        FaceDirection(direction);
        tiltDirection = TiltDirection(direction);
        CurrentCell = cell;
        moveStart = cachedTransform.localPosition;
        moveTarget = localPosition;
        moveTimer = 0f;
        IsMoving = true;
    }

    public void PlayBlocked(Vector2Int direction)
    {
        FaceDirection(direction);
        tiltDirection = TiltDirection(direction);
        blockedDirection = ((Vector2)direction).normalized;
        blockedTimer = 0f;
        isBlocked = true;
    }

    public void ApplyLayout(Vector3 localPosition)
    {
        moveStart = localPosition;
        moveTarget = localPosition;
        cachedTransform.localPosition = localPosition;
        IsMoving = false;
        isBlocked = false;
        ResetDepthVisual();
    }

    public void ApplyWinFade(float progress)
    {
        winOpacity = 1f - Mathf.SmoothStep(0f, 1f, progress);
        Color color = baseSpriteColor;
        color.a *= winOpacity;
        spriteRenderer.color = color;
        ApplyShadowDepth(0f);
    }

    private void UpdateMovement()
    {
        moveTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(moveTimer / moveDuration);
        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
        float lift = Mathf.Sin(progress * Mathf.PI);
        cachedTransform.localPosition = Vector3.Lerp(moveStart, moveTarget, easedProgress);
        ApplyLiftVisual(lift);

        if (progress < 1f) return;
        cachedTransform.localPosition = moveTarget;
        IsMoving = false;
        ResetDepthVisual();
    }

    private void UpdateIdleVisual()
    {
        float wave = Mathf.Sin(Time.time * idleBobSpeed);
        float bob = wave * idleBobHeight;
        visualTransform.localPosition = Vector3.up * bob;
        visualTransform.localRotation = Quaternion.identity;
        visualTransform.localScale = baseVisualScale;
        ApplyShadowDepth(Mathf.Abs(wave) * 0.12f);
    }

    private void UpdateBlockedReaction()
    {
        blockedTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(blockedTimer / blockedDuration);
        float pushProgress = Mathf.Clamp01(progress / 0.32f);
        float push = Mathf.Sin(pushProgress * Mathf.PI) * blockedPushDistance;
        float shakeProgress = Mathf.Clamp01((progress - 0.24f) / 0.76f);
        float shake = Mathf.Sin(shakeProgress * Mathf.PI * blockedShakeCycles * 2f)
            * blockedShakeDistance * (1f - shakeProgress);
        visualTransform.localPosition = blockedDirection * (push + shake);
        visualTransform.localRotation = Quaternion.Euler(0f, 0f,
            tiltDirection * moveTiltAngle * Mathf.Sin(progress * Mathf.PI) * 0.35f);
        visualTransform.localScale = baseVisualScale;
        ApplyShadowDepth(0f);

        if (progress < 1f) return;
        isBlocked = false;
        ResetDepthVisual();
    }

    private void ApplyLiftVisual(float lift)
    {
        visualTransform.localPosition = Vector3.up * (lift * moveLiftHeight);
        visualTransform.localRotation = Quaternion.Euler(0f, 0f, tiltDirection * moveTiltAngle * lift);
        visualTransform.localScale = baseVisualScale * (1f + moveScaleBoost * lift);
        ApplyShadowDepth(lift);
    }

    private void ApplyShadowDepth(float depth)
    {
        shadowTransform.localPosition = baseShadowPosition + Vector3.down * (moveLiftHeight * depth * 0.35f);
        shadowTransform.localScale = baseShadowScale * (1f + shadowExpansion * depth);
        Color color = baseShadowColor;
        color.a *= Mathf.Lerp(1f, liftedShadowOpacity, depth) * winOpacity;
        shadowRenderer.color = color;
    }

    private void ResetDepthVisual()
    {
        visualTransform.localPosition = Vector3.zero;
        visualTransform.localRotation = Quaternion.identity;
        visualTransform.localScale = baseVisualScale;
        ApplyShadowDepth(0f);
    }

    private void ConfigureShadow(float cellSize)
    {
        Vector2 size = shadowRenderer.sprite.bounds.size;
        baseShadowScale = new Vector3(cellSize * shadowWidth / size.x, cellSize * shadowHeight / size.y, 1f);
        baseShadowPosition = Vector3.down * (cellSize * shadowVerticalOffset);
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
        return cellSize * fill / Mathf.Max(size.x, size.y);
    }

    private void FaceDirection(Vector2Int direction)
    {
        if (direction.x != 0) spriteRenderer.flipX = direction.x < 0;
    }

    private float TiltDirection(Vector2Int direction)
    {
        return direction.x != 0 ? -direction.x : direction.y;
    }
}
