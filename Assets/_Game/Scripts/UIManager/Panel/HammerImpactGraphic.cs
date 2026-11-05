using UnityEngine;
using UnityEngine.UI;

public sealed class HammerImpactGraphic : MaskableGraphic
{
    private const float IMPACT_DURATION = 0.58f;
    private const int RING_SEGMENTS = 24;
    private static readonly Vector2[] SHARD_DIRECTIONS =
    {
        new(-0.88f, 0.48f), new(-0.45f, 0.9f), new(0.08f, 1f),
        new(0.58f, 0.82f), new(0.96f, 0.28f), new(0.86f, -0.52f),
        new(0.35f, -0.94f), new(-0.2f, -0.98f), new(-0.72f, -0.7f),
        new(-1f, -0.08f)
    };
    private static readonly float[] SHARD_SIZES =
    {
        17f, 11f, 15f, 13f, 18f, 12f, 16f, 11f, 15f, 10f
    };

    private float remaining;
    private Vector2 impactPosition;

    void Update()
    {
        if (remaining <= 0f) return;
        remaining = Mathf.Max(0f, remaining - Time.unscaledDeltaTime);
        SetVerticesDirty();
    }

    public void Play(Vector2 localPosition)
    {
        impactPosition = localPosition;
        remaining = IMPACT_DURATION;
        SetVerticesDirty();
    }

    public void ResetImpact()
    {
        remaining = 0f;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (remaining <= 0f) return;
        float progress = 1f - remaining / IMPACT_DURATION;
        DrawFlashRing(mesh, progress);
        DrawShards(mesh, progress);
    }

    private void DrawFlashRing(VertexHelper mesh, float progress)
    {
        float radius = Mathf.Lerp(20f, 145f, Mathf.SmoothStep(0f, 1f, progress));
        Color color = new(1f, 0.9f, 0.3f, 1f - progress);
        Vector2 previous = impactPosition + Vector2.right * radius;
        for (int i = 1; i <= RING_SEGMENTS; i++)
        {
            Vector2 next = impactPosition + Direction(i * 360f / RING_SEGMENTS) * radius;
            AddLine(mesh, previous, next, Mathf.Lerp(12f, 2f, progress), color);
            previous = next;
        }
    }

    private void DrawShards(VertexHelper mesh, float progress)
    {
        float travel = Mathf.SmoothStep(0f, 1f, progress);
        float alpha = 1f - Mathf.InverseLerp(0.68f, 1f, progress);
        for (int i = 0; i < SHARD_DIRECTIONS.Length; i++)
        {
            Vector2 direction = SHARD_DIRECTIONS[i].normalized;
            Vector2 fall = Vector2.down * progress * progress * 70f;
            Vector2 position = impactPosition + direction * travel * 145f + fall;
            float rotation = i * 37f + progress * (210f + i * 9f);
            Color color = i % 2 == 0
                ? new Color(0.72f, 0.78f, 0.9f, alpha)
                : new Color(0.35f, 0.43f, 0.58f, alpha);
            AddShard(mesh, position, SHARD_SIZES[i], rotation, color);
        }
    }

    private static void AddShard(VertexHelper mesh, Vector2 center,
        float size, float rotation, Color color)
    {
        Vector2 right = Direction(rotation) * size;
        Vector2 up = Direction(rotation + 90f) * size * 0.62f;
        AddQuad(mesh, center - right - up * 0.45f, center - right * 0.35f + up,
            center + right + up * 0.25f, center + right * 0.55f - up, color);
    }

    private static Vector2 Direction(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private static void AddLine(VertexHelper mesh, Vector2 start, Vector2 end,
        float width, Color color)
    {
        Vector2 side = new Vector2(-(end - start).y, (end - start).x).normalized * width * 0.5f;
        AddQuad(mesh, start - side, start + side, end + side, end - side, color);
    }

    private static void AddQuad(VertexHelper mesh, Vector2 a, Vector2 b,
        Vector2 c, Vector2 d, Color color)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(a, color, Vector2.zero);
        mesh.AddVert(b, color, Vector2.up);
        mesh.AddVert(c, color, Vector2.one);
        mesh.AddVert(d, color, Vector2.right);
        mesh.AddTriangle(start, start + 1, start + 2);
        mesh.AddTriangle(start, start + 2, start + 3);
    }
}
