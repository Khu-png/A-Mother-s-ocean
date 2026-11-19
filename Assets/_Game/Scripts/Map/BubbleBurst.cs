using UnityEngine;

// Sprites for the pooled water effects, wired in the Inspector.
[System.Serializable]
public sealed class EffectSprites
{
    [SerializeField] private Sprite bubble;
    [SerializeField] private Sprite sparkle;
    [SerializeField] private Sprite ring;

    public Sprite Bubble => bubble;
    public Sprite Sparkle => sparkle;
    public Sprite Ring => ring;
}

// Prewarmed ring buffer of effect pieces; reusing the oldest one keeps bursts allocation-free.
public sealed class BubbleBurst
{
    private readonly BubbleParticle[] BUBBLES;
    private readonly EffectSprites SPRITES;
    private readonly float CELL_SIZE;
    private int nextIndex;

    public BubbleBurst(Transform parent, float cellSize, int capacity, EffectSprites sprites)
    {
        Transform root = new GameObject("Bubbles").transform;
        root.SetParent(parent, false);
        CELL_SIZE = cellSize;
        SPRITES = sprites;
        BUBBLES = new BubbleParticle[capacity];
        for (int index = 0; index < capacity; index++) BUBBLES[index] = CreateBubble(root);
    }

    // spread is measured in cells per second; size in cells. Bubbles rise a little and wobble.
    public void Play(Vector3 localPosition, int count, float spread, float size, Color color)
    {
        for (int index = 0; index < count; index++)
        {
            float angle = (index + Random.value * 0.6f) / count * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) + 0.6f, 0f);
            float speed = spread * CELL_SIZE * Random.Range(0.5f, 1f);
            float bubbleSize = size * CELL_SIZE * Random.Range(0.5f, 1.5f);
            BubbleParticle bubble = NextBubble();
            bubble.Launch(localPosition + Vector3.back * 0.05f, direction * speed,
                bubbleSize, Random.Range(0.5f, 0.85f), color);
            bubble.Style(SPRITES.Bubble, FxStyle.Bubble, CELL_SIZE * Random.Range(0.4f, 1f),
                CELL_SIZE * Random.Range(0.02f, 0.06f), 0f);
        }
    }

    // Glints that twinkle outward, for the moment something is rescued.
    public void PlaySparkles(Vector3 localPosition, int count, float spread, float size)
    {
        for (int index = 0; index < count; index++)
        {
            float angle = (index + Random.value) / count * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            BubbleParticle sparkle = NextBubble();
            sparkle.Launch(localPosition + Vector3.back * 0.06f,
                direction * (spread * CELL_SIZE * Random.Range(0.5f, 1f)),
                size * CELL_SIZE * Random.Range(0.6f, 1.4f), Random.Range(0.4f, 0.65f), Color.white);
            sparkle.Style(SPRITES.Sparkle, FxStyle.Sparkle, 0f, 0f, Random.Range(-90f, 90f));
        }
    }

    // One soft ring that swells outward from the spot.
    public void PlayRing(Vector3 localPosition, float size, Color color)
    {
        BubbleParticle ring = NextBubble();
        ring.Launch(localPosition + Vector3.back * 0.04f, Vector3.zero, size * CELL_SIZE, 0.45f, color);
        ring.Style(SPRITES.Ring, FxStyle.Ring, 0f, 0f, 0f);
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
            BubbleParticle bubble = NextBubble();
            bubble.LaunchSwirl(localPosition + Vector3.back * 0.05f, angle,
                bubbleRadius, angularSpeed, bubbleSize, Random.Range(0.5f, 0.65f), color);
            bubble.Style(SPRITES.Bubble, FxStyle.Bubble, 0f, 0f, 0f);
        }
    }

    private BubbleParticle NextBubble()
    {
        BubbleParticle bubble = BUBBLES[nextIndex];
        nextIndex = (nextIndex + 1) % BUBBLES.Length;
        return bubble;
    }

    private BubbleParticle CreateBubble(Transform root)
    {
        GameObject view = new GameObject("Bubble");
        view.transform.SetParent(root, false);
        SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
        renderer.sprite = SPRITES.Bubble;
        renderer.sortingOrder = RenderOrder.EFFECT_BUBBLES;
        BubbleParticle bubble = view.AddComponent<BubbleParticle>();
        bubble.Setup(renderer);
        view.SetActive(false);
        return bubble;
    }
}
