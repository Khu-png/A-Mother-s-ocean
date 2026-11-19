using UnityEngine;

public enum IceChange
{
    None,
    Cracked,
    Melted
}

// Ice sealing a baby shark. It shows how many more babies must be rescued before it
// melts, and stays shut while that number is above zero.
public class IceCover : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Sprite for each counter value; index 0 is the counter 1.")]
    [SerializeField] private Sprite[] counterSprites;
    [Tooltip("Flake shapes thrown out when the ice cracks or melts.")]
    [SerializeField] private Sprite[] shardSprites;
    [SerializeField, Min(0.01f)] private float punchDuration = 0.22f;
    [SerializeField, Range(0f, 0.5f)] private float punchStrength = 0.14f;
    [SerializeField, Range(0f, 25f)] private float shakeAngle = 3f;
    [SerializeField, Range(0f, 0.3f)] private float kickDistance = 0.09f;
    [SerializeField, Min(0.01f)] private float meltDuration = 0.32f;
    [SerializeField, Min(0f)] private float meltGrowth = 0.35f;

    private Transform cachedTransform;
    private Vector3 basePosition;
    private Vector3 kickDirection;
    private Vector2Int cell;
    private float baseScale;
    private float entranceFactor = 1f;
    private float animationTime;
    private float fade = 1f;
    private bool isMelting;
    private int remaining;

    public Sprite[] ShardSprites => shardSprites;
    public int Threshold { get; private set; }
    public bool IsFrozen { get; private set; }
    public Vector2Int Cell => cell;

    void Update()
    {
        if (animationTime <= 0f) return;
        animationTime = Mathf.Max(0f, animationTime - Time.deltaTime);
        if (isMelting) UpdateMelt();
        else UpdateShake();
    }

    public void Setup(Vector2Int setupCell, Vector3 localPosition, float cellSize, int threshold)
    {
        cell = setupCell;
        cachedTransform = transform;
        basePosition = localPosition;
        cachedTransform.localPosition = localPosition;
        baseScale = cellSize;
        Threshold = threshold;
        spriteRenderer.sortingOrder = RenderOrder.ICE_COVER;
        Freeze(threshold);
        ApplyVisual(1f, 1f);
    }

    // others = babies already rescued, not counting the one inside this ice.
    public IceChange Refresh(int others)
    {
        int left = Threshold - others;
        if (left <= 0)
        {
            if (!IsFrozen) return IceChange.None;
            Melt();
            return IceChange.Melted;
        }
        if (IsFrozen && left == remaining) return IceChange.None;
        bool wasFrozen = IsFrozen;
        Freeze(left);
        StartAnimation(punchDuration, false);
        return wasFrozen ? IceChange.Cracked : IceChange.None;
    }

    // The player bumped into the ice.
    public void PlayDenied()
    {
        StartAnimation(punchDuration, false);
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        basePosition = localPosition;
        cachedTransform.localPosition = localPosition;
    }

    public void ApplyEntranceScale(float factor)
    {
        entranceFactor = factor;
        ApplyVisual(1f, fade);
    }

    public void ApplyFade(float progress)
    {
        fade = 1f - Mathf.SmoothStep(0f, 1f, progress);
        ApplyVisual(1f, fade);
    }

    private void Freeze(int left)
    {
        IsFrozen = true;
        remaining = left;
        cachedTransform.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
        spriteRenderer.sprite = counterSprites[left - 1];
        ApplyVisual(1f, 1f);
    }

    private void Melt()
    {
        IsFrozen = false;
        StartAnimation(meltDuration, true);
    }

    private void StartAnimation(float duration, bool melting)
    {
        isMelting = melting;
        animationTime = duration;
        float angle = Random.value * Mathf.PI * 2f;
        kickDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
    }

    // The whole block takes a hit: it is shoved sideways, springs back past its place
    // once and settles, squashing as it goes - a heavy thud rather than a buzz.
    private void UpdateShake()
    {
        float progress = 1f - animationTime / punchDuration;
        float spring = Mathf.Cos(progress * Mathf.PI * 1.5f) * (1f - progress) * (1f - progress);
        cachedTransform.localPosition = basePosition + kickDirection * (spring * kickDistance * baseScale);
        cachedTransform.localRotation = Quaternion.Euler(0f, 0f, spring * shakeAngle * Mathf.Sign(kickDirection.x));
        ApplyVisual(1f - spring * punchStrength, 1f);
        if (animationTime > 0f) return;
        cachedTransform.localPosition = basePosition;
        cachedTransform.localRotation = Quaternion.identity;
    }

    private void UpdateMelt()
    {
        float progress = 1f - animationTime / meltDuration;
        ApplyVisual(1f + progress * meltGrowth, 1f - progress);
        if (animationTime <= 0f) gameObject.SetActive(false);
    }

    private void ApplyVisual(float scale, float alpha)
    {
        cachedTransform.localScale = Vector3.one * (baseScale * entranceFactor * scale);
        Color color = spriteRenderer.color;
        color.a = alpha * fade;
        spriteRenderer.color = color;
    }
}
