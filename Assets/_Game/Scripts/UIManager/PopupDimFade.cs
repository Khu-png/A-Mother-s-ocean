using UnityEngine;
using UnityEngine.UI;

// Fades a popup's dark backdrop in from transparent each time the popup opens,
// on unscaled time so it also works while the game is paused.
public sealed class PopupDimFade : MonoBehaviour
{
    [SerializeField] private Image dim;
    [SerializeField, Min(0.01f)] private float duration = 0.22f;
    private Color restColor;
    private float elapsed;

    void Awake()
    {
        restColor = dim.color;
    }

    void OnEnable()
    {
        elapsed = 0f;
        ApplyAlpha(0f);
    }

    void Update()
    {
        if (elapsed >= duration) return;
        elapsed += Time.unscaledDeltaTime;
        ApplyAlpha(Mathf.SmoothStep(0f, 1f, elapsed / duration));
    }

    private void ApplyAlpha(float progress)
    {
        Color color = restColor;
        color.a *= progress;
        dim.color = color;
    }
}
