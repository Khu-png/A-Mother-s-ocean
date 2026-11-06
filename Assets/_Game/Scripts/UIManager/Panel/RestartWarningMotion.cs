using System.Collections;
using UnityEngine;

public sealed class RestartWarningMotion : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform dialog;
    [SerializeField, Min(0.01f)] private float duration = 0.24f;
    [SerializeField, Min(0f)] private float startOffset = 110f;
    [SerializeField, Range(0.5f, 1f)] private float startScale = 0.78f;
    private Vector2 homePosition;
    private Vector3 homeScale;
    private Coroutine opening;
    private bool initialized;

    public bool IsVisible => gameObject.activeSelf;

    void OnDisable()
    {
        opening = null;
    }

    public void Show()
    {
        Initialize();
        group.alpha = 0f;
        dialog.anchoredPosition = homePosition + Vector2.down * startOffset;
        dialog.localScale = homeScale * startScale;
        gameObject.SetActive(true);
        opening = StartCoroutine(AnimateOpen());
    }

    public void Hide()
    {
        if (opening != null) StopCoroutine(opening);
        opening = null;
        group.alpha = 1f;
        dialog.anchoredPosition = homePosition;
        dialog.localScale = homeScale;
        gameObject.SetActive(false);
    }

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;
        homePosition = dialog.anchoredPosition;
        homeScale = dialog.localScale;
    }

    private IEnumerator AnimateOpen()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float movement = 1f - Mathf.Pow(1f - progress, 3f);
            float bounce = Mathf.Sin(progress * Mathf.PI) * 0.06f;
            group.alpha = progress;
            dialog.anchoredPosition = Vector2.LerpUnclamped(
                homePosition + Vector2.down * startOffset, homePosition, movement);
            dialog.localScale = homeScale * Mathf.Lerp(startScale, 1f, movement)
                * (1f + bounce);
            yield return null;
        }
        group.alpha = 1f;
        dialog.anchoredPosition = homePosition;
        dialog.localScale = homeScale;
        opening = null;
    }
}
