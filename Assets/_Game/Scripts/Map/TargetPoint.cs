using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bobHeight = 0.035f;
    [SerializeField] private float bobSpeed = 2.6f;
    [Header("Rescue")]
    [SerializeField, Min(0.01f)] private float rescueDuration = 0.42f;
    [SerializeField, Min(0f)] private float rescueHopHeight = 0.45f;
    [SerializeField, Min(0f)] private float rescueHopScale = 0.35f;

    private Vector3 basePosition;
    private Vector3 baseScale;
    private float timeOffset;
    private float entranceFactor = 1f;
    private float rescueElapsed;
    private bool isRescuing;
    private Color baseColor;
    private System.Action<TargetPoint> rescueArrived;
    private Transform cachedTransform;
    private MaterialPropertyBlock idleBlock;
    public bool IsCollected { get; private set; }
    public Vector2Int Cell => cell;
    public Vector3 WorldPosition => cachedTransform.position;
    public Sprite DisplaySprite => spriteRenderer.sprite;
    public Color DisplayColor => spriteRenderer.color;

    void Awake()
    {
        cachedTransform = transform;
    }

    void Update()
    {
        if (isRescuing)
        {
            UpdateRescue();
            return;
        }
        float wave = Mathf.Sin(Time.time * bobSpeed + timeOffset);
        cachedTransform.localPosition = basePosition + Vector3.up * (wave * bobHeight);
        cachedTransform.localScale = baseScale * ((1f + wave * 0.045f) * entranceFactor);
    }

    public void ApplyEntranceScale(float factor)
    {
        entranceFactor = factor;
        cachedTransform.localScale = baseScale * factor;
    }

    public void Setup(Vector2Int setupCell, Vector3 localPosition, Sprite sprite, Color color)
    {
        cell = setupCell;
        cachedTransform = transform;
        basePosition = localPosition;
        timeOffset = (cell.x * 0.73f + cell.y * 1.31f) % Mathf.PI;
        cachedTransform.localPosition = basePosition;

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        idleBlock = new MaterialPropertyBlock();
        // Reuse the bob offset so each baby swims slightly out of step with the others.
        SpriteIdleWave.Bind(spriteRenderer, idleBlock, timeOffset * 2f);
        baseColor = color;
        spriteRenderer.sortingOrder = RenderOrder.TARGET_POINT;
        baseScale = cachedTransform.localScale;
        cachedTransform.localScale = baseScale;
        ResetTarget();
    }

    public void ConfigureRenderer(SpriteRenderer setupRenderer)
    {
        spriteRenderer = setupRenderer;
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        basePosition = localPosition;
        cachedTransform.localPosition = localPosition;
    }

    // The baby hops up, then dives into the mother standing on this cell.
    public void Collect(System.Action<TargetPoint> arrived)
    {
        if (IsCollected) return;
        IsCollected = true;
        SoundManager.Ins.PlaySound(GameConstants.COLLECT_SOUND);
        rescueArrived = arrived;
        rescueElapsed = 0f;
        isRescuing = true;
    }

    public void ApplyFade(float progress)
    {
        Color color = baseColor;
        color.a *= 1f - Mathf.SmoothStep(0f, 1f, progress);
        spriteRenderer.color = color;
    }

    private void UpdateRescue()
    {
        rescueElapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(rescueElapsed / rescueDuration);
        bool hopping = progress < 0.5f;
        float phase = hopping ? progress * 2f : (progress - 0.5f) * 2f;
        float height = hopping ? Mathf.Sin(phase * Mathf.PI * 0.5f) : 1f - phase * phase;
        float scale = hopping ? 1f + rescueHopScale * phase : (1f + rescueHopScale) * (1f - phase);
        cachedTransform.localPosition = basePosition + Vector3.up * (height * rescueHopHeight);
        cachedTransform.localScale = baseScale * scale;
        if (progress < 1f) return;
        isRescuing = false;
        gameObject.SetActive(false);
        rescueArrived(this);
    }

    public void ResetTarget()
    {
        IsCollected = false;
        isRescuing = false;
        spriteRenderer.color = baseColor;
        gameObject.SetActive(true);
        cachedTransform.localPosition = basePosition;
        cachedTransform.localScale = baseScale;
    }

}
