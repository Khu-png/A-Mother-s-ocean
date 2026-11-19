using UnityEngine;

public enum FxStyle
{
    Bubble,
    Sparkle,
    Ring
}

// A single pooled effect piece. Bubbles drift outward, rise and wobble, then pop;
// sparkles twinkle and spin; a ring swells and fades; a swirl spirals into a centre.
public sealed class BubbleParticle : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Transform cachedTransform;
    private Vector3 startPosition;
    private Vector3 velocity;
    private Color color;
    private float size;
    private float lifetime;
    private float age;
    private bool isSwirling;
    private float swirlAngle;
    private float swirlRadius;
    private float swirlAngularSpeed;
    private FxStyle style;
    private float lift;
    private float wobble;
    private float wobblePhase;
    private float spin;

    public void Setup(SpriteRenderer setupRenderer)
    {
        spriteRenderer = setupRenderer;
        cachedTransform = transform;
    }

    public void Launch(Vector3 localPosition, Vector3 launchVelocity, float bubbleSize,
        float bubbleLifetime, Color bubbleColor)
    {
        startPosition = localPosition;
        velocity = launchVelocity;
        size = bubbleSize;
        lifetime = bubbleLifetime;
        color = bubbleColor;
        age = 0f;
        isSwirling = false;
        style = FxStyle.Bubble;
        lift = 0f;
        wobble = 0f;
        wobblePhase = Random.value * Mathf.PI * 2f;
        spin = 0f;
        gameObject.SetActive(true);
        Apply(0f);
    }

    // Picks the look and the extra motion for a piece that was just launched.
    public void Style(Sprite sprite, FxStyle fxStyle, float liftAcceleration, float wobbleAmount,
        float spinSpeed)
    {
        spriteRenderer.sprite = sprite;
        style = fxStyle;
        lift = liftAcceleration;
        wobble = wobbleAmount;
        spin = spinSpeed;
        cachedTransform.localRotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
        Apply(0f);
    }

    // angularSpeed is in radians per second; its sign sets the spin direction.
    public void LaunchSwirl(Vector3 center, float angle, float radius, float angularSpeed,
        float bubbleSize, float bubbleLifetime, Color bubbleColor)
    {
        Launch(center, Vector3.zero, bubbleSize, bubbleLifetime, bubbleColor);
        isSwirling = true;
        swirlAngle = angle;
        swirlRadius = radius;
        swirlAngularSpeed = angularSpeed;
        Apply(0f);
    }

    void Update()
    {
        age += Time.deltaTime;
        float progress = Mathf.Clamp01(age / lifetime);
        Apply(progress);
        if (progress >= 1f) gameObject.SetActive(false);
    }

    private void Apply(float progress)
    {
        cachedTransform.localPosition = startPosition + Offset(progress);
        if (spin != 0f) cachedTransform.Rotate(0f, 0f, spin * Time.deltaTime);
        cachedTransform.localScale = Vector3.one * ScaleAt(progress);
        Color faded = color;
        faded.a *= AlphaAt(progress);
        spriteRenderer.color = faded;
    }

    private float ScaleAt(float progress)
    {
        if (style == FxStyle.Ring)
            return size * (0.3f + 0.7f * (1f - (1f - progress) * (1f - progress)));
        if (style == FxStyle.Sparkle)
            return size * Mathf.Sin(progress * Mathf.PI) * (1f + 0.25f * Mathf.Sin(age * 28f + wobblePhase));
        float grow = Mathf.Min(1f, progress * 5f);
        float pop = progress > 0.85f ? 1f + (progress - 0.85f) * 2.2f : 1f - progress * 0.3f;
        return size * grow * pop;
    }

    private float AlphaAt(float progress)
    {
        if (style == FxStyle.Ring) return (1f - progress) * (1f - progress);
        if (style == FxStyle.Sparkle) return 1f - progress * progress * 0.6f;
        return progress > 0.85f ? (1f - progress) / 0.15f : 1f - progress * progress * 0.3f;
    }

    private Vector3 Offset(float progress)
    {
        if (isSwirling)
        {
            float angle = swirlAngle + swirlAngularSpeed * age;
            float radius = swirlRadius * (1f - progress * 0.8f);
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }
        float travel = 1f - (1f - progress) * (1f - progress);
        Vector3 drift = velocity * (travel * lifetime);
        drift += Vector3.up * (0.5f * lift * age * age);
        drift += Vector3.right * (Mathf.Sin(age * 9f + wobblePhase) * wobble * Mathf.Min(1f, age * 4f));
        return drift;
    }
}
