using UnityEngine;

public class PathMarkFeedback : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Vector3 baseScale;
    private float timer;
    private float winOpacity = 1f;
    private Transform cachedTransform;

    void Awake()
    {
        cachedTransform = transform;
    }

    void Update()
    {
        timer = Mathf.Min(1f, timer + Time.deltaTime * 8f);
        float ease = timer * timer * (3f - 2f * timer);
        cachedTransform.localScale = Vector3.Lerp(baseScale * 0.25f, baseScale, ease);
        if (spriteRenderer == null) return;
        Color color = baseColor;
        color.a = baseColor.a * ease * winOpacity;
        spriteRenderer.color = color;
    }

    public void Setup(SpriteRenderer renderer, Color color, Vector3 scale)
    {
        spriteRenderer = renderer;
        baseColor = color;
        baseScale = scale;
        winOpacity = 1f;
        cachedTransform = transform;
        cachedTransform.localScale = baseScale * 0.25f;
    }

    public void Restart(Vector3 localPosition)
    {
        timer = 0f;
        cachedTransform.localPosition = localPosition;
        cachedTransform.localScale = baseScale * 0.25f;
        Color color = baseColor;
        color.a = 0f;
        spriteRenderer.color = color;
    }

    public void ApplyWinFade(float progress)
    {
        winOpacity = 1f - Mathf.SmoothStep(0f, 1f, progress);
    }
}
