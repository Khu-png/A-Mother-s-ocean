using System.Collections;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private Vector2Int cell;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMesh label;
    [SerializeField, Min(0f)] private float rotationDuration = 0.22f;
    private Coroutine rotationRoutine;
    private Transform cachedTransform;
    private Transform labelTransform;

    public Vector2Int Cell => cell;
    public bool IsRotating => rotationRoutine != null;

    void Awake()
    {
        cachedTransform = transform;
    }

    void OnDisable()
    {
        StopRotation();
    }

    public void Setup(Vector2Int setupCell, Vector3 localPosition, float cellSize, Color color)
    {
        cell = setupCell;
        cachedTransform = transform;
        cachedTransform.localPosition = localPosition;
        cachedTransform.localEulerAngles = Vector3.zero;

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

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void ConfigureVisuals(SpriteRenderer setupRenderer, TextMesh setupLabel)
    {
        spriteRenderer = setupRenderer;
        label = setupLabel;
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
