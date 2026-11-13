using UnityEngine;

public class PathMarkFeedback : MonoBehaviour
{
    private const float GROW_SPEED = 8f;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Vector3 baseScale;
    private float timer;
    private float winOpacity = 1f;
    private bool isRetracting;
    private Transform cachedTransform;

    void Awake()
    {
        cachedTransform = transform;
    }

    void Update()
    {
        float change = Time.deltaTime * GROW_SPEED;
        timer = isRetracting ? Mathf.Max(0f, timer - change) : Mathf.Min(1f, timer + change);
        if (isRetracting && timer <= 0f) gameObject.SetActive(false);
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

    // Shrinks the mark away when the player backtracks; Restart cancels it if reused.
    public void Retract()
    {
        isRetracting = true;
    }

    public void Restart(Vector3 localPosition)
    {
        isRetracting = false;
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
