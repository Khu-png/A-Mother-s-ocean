using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class PowerUpIconMotion
{
    public struct HomeState
    {
        public Vector2 AnchorMin, AnchorMax, Pivot, Position, Size;
        public Vector3 Scale;
        public Quaternion Rotation;
        public Color Color;
    }

    public static HomeState Capture(RectTransform icon, Image image)
    {
        return new HomeState
        {
            AnchorMin = icon.anchorMin,
            AnchorMax = icon.anchorMax,
            Pivot = icon.pivot,
            Position = icon.anchoredPosition,
            Size = icon.sizeDelta,
            Scale = icon.localScale,
            Rotation = icon.localRotation,
            Color = image.color
        };
    }

    public static void Restore(RectTransform icon, Image image,
        RectTransform home, HomeState state)
    {
        icon.SetParent(home, false);
        icon.anchorMin = state.AnchorMin;
        icon.anchorMax = state.AnchorMax;
        icon.pivot = state.Pivot;
        icon.anchoredPosition = state.Position;
        icon.sizeDelta = state.Size;
        icon.localScale = state.Scale;
        icon.localRotation = state.Rotation;
        image.color = state.Color;
    }

    public static IEnumerator FreezeIntro(RectTransform icon, Image image,
        RectTransform root)
    {
        Vector2 start = Prepare(icon, root, icon.pivot);
        float elapsed = 0f;
        const float travelDuration = 0.72f;
        while (elapsed < travelDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / travelDuration);
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            icon.anchoredPosition = Vector2.Lerp(start, Vector2.zero, eased)
                + Vector2.up * Mathf.Sin(progress * Mathf.PI) * 70f;
            icon.localScale = Vector3.one * Mathf.Lerp(1f, 3f, eased);
            icon.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Sin(progress * Mathf.PI) * -10f);
            yield return null;
        }
        yield return FadeFreezeIcon(icon, image);
    }

    private static IEnumerator FadeFreezeIcon(RectTransform icon, Image image)
    {
        Color color = image.color;
        float elapsed = 0f;
        const float holdDuration = 0.12f;
        const float duration = 0.52f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float fadeProgress = Mathf.InverseLerp(holdDuration, duration, elapsed);
            float eased = Mathf.SmoothStep(0f, 1f, fadeProgress);
            icon.localScale = Vector3.one * Mathf.Lerp(3f, 3.7f, eased);
            color.a = 1f - eased;
            image.color = color;
            yield return null;
        }
    }

    public static IEnumerator HammerStrike(RectTransform icon, RectTransform root,
        Vector2 destination)
    {
        Vector2 start = Prepare(icon, root, new Vector2(0.3f, 0.18f));
        Vector2 rise = start + new Vector2(-22f, 80f);
        Vector2 stance = destination + new Vector2(-78f, -14f);
        yield return AnimateHammer(icon, start, rise, 0.14f, 0f, -140f, 1f, 1.08f);
        yield return AnimateHammer(icon, rise, stance, 0.34f, -140f, -690f, 1.08f, 1.18f);
        yield return HoldHammer(icon, 0.12f);
        yield return AnimateHammer(icon, stance, stance, 0.15f, -690f, -765f, 1.18f, 1.3f);
    }

    public static IEnumerator HammerRetreat(RectTransform icon)
    {
        Vector2 start = icon.anchoredPosition;
        float elapsed = 0f;
        const float duration = 0.24f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            icon.anchoredPosition = start + Vector2.up * progress * 55f;
            icon.localScale = Vector3.one * Mathf.Lerp(1.3f, 0f, progress);
            yield return null;
        }
    }

    private static IEnumerator AnimateHammer(RectTransform icon, Vector2 from,
        Vector2 to, float duration, float fromAngle, float toAngle,
        float fromScale, float toScale)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            icon.anchoredPosition = Vector2.LerpUnclamped(from, to, progress);
            icon.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.LerpUnclamped(fromAngle, toAngle, progress));
            icon.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, progress);
            yield return null;
        }
    }

    private static IEnumerator HoldHammer(RectTransform icon, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public static Vector2 LocalPoint(RectTransform root, Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root, screenPosition, null, out Vector2 localPoint);
        return localPoint;
    }

    private static Vector2 Prepare(RectTransform icon, RectTransform root, Vector2 pivot)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, icon.position);
        Vector2 size = icon.rect.size;
        icon.SetParent(root, false);
        icon.anchorMin = icon.anchorMax = Vector2.one * 0.5f;
        icon.sizeDelta = size;
        icon.pivot = pivot;
        Vector2 center = LocalPoint(root, screen);
        icon.anchoredPosition = center + Vector2.Scale(size, icon.pivot - Vector2.one * 0.5f);
        return icon.anchoredPosition;
    }
}
