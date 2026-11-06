using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class MenuSkinTransition : MonoBehaviour
{
    [SerializeField] private Image preview;
    [SerializeField, Min(0.01f)] private float phaseDuration = 0.2f;
    [SerializeField, Min(1f)] private float travelDistance = 240f;
    [SerializeField, Range(0.5f, 1f)] private float travelScale = 0.84f;
    private RectTransform previewTransform;
    private Vector2 homePosition;
    private Vector3 homeScale;
    private Coroutine transition;

    public bool IsPlaying => transition != null;

    void Awake()
    {
        previewTransform = preview.rectTransform;
        homePosition = previewTransform.anchoredPosition;
        homeScale = previewTransform.localScale;
    }

    void OnDisable()
    {
        if (transition != null) StopCoroutine(transition);
        transition = null;
        RestorePreview();
    }

    public void Play(int direction, Action swapSkin)
    {
        transition = StartCoroutine(Animate(direction, swapSkin));
    }

    private IEnumerator Animate(int direction, Action swapSkin)
    {
        Vector2 offset = Vector2.right * travelDistance * direction;
        yield return Move(homePosition, homePosition - offset,
            homeScale, homeScale * travelScale);
        swapSkin();
        previewTransform.anchoredPosition = homePosition + offset;
        previewTransform.localScale = homeScale * travelScale;
        yield return Move(homePosition + offset, homePosition,
            homeScale * travelScale, homeScale);
        transition = null;
    }

    private IEnumerator Move(Vector2 fromPosition, Vector2 toPosition,
        Vector3 fromScale, Vector3 toScale)
    {
        float elapsed = 0f;
        while (elapsed < phaseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / phaseDuration);
            float eased = progress * progress * (3f - 2f * progress);
            previewTransform.anchoredPosition = Vector2.LerpUnclamped(
                fromPosition, toPosition, eased);
            previewTransform.localScale = Vector3.LerpUnclamped(
                fromScale, toScale, eased);
            yield return null;
        }
    }

    private void RestorePreview()
    {
        previewTransform.anchoredPosition = homePosition;
        previewTransform.localScale = homeScale;
    }
}
