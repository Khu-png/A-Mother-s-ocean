using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color closedColor;
    [SerializeField] private Color openColor;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [Header("Open Feedback")]
    [SerializeField, Range(0f, 0.2f)] private float openBreatheAmount = 0.06f;
    [SerializeField, Min(0f)] private float openBreatheSpeed = 4f;
    private readonly ScalePulse pulse = new ScalePulse();
    private Color winFadeColor;
    private Vector3 baseScale;
    private float entranceFactor = 1f;

    public bool IsOpen { get; private set; }
    public Vector2Int Cell => cell;
    public Vector3 WorldPosition => transform.position;
    public Sprite DisplaySprite => spriteRenderer.sprite;
    public Color DisplayColor => spriteRenderer.color;

    public void Setup(Vector2Int setupCell, Vector3 localPosition, Color setupClosedColor, Color setupOpenColor)
    {
        cell = setupCell;
        closedColor = setupClosedColor;
        openColor = setupOpenColor;
        transform.localPosition = localPosition;
        baseScale = transform.localScale;

        spriteRenderer.sprite = closedSprite;
        spriteRenderer.sortingOrder = 2;
        SetOpen(false);
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }

    void Update()
    {
        pulse.Advance(Time.deltaTime);
        float breathe = IsOpen ? 1f + Mathf.Sin(Time.time * openBreatheSpeed) * openBreatheAmount : 1f;
        transform.localScale = baseScale * (entranceFactor * pulse.Factor * breathe);
    }

    public void ApplyEntranceScale(float factor)
    {
        entranceFactor = factor;
    }

    public void PlayPulse(float duration, float amount)
    {
        pulse.Play(duration, amount);
    }

    public void SetOpen(bool isOpen)
    {
        IsOpen = isOpen;
        spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
        spriteRenderer.color = isOpen ? openColor : closedColor;
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

    public void ConfigureVisuals(SpriteRenderer setupRenderer, Sprite setupClosedSprite, Sprite setupOpenSprite)
    {
        spriteRenderer = setupRenderer;
        closedSprite = setupClosedSprite;
        openSprite = setupOpenSprite;
    }
}
