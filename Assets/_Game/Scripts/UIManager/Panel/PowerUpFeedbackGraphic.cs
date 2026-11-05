using UnityEngine;
using UnityEngine.UI;

public sealed class PowerUpFeedbackGraphic : MaskableGraphic
{
    private const int SNOW_COUNT = 36;

    private struct Snowflake
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public float Rotation;
        public float Life;
    }

    private readonly Snowflake[] snow = new Snowflake[SNOW_COUNT];
    [SerializeField] private CanvasGroup frostGroup;
    [SerializeField] private CanvasGroup iceFrameGroup;
    [SerializeField] private RectTransform[] iceCorners;
    [SerializeField] private RectTransform freezeBarFill;
    private float freezeOpacity;
    private float freezeTarget;
    private bool snowInitialized;

    void Update()
    {
        float delta = Time.unscaledDeltaTime;
        float previousOpacity = freezeOpacity;
        freezeOpacity = Mathf.MoveTowards(freezeOpacity, freezeTarget, delta * 4f);
        UpdateFrostVisuals();
        if (freezeOpacity > 0.001f) UpdateSnow(delta);
        if (freezeOpacity > 0f || previousOpacity != freezeOpacity)
            SetVerticesDirty();
    }

    public void BeginFreeze()
    {
        InitializeSnow();
        freezeTarget = 1f;
        enabled = true;
        SetVerticesDirty();
    }

    public void SetFreezeProgress(float progress)
    {
        SetBarProgress(progress);
    }

    public void EndFreeze()
    {
        freezeTarget = 0f;
    }

    public void ResetFeedback()
    {
        freezeOpacity = 0f;
        freezeTarget = 0f;
        frostGroup.alpha = 0f;
        iceFrameGroup.alpha = 0f;
        SetBarProgress(1f);
        SetCornerScale(0.15f);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        if (freezeOpacity > 0.001f) DrawFreeze(vertexHelper);
    }

    private void DrawFreeze(VertexHelper mesh)
    {
        for (int i = 0; i < snow.Length; i++) DrawSnowflake(mesh, snow[i]);
    }

    private void UpdateFrostVisuals()
    {
        frostGroup.alpha = freezeOpacity;
        iceFrameGroup.alpha = freezeOpacity;
        float reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(freezeOpacity * 1.4f));
        float overshoot = Mathf.Sin(reveal * Mathf.PI) * 0.12f;
        SetCornerScale(Mathf.Lerp(0.15f, 1f, reveal) + overshoot);
    }

    private void SetCornerScale(float scale)
    {
        Vector3 cornerScale = Vector3.one * scale;
        for (int i = 0; i < iceCorners.Length; i++)
            iceCorners[i].localScale = cornerScale;
    }

    private void SetBarProgress(float progress)
    {
        Vector2 anchorMax = freezeBarFill.anchorMax;
        anchorMax.x = Mathf.Lerp(0.04f, 1f, Mathf.Clamp01(progress));
        freezeBarFill.anchorMax = anchorMax;
    }

    private void DrawSnowflake(VertexHelper mesh, Snowflake flake)
    {
        Color snowColor = new Color(0.88f, 1f, 1f, freezeOpacity * 0.9f);
        for (int branch = 0; branch < 6; branch++)
        {
            float angle = flake.Rotation + branch * 60f;
            Vector2 direction = Direction(angle);
            Vector2 tip = flake.Position + direction * flake.Size;
            Vector2 joint = flake.Position + direction * flake.Size * 0.58f;
            AddLine(mesh, flake.Position, tip, Mathf.Max(1.4f, flake.Size * 0.1f), snowColor);
            AddLine(mesh, joint, joint + Direction(angle - 35f) * flake.Size * 0.38f,
                1.1f, snowColor);
            AddLine(mesh, joint, joint + Direction(angle + 35f) * flake.Size * 0.38f,
                1.1f, snowColor);
        }
    }

    private void InitializeSnow()
    {
        if (snowInitialized) return;
        Rect rect = rectTransform.rect;
        for (int i = 0; i < snow.Length; i++) ResetSnow(i, rect);
        snowInitialized = true;
    }

    private void UpdateSnow(float delta)
    {
        Rect rect = rectTransform.rect;
        for (int i = 0; i < snow.Length; i++)
        {
            Snowflake flake = snow[i];
            flake.Position += flake.Velocity * delta;
            flake.Rotation += delta * 10f;
            flake.Life -= delta;
            snow[i] = flake;
            if (flake.Life <= 0f) ResetSnow(i, rect);
        }
    }

    private void ResetSnow(int index, Rect rect)
    {
        bool top = Random.value > 0.5f;
        int zone = Random.Range(0, 3);
        float x = GetIceEmissionX(rect, zone);
        float depth = zone == 1 ? Random.Range(45f, 95f) : Random.Range(100f, 190f);
        float verticalSpeed = Random.Range(30f, 55f) * (top ? -1f : 1f);
        snow[index] = new Snowflake
        {
            Position = new Vector2(x, top ? rect.yMax - depth : rect.yMin + depth),
            Velocity = new Vector2(Random.Range(-45f, 45f), verticalSpeed),
            Size = Random.Range(8f, 17f),
            Rotation = Random.Range(0f, 360f),
            Life = Random.Range(2.8f, 4.5f)
        };
    }

    private static float GetIceEmissionX(Rect rect, int zone)
    {
        float center = zone == 0 ? 0.12f : zone == 1 ? 0.5f : 0.88f;
        float normalizedX = center + Random.Range(-0.1f, 0.1f);
        return Mathf.Lerp(rect.xMin, rect.xMax, normalizedX);
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
        AddQuad(mesh, start - side, end + side, color, start + side, end - side);
    }

    private static void AddQuad(VertexHelper mesh, Vector2 min, Vector2 max, Color color)
    {
        AddQuad(mesh, min, max, color, new Vector2(min.x, max.y), new Vector2(max.x, min.y));
    }

    private static void AddQuad(VertexHelper mesh, Vector2 a, Vector2 c, Color color,
        Vector2 b, Vector2 d)
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
