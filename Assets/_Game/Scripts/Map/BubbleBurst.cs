using UnityEngine;

// Prewarmed ring buffer of bubbles; reusing the oldest bubble keeps bursts allocation-free.
public sealed class BubbleBurst
{
    private readonly BubbleParticle[] BUBBLES;
    private readonly float CELL_SIZE;
    private int nextIndex;

    public BubbleBurst(Transform parent, float cellSize, int capacity)
    {
        Transform root = new GameObject("Bubbles").transform;
        root.SetParent(parent, false);
        CELL_SIZE = cellSize;
        BUBBLES = new BubbleParticle[capacity];
        for (int index = 0; index < capacity; index++) BUBBLES[index] = CreateBubble(root);
    }

    // spread is measured in cells per second; size in cells.
    public void Play(Vector3 localPosition, int count, float spread, float size, Color color)
    {
        for (int index = 0; index < count; index++)
        {
            float angle = (index + Random.value * 0.6f) / count * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) + 0.6f, 0f);
            float speed = spread * CELL_SIZE * Random.Range(0.6f, 1f);
            float bubbleSize = size * CELL_SIZE * Random.Range(0.6f, 1.2f);
            float lifetime = Random.Range(0.45f, 0.7f);
            NextBubble().Launch(localPosition + Vector3.back * 0.05f, direction * speed,
                bubbleSize, lifetime, color);
        }
    }

    // Bubbles orbit the centre while being pulled inward, like a small whirlpool.
    public void PlaySwirl(Vector3 localPosition, int count, float radius, float turnsPerSecond,
        float size, Color color)
    {
        float angularSpeed = turnsPerSecond * Mathf.PI * 2f;
        for (int index = 0; index < count; index++)
        {
            float angle = (float)index / count * Mathf.PI * 2f;
            float bubbleRadius = radius * CELL_SIZE * Random.Range(0.75f, 1.1f);
            float bubbleSize = size * CELL_SIZE * Random.Range(0.6f, 1.1f);
            float lifetime = Random.Range(0.5f, 0.65f);
            NextBubble().LaunchSwirl(localPosition + Vector3.back * 0.05f, angle,
                bubbleRadius, angularSpeed, bubbleSize, lifetime, color);
        }
    }

    private BubbleParticle NextBubble()
    {
        BubbleParticle bubble = BUBBLES[nextIndex];
        nextIndex = (nextIndex + 1) % BUBBLES.Length;
        return bubble;
    }

    private static BubbleParticle CreateBubble(Transform root)
    {
        GameObject view = new GameObject("Bubble");
        view.transform.SetParent(root, false);
        SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
        renderer.sprite = GridVisual.CircleSprite();
        renderer.sortingOrder = RenderOrder.EFFECT_BUBBLES;
        BubbleParticle bubble = view.AddComponent<BubbleParticle>();
        bubble.Setup(renderer);
        view.SetActive(false);
        return bubble;
    }
}
