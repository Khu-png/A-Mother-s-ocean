using UnityEngine;

// Background bubbles that drift up through the camera view during play: behind the board,
// in front of the ocean backdrop. A fixed pool is created once and recycled from the bottom.
// Sizes and speeds are fractions of the visible height so every screen looks the same.
public sealed class AmbientBubbles : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Sprite bubbleSprite;
    [SerializeField, Range(1, 40)] private int count = 14;
    [SerializeField] private float planeZ = 0f;
    [SerializeField] private Vector2 sizeRange = new Vector2(0.015f, 0.04f);
    [SerializeField] private Vector2 speedRange = new Vector2(0.035f, 0.08f);
    [SerializeField, Min(0f)] private float wobble = 0.012f;
    [SerializeField] private Vector2 alphaRange = new Vector2(0.25f, 0.6f);
    private Transform[] bubbles;
    private SpriteRenderer[] renderers;
    private float[] speeds;
    private float[] baseX;
    private float[] phases;
    private float[] alphas;
    private float[] heights;

    void Awake()
    {
        bubbles = new Transform[count];
        renderers = new SpriteRenderer[count];
        speeds = new float[count];
        baseX = new float[count];
        phases = new float[count];
        alphas = new float[count];
        heights = new float[count];
        // Bounded pool created once; bubbles are recycled instead of spawned during play.
        for (int i = 0; i < count; i++)
        {
            CreateBubble(i);
            Respawn(i, Random.value);
        }
    }

    void Update()
    {
        Vector3 bottomLeft = ViewportToPlane(0f, 0f);
        Vector3 topRight = ViewportToPlane(1f, 1f);
        float viewHeight = topRight.y - bottomLeft.y;
        float time = Time.unscaledTime;
        for (int i = 0; i < count; i++)
        {
            heights[i] += speeds[i] * Time.unscaledDeltaTime;
            if (heights[i] >= 1f) Respawn(i, 0f);
            float x = Mathf.Lerp(bottomLeft.x, topRight.x, baseX[i])
                + Mathf.Sin(time * 1.6f + phases[i]) * wobble * viewHeight;
            float y = Mathf.Lerp(bottomLeft.y, topRight.y, heights[i]);
            bubbles[i].position = new Vector3(x, y, planeZ);
            float fade = Mathf.Clamp01((1f - heights[i]) * 4f) * Mathf.Clamp01(heights[i] * 6f);
            renderers[i].color = new Color(1f, 1f, 1f, alphas[i] * fade);
        }
    }

    private void CreateBubble(int index)
    {
        GameObject view = new GameObject("AmbientBubble");
        bubbles[index] = view.transform;
        bubbles[index].SetParent(transform, false);
        renderers[index] = view.AddComponent<SpriteRenderer>();
        renderers[index].sprite = bubbleSprite;
        renderers[index].sortingOrder = RenderOrder.AMBIENT_BUBBLES;
    }

    private void Respawn(int index, float startHeight)
    {
        Vector3 bottomLeft = ViewportToPlane(0f, 0f);
        Vector3 topRight = ViewportToPlane(1f, 1f);
        float viewHeight = topRight.y - bottomLeft.y;
        float worldSize = viewHeight * Random.Range(sizeRange.x, sizeRange.y);
        float spriteSize = bubbleSprite.bounds.size.y;
        bubbles[index].localScale = Vector3.one * (worldSize / spriteSize);
        baseX[index] = Random.value;
        speeds[index] = Random.Range(speedRange.x, speedRange.y);
        phases[index] = Random.value * Mathf.PI * 2f;
        alphas[index] = Random.Range(alphaRange.x, alphaRange.y);
        heights[index] = startHeight;
    }

    // Where a viewport corner meets the bubble plane (works for perspective and orthographic).
    private Vector3 ViewportToPlane(float x, float y)
    {
        Ray ray = gameCamera.ViewportPointToRay(new Vector3(x, y, 0f));
        float distance = (planeZ - ray.origin.z) / ray.direction.z;
        return ray.GetPoint(distance);
    }
}
