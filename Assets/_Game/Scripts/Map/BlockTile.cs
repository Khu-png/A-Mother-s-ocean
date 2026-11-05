using System.Collections;
using UnityEngine;

public class BlockTile : MonoBehaviour
{
    [SerializeField] private MapTile tile;
    [SerializeField] private Color blockColor = new Color(0.02f, 0.03f, 0.04f, 1f);
    [SerializeField] private Sprite pathSprite;
    [SerializeField] private Material pathDepthMaterial;
    [SerializeField] private Vector3 pathScale = Vector3.one * 1.2f;
    private Coroutine feedbackRoutine;
    private Transform tileTransform;
    private Vector3 baseScale;

    public MapTile Tile => tile;

    void Awake()
    {
        tileTransform = transform;
        baseScale = tileTransform.localScale;
    }

    void OnDisable()
    {
        StopFeedback();
        tileTransform.localScale = baseScale;
        tileTransform.localRotation = Quaternion.identity;
    }

    public void BindTile(MapTile setupTile)
    {
        tile = setupTile;
    }

    public void Setup(MapTile setupTile, Color color)
    {
        tile = setupTile;
        tileTransform = tile.transform;
        baseScale = tileTransform.localScale;
        blockColor = color;
        tile.SetColor(blockColor);
        tile.SetLabel(string.Empty, Color.white);
    }

    public void PulseHighlight()
    {
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(Pulse());
    }

    public void PlayBreak()
    {
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(BreakReaction());
    }

    private IEnumerator Pulse()
    {
        Vector3 baseScale = tileTransform.localScale;
        yield return AnimateScale(baseScale, baseScale * 1.08f, 0.08f);
        yield return AnimateScale(baseScale * 1.08f, baseScale, 0.1f);
        feedbackRoutine = null;
    }

    private IEnumerator BreakReaction()
    {
        Vector3 obstacleScale = tileTransform.localScale;
        yield return AnimateScale(obstacleScale, obstacleScale * 0.82f, 0.07f);
        tile.ApplyObstacleBreak(pathSprite, pathDepthMaterial, pathScale * 0.78f);
        yield return AnimateScale(pathScale * 0.78f, pathScale * 1.12f, 0.12f);
        yield return AnimateScale(pathScale * 1.12f, pathScale, 0.15f);
        tileTransform.localScale = pathScale;
        feedbackRoutine = null;
    }

    private IEnumerator AnimateScale(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            tileTransform.localScale = Vector3.LerpUnclamped(from, to, progress);
            float shake = Mathf.Sin(progress * Mathf.PI * 6f) * (1f - progress) * 4f;
            tileTransform.localRotation = Quaternion.Euler(0f, 0f, shake);
            yield return null;
        }
        tileTransform.localScale = to;
        tileTransform.localRotation = Quaternion.identity;
    }

    private void StopFeedback()
    {
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = null;
    }
}
