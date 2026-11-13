using UnityEngine;

// Red dot on the Inv (skin) tab while a bought skin has not been looked at yet.
// The dot gently pulses so it draws the eye without being noisy.
public sealed class MenuNewSkinBadge : MonoBehaviour
{
    [SerializeField] private RectTransform dot;
    [SerializeField, Range(0f, 0.4f)] private float pulseAmount = 0.15f;
    [SerializeField, Min(0f)] private float pulseSpeed = 4f;
    private Vector3 dotRestScale;

    void Awake()
    {
        dotRestScale = dot.localScale;
    }

    void Update()
    {
        if (!dot.gameObject.activeSelf) return;
        float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        dot.localScale = dotRestScale * pulse;
    }

    public void Refresh()
    {
        dot.gameObject.SetActive(MenuPlayerProgress.HasNewSkin);
    }
}
