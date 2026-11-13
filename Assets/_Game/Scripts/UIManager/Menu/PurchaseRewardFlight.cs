using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// After a purchase the bought item pops up in the middle of the screen. Power-ups then drift
// up and fade out in place; a heart flies along an arc into the heart bar, which bumps on landing.
// Runs on unscaled time so it also works while the game is paused.
public sealed class PurchaseRewardFlight : MonoBehaviour
{
    [SerializeField] private RectTransform layer;
    [SerializeField] private RectTransform flyingItem;
    [SerializeField] private Image flyingImage;
    [SerializeField] private TMP_Text amountLabel;
    [SerializeField] private CanvasGroup ribbonGroup;
    [SerializeField] private RewardCelebrationBackdrop backdrop;
    [SerializeField, Min(0.01f)] private float popDuration = 0.35f;
    [SerializeField, Min(0f)] private float holdDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float flyDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float bumpDuration = 0.3f;
    [SerializeField, Min(0f)] private float arcHeight = 320f;
    [SerializeField, Range(0f, 0.6f)] private float bumpAmount = 0.25f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.5f;
    [SerializeField, Min(0f)] private float fadeRise = 60f;
    private RectTransform target;
    private Image targetImage;
    private Vector2 targetPoint;
    private Vector3 targetRestScale;
    private Action landed;
    private float elapsed;
    private bool isPlaying;
    private bool fadesInPlace;

    private float FlightEnd => popDuration + holdDuration + flyDuration;

    void OnEnable()
    {
        flyingItem.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        Complete();
    }

    void Update()
    {
        if (!isPlaying) return;
        elapsed += Time.unscaledDeltaTime;
        if (elapsed < popDuration + holdDuration) ApplyPop(Mathf.Clamp01(elapsed / popDuration));
        else if (fadesInPlace)
        {
            float fade = (elapsed - popDuration - holdDuration) / fadeDuration;
            if (fade < 1f) ApplyFade(fade);
            else Complete();
        }
        else if (elapsed < FlightEnd) ApplyFlight((elapsed - popDuration - holdDuration) / flyDuration);
        else if (elapsed < FlightEnd + bumpDuration) ApplyBump((elapsed - FlightEnd) / bumpDuration);
        else Complete();
    }

    // Pops the item up, then lets it drift up and fade away where it is.
    public void PlayInPlace(Sprite itemSprite, string amount)
    {
        Begin(itemSprite, amount, true);
    }

    // spritePoint is a 0-1 position inside the destination's drawn sprite (0.5, 0.5 = centre),
    // so the item lands on a specific detail of the art, such as the heart badge.
    public void Play(Sprite itemSprite, string amount, Image destination,
        Vector2 spritePoint, Action onLanded)
    {
        Begin(itemSprite, amount, false);
        targetImage = destination;
        target = destination.rectTransform;
        targetPoint = spritePoint;
        targetRestScale = target.localScale;
        landed = onLanded;
    }

    private void Begin(Sprite itemSprite, string amount, bool inPlace)
    {
        Complete();
        flyingImage.sprite = itemSprite;
        flyingImage.color = Color.white;
        amountLabel.text = amount;
        fadesInPlace = inPlace;
        elapsed = 0f;
        isPlaying = true;
        flyingItem.gameObject.SetActive(true);
        ApplyPop(0f);
        backdrop.Play(popDuration + holdDuration);
        SoundManager.Ins.PlaySound(GameConstants.COLLECT_SOUND, 1.2f, 1f);
    }

    private void ApplyFade(float progress)
    {
        Vector3 center = layer.TransformPoint(layer.rect.center);
        flyingItem.position = center + layer.TransformVector(Vector3.up * (fadeRise * progress));
        flyingItem.localScale = Vector3.one * (1f + 0.1f * progress);
        float alpha = 1f - progress * progress;
        flyingImage.color = new Color(1f, 1f, 1f, alpha);
        ribbonGroup.alpha = alpha;
    }

    private void ApplyPop(float progress)
    {
        flyingItem.position = layer.TransformPoint(layer.rect.center);
        float overshoot = progress < 0.6f
            ? Mathf.Lerp(0f, 1.25f, 1f - (1f - progress / 0.6f) * (1f - progress / 0.6f))
            : Mathf.Lerp(1.25f, 1f, Mathf.SmoothStep(0f, 1f, (progress - 0.6f) / 0.4f));
        flyingItem.localScale = Vector3.one * overshoot;
        ribbonGroup.alpha = 1f;
    }

    private void ApplyFlight(float progress)
    {
        float eased = progress * progress * (3f - 2f * progress);
        Vector3 start = layer.TransformPoint(layer.rect.center);
        Vector3 end = DestinationWorldPoint();
        Vector3 control = (start + end) * 0.5f + layer.TransformVector(Vector3.up * arcHeight);
        Vector3 a = Vector3.Lerp(start, control, eased);
        Vector3 b = Vector3.Lerp(control, end, eased);
        flyingItem.position = Vector3.Lerp(a, b, eased);
        flyingItem.localScale = Vector3.one * Mathf.Lerp(1f, EndScale(), eased);
        ribbonGroup.alpha = 1f - progress;
    }

    private void ApplyBump(float progress)
    {
        if (flyingItem.gameObject.activeSelf) Land();
        target.localScale = targetRestScale * (1f + bumpAmount * Mathf.Sin(progress * Mathf.PI));
    }

    private void Land()
    {
        flyingItem.gameObject.SetActive(false);
        SoundManager.Ins.PlaySound(GameConstants.BUTTON_SOUND, 1.4f, 1f);
        Action callback = landed;
        landed = null;
        callback();
    }

    // Finishes instantly (for example when a new purchase starts mid-flight).
    private void Complete()
    {
        if (!isPlaying) return;
        isPlaying = false;
        backdrop.Hide();
        if (fadesInPlace)
        {
            flyingItem.gameObject.SetActive(false);
            return;
        }
        if (flyingItem.gameObject.activeSelf) Land();
        target.localScale = targetRestScale;
    }

    private Vector3 DestinationWorldPoint()
    {
        Rect rect = DrawnSpriteRect();
        return target.TransformPoint(new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, targetPoint.x),
            Mathf.Lerp(rect.yMin, rect.yMax, targetPoint.y)));
    }

    // The area the sprite actually covers: Preserve Aspect letterboxes it inside the rect.
    private Rect DrawnSpriteRect()
    {
        Rect rect = target.rect;
        if (!targetImage.preserveAspect) return rect;
        Rect spriteRect = targetImage.sprite.rect;
        float spriteAspect = spriteRect.width / spriteRect.height;
        if (rect.width / rect.height > spriteAspect)
        {
            float width = rect.height * spriteAspect;
            return new Rect(rect.center.x - width * 0.5f, rect.yMin, width, rect.height);
        }
        float height = rect.width / spriteAspect;
        return new Rect(rect.xMin, rect.center.y - height * 0.5f, rect.width, height);
    }

    // Shrinks the item to roughly the destination's height so it looks like it goes inside.
    private float EndScale()
    {
        float destinationHeight = DrawnSpriteRect().height * target.lossyScale.y;
        float itemHeight = flyingItem.rect.height * layer.lossyScale.y;
        return Mathf.Clamp(destinationHeight / itemHeight, 0.15f, 1f);
    }
}
