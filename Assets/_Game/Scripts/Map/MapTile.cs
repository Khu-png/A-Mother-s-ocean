using System.Collections;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMesh label;
    [SerializeField, Min(0f)] private float rotationDuration = 0.22f;
    private Coroutine rotationRoutine;
    private bool hasPrefabSprite;

    public Vector2Int Cell => cell;
    public Sprite Sprite => spriteRenderer != null ? spriteRenderer.sprite : null;
    public bool HasPrefabSprite => hasPrefabSprite;
    public bool IsRotating => rotationRoutine != null;

    public void Setup(Vector2Int setupCell, Vector3 worldPosition, float cellSize, Sprite sprite, Color color)
    {
        cell = setupCell;
        transform.position = worldPosition;
        transform.localEulerAngles = Vector3.zero;

        spriteRenderer = spriteRenderer != null ? spriteRenderer : gameObject.AddComponent<SpriteRenderer>();
        hasPrefabSprite = spriteRenderer.sprite != null;
        if (spriteRenderer.sprite == null) spriteRenderer.sprite = MapTileAssetLibrary.SpriteOrFallback("tile_empty", sprite);
        spriteRenderer.sortingOrder = -1;
        spriteRenderer.color = color;

        label = label != null ? label : new GameObject("Label").AddComponent<TextMesh>();
        label.transform.SetParent(transform);
        label.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        label.transform.localScale = Vector3.one * (0.18f / cellSize);
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontStyle = FontStyle.Bold;
        SetLabel(string.Empty, Color.white);
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void SetSprite(Sprite sprite)
    {
        if (sprite != null) spriteRenderer.sprite = sprite;
    }

    public void SetVisualRotation(float zDegrees, bool animate = false)
    {
        StopRotation();
        if (!animate || !isActiveAndEnabled || rotationDuration <= 0f)
        {
            ApplyRotation(zDegrees);
            return;
        }

        rotationRoutine = StartCoroutine(RotateVisual(zDegrees));
    }

    private IEnumerator RotateVisual(float targetAngle)
    {
        float startAngle = transform.localEulerAngles.z;
        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / rotationDuration);
            ApplyRotation(Mathf.LerpAngle(startAngle, targetAngle, progress));
            yield return null;
        }

        ApplyRotation(targetAngle);
        rotationRoutine = null;
    }

    private void OnDisable()
    {
        StopRotation();
    }

    private void StopRotation()
    {
        if (rotationRoutine != null) StopCoroutine(rotationRoutine);
        rotationRoutine = null;
    }

    private void ApplyRotation(float zDegrees)
    {
        float rotation = Mathf.Repeat(zDegrees, 360f);
        transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        label.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(-rotation, 360f));
    }

    public void SetLabel(string text, Color color)
    {
        label.text = text;
        label.color = color;
    }
}
