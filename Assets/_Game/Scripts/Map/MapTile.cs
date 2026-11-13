using System.Collections;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMesh label;
    [SerializeField] private TileWinVisual winVisual;
    [SerializeField, Min(0f)] private float rotationDuration = 0.22f;
    private Coroutine rotationRoutine;
    private Coroutine flashRoutine;
    private Color flashBaseColor;
    private Transform cachedTransform;
    private Transform labelTransform;
    private float configuredCellSize;
    private Vector3 entranceBaseScale;

    public Vector2Int Cell => cell;
    public bool IsRotating => rotationRoutine != null;
    public Vector3 WorldPosition => cachedTransform.position;

    public void BeginWinDetailFade()
    {
        StopRotation();
        winVisual.BeginDetailFade(cachedTransform.localRotation);
    }

    public void ApplyWinDetailFade(float progress)
    {
        winVisual.ApplyDetailFade(progress);
    }

    public void ApplyWinBaseClear(float progress)
    {
        winVisual.ApplyBaseClear(progress);
    }

    void Awake()
    {
        cachedTransform = transform;
    }

    void OnDisable()
    {
        StopRotation();
        StopFlash();
    }

    public void Setup(Vector2Int setupCell, Vector3 localPosition, float cellSize, Color color)
    {
        cell = setupCell;
        configuredCellSize = cellSize;
        cachedTransform = transform;
        cachedTransform.localPosition = localPosition;
        cachedTransform.localEulerAngles = Vector3.zero;
        winVisual.ConfigureBase(cellSize, cachedTransform.localScale);

        spriteRenderer.sortingOrder = -1;
        spriteRenderer.color = color;

        labelTransform = label.transform;
        labelTransform.localPosition = new Vector3(0f, 0f, -0.01f);
        labelTransform.localScale = Vector3.one * (0.18f / cellSize);
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontStyle = FontStyle.Bold;
        SetLabel(string.Empty, Color.white);
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        cachedTransform.localPosition = localPosition;
    }

    public void BeginEntrance()
    {
        entranceBaseScale = cachedTransform.localScale;
    }

    public void ApplyEntranceScale(float factor)
    {
        cachedTransform.localScale = entranceBaseScale * factor;
    }

    public void SetColor(Color color)
    {
        StopFlash();
        spriteRenderer.color = color;
    }

    public void ApplyObstacleBreak(Sprite pathSprite, Material depthMaterial,
        Vector3 pathScale)
    {
        cachedTransform.localScale = pathScale;
        spriteRenderer.sprite = pathSprite;
        spriteRenderer.color = Color.white;
        winVisual.ApplyObstacleBreak(depthMaterial);
        winVisual.ConfigureBase(configuredCellSize, pathScale);
    }

    public void ConfigureVisuals(SpriteRenderer setupRenderer, TextMesh setupLabel)
    {
        spriteRenderer = setupRenderer;
        label = setupLabel;
    }

    public void SetVisualRotation(float zDegrees, bool animate = false, float delay = 0f)
    {
        StopRotation();
        if (!animate || !isActiveAndEnabled || rotationDuration <= 0f)
        {
            ApplyRotation(zDegrees);
            return;
        }

        rotationRoutine = StartCoroutine(RotateVisual(zDegrees, delay));
    }

    // Briefly tints the tile toward flashColor and back; restarting keeps the original color.
    public void Flash(Color flashColor, float duration)
    {
        StopFlash();
        flashBaseColor = spriteRenderer.color;
        flashRoutine = StartCoroutine(FlashColor(flashColor, duration));
    }

    private IEnumerator FlashColor(Color flashColor, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pulse = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            spriteRenderer.color = Color.Lerp(flashBaseColor, flashColor, pulse);
            yield return null;
        }
        spriteRenderer.color = flashBaseColor;
        flashRoutine = null;
    }

    private void StopFlash()
    {
        if (flashRoutine == null) return;
        StopCoroutine(flashRoutine);
        spriteRenderer.color = flashBaseColor;
        flashRoutine = null;
    }

    private IEnumerator RotateVisual(float targetAngle, float delay)
    {
        // A delay lets several pipes rotate as a ripple spreading from the button.
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float startAngle = cachedTransform.localEulerAngles.z;
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

    private void StopRotation()
    {
        if (rotationRoutine != null) StopCoroutine(rotationRoutine);
        rotationRoutine = null;
    }

    private void ApplyRotation(float zDegrees)
    {
        float rotation = Mathf.Repeat(zDegrees, 360f);
        cachedTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        labelTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(-rotation, 360f));
    }

    public void SetLabel(string text, Color color)
    {
        label.text = text;
        label.color = color;
    }
}
