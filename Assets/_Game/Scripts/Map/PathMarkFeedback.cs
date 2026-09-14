using UnityEngine;

public class PathMarkFeedback : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Vector3 baseScale;
    private float timer;

    public void Setup(SpriteRenderer renderer, Color color, Vector3 scale)
    {
        spriteRenderer = renderer;
        baseColor = color;
        baseScale = scale;
        transform.localScale = baseScale * 0.25f;
    }

    private void Update()
    {
        timer = Mathf.Min(1f, timer + Time.deltaTime * 8f);
        float ease = timer * timer * (3f - 2f * timer);
        transform.localScale = Vector3.Lerp(baseScale * 0.25f, baseScale, ease);
        if (spriteRenderer == null) return;
        Color color = baseColor;
        color.a = baseColor.a * ease;
        spriteRenderer.color = color;
    }
}
