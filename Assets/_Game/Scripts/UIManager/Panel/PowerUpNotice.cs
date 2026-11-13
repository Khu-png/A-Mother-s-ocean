using TMPro;
using UnityEngine;

// Short message that appears instantly just above a power-up button, holds briefly,
// then fades out. Runs on unscaled time so it also works while the game is paused.
public sealed class PowerUpNotice : MonoBehaviour
{
    [SerializeField] private RectTransform noticeTransform;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text label;
    [SerializeField, Min(0f)] private float holdDuration = 1.1f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.45f;
    [SerializeField, Min(0f)] private float gapAboveButton = 18f;
    private float elapsed;
    private bool isShowing;

    void OnEnable()
    {
        Hide();
    }

    void Update()
    {
        if (!isShowing) return;
        elapsed += Time.unscaledDeltaTime;
        float fade = Mathf.Clamp01((elapsed - holdDuration) / fadeDuration);
        group.alpha = 1f - fade;
        if (fade >= 1f) Hide();
    }

    public void Show(RectTransform button, string message)
    {
        label.text = message;
        Vector3 top = new Vector3(button.rect.center.x, button.rect.yMax + gapAboveButton, 0f);
        Vector3 bottomOfNotice = Vector3.up * (noticeTransform.rect.height * noticeTransform.pivot.y);
        noticeTransform.position = button.TransformPoint(top) + noticeTransform.TransformVector(bottomOfNotice);
        group.alpha = 1f;
        elapsed = 0f;
        isShowing = true;
    }

    private void Hide()
    {
        isShowing = false;
        group.alpha = 0f;
    }
}
