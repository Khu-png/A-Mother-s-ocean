using UnityEngine;

// Pooled flakes of ice that fly out of a cracking or melting cover, then fall and fade.
public sealed class IceShardBurst
{
    private readonly IceShard[] SHARDS;
    private readonly float CELL_SIZE;
    private int nextIndex;

    public IceShardBurst(Transform parent, float cellSize, int capacity, Sprite[] sprites)
    {
        Transform root = new GameObject("IceShards").transform;
        root.SetParent(parent, false);
        CELL_SIZE = cellSize;
        SHARDS = new IceShard[capacity];
        for (int index = 0; index < capacity; index++) SHARDS[index] = CreateShard(root, sprites[index % sprites.Length]);
    }

    // speed and size are in cells.
    public void Play(Vector3 localPosition, int count, float speed, float size)
    {
        for (int index = 0; index < count; index++)
        {
            float angle = (index + Random.value) / count * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) + 0.5f, 0f);
            NextShard().Launch(localPosition + Vector3.back * 0.05f,
                direction * (speed * CELL_SIZE * Random.Range(0.5f, 1f)),
                size * CELL_SIZE * (Random.value < 0.3f ? Random.Range(1.3f, 1.8f) : Random.Range(0.5f, 1.1f)), Random.Range(0.45f, 0.75f),
                CELL_SIZE * 3.2f, Random.Range(-540f, 540f));
        }
    }

    private IceShard NextShard()
    {
        IceShard shard = SHARDS[nextIndex];
        nextIndex = (nextIndex + 1) % SHARDS.Length;
        return shard;
    }

    private static IceShard CreateShard(Transform root, Sprite sprite)
    {
        GameObject view = new GameObject("IceShard");
        view.transform.SetParent(root, false);
        SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = RenderOrder.EFFECT_BUBBLES + 1;
        IceShard shard = view.AddComponent<IceShard>();
        shard.Setup(renderer);
        view.SetActive(false);
        return shard;
    }
}
