using UnityEngine;

// A single pooled bubble: either drifts outward, or spirals into a centre (swirl),
// then shrinks and fades away.
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
        gameObject.SetActive(true);
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
        float grow = Mathf.Min(1f, progress * 5f);
        cachedTransform.localScale = Vector3.one * (size * grow * (1f - progress * 0.4f));
        Color faded = color;
        faded.a *= 1f - progress * progress;
        spriteRenderer.color = faded;
    }

    private Vector3 Offset(float progress)
    {
        if (!isSwirling)
        {
            float travel = 1f - (1f - progress) * (1f - progress);
            return velocity * (travel * lifetime);
        }
        float angle = swirlAngle + swirlAngularSpeed * age;
        float radius = swirlRadius * (1f - progress * 0.8f);
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
    }
}
