using UnityEngine;
using UnityEngine.UI;

// Hand animations used by the gameplay tutorial: a swipe hint and a tap-here point.
public static class TutorialHandMotion
{
    private const float POINT_BOB_HEIGHT = 12f;
    private const float SWIPE_FADE_START = 0.72f;

    public static void PlaceAtAnchor(RectTransform rect, Vector2 anchor, Vector2 offset)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = offset;
    }

    public static void Swipe(Image hand, Color baseColor, Vector2 offset,
        float distance, float time, float duration, float repeatDelay)
    {
        float cycle = duration + repeatDelay;
        float progress = Mathf.Clamp01(time % cycle / duration);
        float movement = Mathf.SmoothStep(0f, 1f, progress);
        hand.rectTransform.anchoredPosition = offset + Vector2.right * (distance * movement);
        Color color = baseColor;
        color.a *= 1f - Mathf.Clamp01((progress - SWIPE_FADE_START) / (1f - SWIPE_FADE_START));
        hand.color = color;
    }

    // The fingertip rests on the anchor, then dips away and taps back onto it.
    public static void Point(Image hand, Color baseColor, Vector2 offset, float time)
    {
        float pulse = (Mathf.Sin(time * 4.5f) + 1f) * 0.5f;
        hand.rectTransform.anchoredPosition = offset + Vector2.down * (pulse * POINT_BOB_HEIGHT);
        hand.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1.05f, pulse);
        hand.color = baseColor;
    }
}
