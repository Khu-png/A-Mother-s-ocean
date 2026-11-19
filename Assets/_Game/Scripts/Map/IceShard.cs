using UnityEngine;

// One pooled ice flake: flies out, tumbles, falls under gravity and fades.
public sealed class IceShard : MonoBehaviour
{
    private const float FLIP_SPEED = 9f;
    private SpriteRenderer spriteRenderer;
    private Transform cachedTransform;
    private Vector3 startPosition;
    private Vector3 velocity;
    private float gravity;
    private float size;
    private float lifetime;
    private float spin;
    private float flipOffset;
    private float age;

    public void Setup(SpriteRenderer setupRenderer)
    {
        spriteRenderer = setupRenderer;
        cachedTransform = transform;
    }

    public void Launch(Vector3 localPosition, Vector3 launchVelocity, float shardSize,
        float shardLifetime, float fallAcceleration, float spinSpeed)
    {
        startPosition = localPosition;
        velocity = launchVelocity;
        size = shardSize;
        lifetime = shardLifetime;
        gravity = fallAcceleration;
        spin = spinSpeed;
        flipOffset = Random.value * Mathf.PI * 2f;
        age = 0f;
        cachedTransform.localRotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
        gameObject.SetActive(true);
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
        cachedTransform.localPosition = startPosition + velocity * age
            + Vector3.down * (0.5f * gravity * age * age);
        cachedTransform.Rotate(0f, 0f, spin * Time.deltaTime);
        // Turning edge-on as it tumbles makes the flake read as a thin pane of ice.
        float flip = Mathf.Lerp(0.25f, 1f, Mathf.Abs(Mathf.Cos(age * FLIP_SPEED + flipOffset)));
        float pop = Mathf.Min(1f, progress * 8f);
        cachedTransform.localScale = new Vector3(flip, 1f, 1f) * (size * pop * (1f - progress * 0.3f));
        Color color = Color.white;
        color.a = 1f - progress * progress * progress;
        spriteRenderer.color = color;
    }
}
