using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bobHeight = 0.035f;
    [SerializeField] private float bobSpeed = 2.6f;
    [Header("Rescue")]
    [SerializeField, Min(0.01f)] private float hopDuration = 0.22f;
    [SerializeField, Min(0f)] private float hopHeight = 0.4f;
    [SerializeField, Range(0f, 0.6f)] private float hopStretch = 0.2f;
    [SerializeField, Min(0.01f)] private float flightDuration = 0.55f;
    [SerializeField, Min(0f)] private float flightArcHeight = 0.4f;
    [SerializeField, Range(0.1f, 1f)] private float arrivalScale = 0.45f;
    [SerializeField, Min(0.01f)] private float trailInterval = 0.04f;

    private const float HEADING_SPAN = 0.05f;
    private const float TURN_IN_SPEED = 6f;
    private Vector3 basePosition;
    private Vector3 baseScale;
    private float timeOffset;
    private float entranceFactor = 1f;
    private float rescueElapsed;
    private Vector3 hopStart;
    private Vector3 flightStart;
    private Vector3 flightEnd;
    private float trailTimer;
    private System.Action<Vector3> trailEmitter;
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

    // The baby first lifts up a little, then launches: it accelerates along an arc into
    // the target panel at destination, leaving a bubble trail (world positions).
    // Tapering at the top of the lift is what makes the launch read as a spring.
    public void Collect(System.Action<TargetPoint> arrived, Vector3 destination,
        System.Action<Vector3> trail)
    {
        if (IsCollected) return;
        IsCollected = true;
        SoundManager.Ins.PlaySound(GameConstants.COLLECT_SOUND);
        rescueArrived = arrived;
        trailEmitter = trail;
        trailTimer = 0f;
        hopStart = cachedTransform.position;
        flightStart = hopStart + Vector3.up * hopHeight;
        flightEnd = destination;
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
        if (rescueElapsed < hopDuration)
        {
            UpdateHop(rescueElapsed / hopDuration);
            return;
        }
        if (!UpdateFlight((rescueElapsed - hopDuration) / flightDuration)) return;
        isRescuing = false;
        gameObject.SetActive(false);
        rescueArrived(this);
    }

    // Eases out so the baby rises quickly and slows at the top, stretching a little as it goes.
    private void UpdateHop(float progress)
    {
        float lift = Mathf.Sin(progress * Mathf.PI * 0.5f);
        float stretch = Mathf.Sin(progress * Mathf.PI) * hopStretch;
        cachedTransform.position = hopStart + Vector3.up * (lift * hopHeight);
        cachedTransform.localScale = Vector3.Scale(baseScale,
            new Vector3(1f - stretch * 0.5f, 1f + stretch, 1f));
    }

    private Vector3 FlightPoint(float travel)
    {
        return Vector3.Lerp(flightStart, flightEnd, travel)
            + Vector3.up * (Mathf.Sin(travel * Mathf.PI) * flightArcHeight);
    }

    // Speeds up from the lift and slows down as it enters the panel; the baby points
    // along its path the whole way. False until it arrives.
    private bool UpdateFlight(float progress)
    {
        float clamped = Mathf.Clamp01(progress);
        float travel = Mathf.SmoothStep(0f, 1f, clamped);
        cachedTransform.position = FlightPoint(travel);
        FaceHeading(FlightPoint(Mathf.Min(1f, travel + HEADING_SPAN))
            - FlightPoint(Mathf.Max(0f, travel - HEADING_SPAN)), clamped);
        cachedTransform.localScale = baseScale * Mathf.Lerp(1f, arrivalScale, travel);
        trailTimer -= Time.deltaTime;
        if (trailTimer <= 0f)
        {
            trailTimer = trailInterval;
            trailEmitter(cachedTransform.position);
        }
        return progress >= 1f;
    }

    // Mirrors the sprite instead of turning it upside down when swimming left, and eases
    // from upright into the heading over the first part of the flight.
    private void FaceHeading(Vector3 heading, float progress)
    {
        bool left = heading.x < 0f;
        spriteRenderer.flipX = left;
        float angle = Mathf.Atan2(left ? -heading.y : heading.y, Mathf.Abs(heading.x)) * Mathf.Rad2Deg;
        cachedTransform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Clamp01(progress * TURN_IN_SPEED));
    }

    public void ResetTarget()
    {
        IsCollected = false;
        isRescuing = false;
        spriteRenderer.flipX = false;
        spriteRenderer.color = baseColor;
        gameObject.SetActive(true);
        cachedTransform.localPosition = basePosition;
        cachedTransform.localRotation = Quaternion.identity;
        cachedTransform.localScale = baseScale;
    }

}
