using UnityEngine;

public sealed class TileWinVisual : MonoBehaviour
{
    private static readonly int OPACITY = Shader.PropertyToID("_Opacity");
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer face;
    [SerializeField] private MeshRenderer depth;
    [SerializeField] private TextMesh label;
    [SerializeField] private Transform shadowTransform;
    [SerializeField] private SpriteRenderer shadow;
    [SerializeField] private Sprite baseTileSprite;
    [SerializeField, Min(0f)] private float liftDistance = 0.4f;
    [SerializeField, Range(0f, 0.3f)] private float orthographicScaleBoost = 0.12f;
    private MaterialPropertyBlock properties;
    private Color faceColor;
    private Color labelColor;
    private Vector3 baseTileScale;
    private float baseLiftDistance;

    void Awake()
    {
        properties = new MaterialPropertyBlock();
    }

    public void ConfigureBase(float cellSize, Vector3 tileScale)
    {
        Vector2 spriteSize = baseTileSprite.bounds.size;
        baseTileScale = new Vector3(cellSize / (tileScale.x * spriteSize.x),
            cellSize / (tileScale.y * spriteSize.y), 1f);
        baseLiftDistance = cellSize * liftDistance / tileScale.z;
    }

    public void BeginDetailFade(Quaternion tileRotation)
    {
        faceColor = face.color;
        labelColor = label.color;
        shadow.sprite = baseTileSprite;
        shadowTransform.localPosition = Vector3.zero;
        shadowTransform.localScale = baseTileScale;
        shadowTransform.localRotation = Quaternion.Inverse(tileRotation);
        shadow.color = Color.white;
        ApplyDetailFade(0f);
    }

    public void ApplyDetailFade(float progress)
    {
        float opacity = 1f - Mathf.SmoothStep(0f, 1f, progress);
        ApplyOpacity(opacity);
        visualRoot.gameObject.SetActive(progress < 1f);
    }

    public void ApplyBaseClear(float progress)
    {
        float eased = Mathf.SmoothStep(0f, 1f, progress);
        float bounce = Mathf.Sin(progress * Mathf.PI);
        shadowTransform.localPosition = Vector3.back * (baseLiftDistance * bounce);
        shadowTransform.localScale = baseTileScale * (1f + orthographicScaleBoost * bounce);
        shadow.color = new Color(1f, 1f, 1f, 1f - eased);
        shadow.gameObject.SetActive(progress < 1f);
    }

    private void ApplyOpacity(float opacity)
    {
        Color color = faceColor;
        color.a *= opacity;
        face.color = color;
        color = labelColor;
        color.a *= opacity;
        label.color = color;
        properties.SetFloat(OPACITY, opacity);
        depth.SetPropertyBlock(properties);
    }

}
