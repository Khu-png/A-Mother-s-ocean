using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Opens and closes the loading cover through a soft circular hole (iris transition).
// The loading content fades out before the hole opens and fades in after it closes.
public sealed class LoadingScreenFade : MonoBehaviour
{
    private static readonly int RADIUS = Shader.PropertyToID("_Radius");
    private const float CLOSED_RADIUS = 0f;
    private const float OPEN_RADIUS = 1f;

    [SerializeField] private CanvasGroup coverGroup;
    [SerializeField] private Image irisImage;
    [SerializeField] private Material irisMaterial;
    [SerializeField] private CanvasGroup[] contentGroups;
    [SerializeField, Min(0.01f)] private float closeDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float openDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float contentFadeDuration = 0.15f;
    private Material irisInstance;
    private float currentRadius;
    private float contentAlpha;

    // The scene starts with the cover fully closed and its content visible.
    void Awake()
    {
        irisInstance = new Material(irisMaterial);
        irisImage.material = irisInstance;
        coverGroup.blocksRaycasts = true;
        SetRadius(CLOSED_RADIUS);
        SetContentAlpha(1f);
    }

    void OnDestroy()
    {
        Destroy(irisInstance);
    }

    public IEnumerator FadeIn()
    {
        gameObject.SetActive(true);
        coverGroup.blocksRaycasts = true;
        // Hide the progress content only when the cover has to close over the old screen.
        if (currentRadius > CLOSED_RADIUS) SetContentAlpha(0f);
        yield return AnimateRadius(CLOSED_RADIUS, closeDuration);
        yield return AnimateContent(1f);
    }

    public IEnumerator FadeOut()
    {
        yield return AnimateContent(0f);
        yield return AnimateRadius(OPEN_RADIUS, openDuration);
        coverGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateRadius(float targetRadius, float fullDuration)
    {
        float startRadius = currentRadius;
        // Scale by remaining distance so an already-closed cover does not wait.
        float duration = fullDuration * Mathf.Abs(targetRadius - startRadius);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            SetRadius(Mathf.Lerp(startRadius, targetRadius, progress));
            yield return null;
        }
        SetRadius(targetRadius);
    }

    private IEnumerator AnimateContent(float targetAlpha)
    {
        float startAlpha = contentAlpha;
        float duration = contentFadeDuration * Mathf.Abs(targetAlpha - startAlpha);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetContentAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }
        SetContentAlpha(targetAlpha);
    }

    private void SetRadius(float radius)
    {
        currentRadius = radius;
        irisInstance.SetFloat(RADIUS, radius);
    }

    private void SetContentAlpha(float alpha)
    {
        contentAlpha = alpha;
        for (int i = 0; i < contentGroups.Length; i++) contentGroups[i].alpha = alpha;
    }
}
