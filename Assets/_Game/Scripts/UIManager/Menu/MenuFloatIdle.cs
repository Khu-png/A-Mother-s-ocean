using UnityEngine;

// Makes a menu element float like it is underwater: a slow bob up and down plus a gentle tilt.
// Children (for example the level text on the shark's board) move with it.
// Runs on unscaled time so it keeps moving while the game is paused.
public sealed class MenuFloatIdle : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField, Min(0f)] private float bobHeight = 14f;
    [SerializeField, Min(0.1f)] private float bobPeriod = 2.8f;
    [SerializeField, Min(0f)] private float tiltDegrees = 2f;
    [SerializeField, Min(0.1f)] private float tiltPeriod = 4.2f;
    private Vector2 restPosition;
    private Quaternion restRotation;

    void Awake()
    {
        restPosition = target.anchoredPosition;
        restRotation = target.localRotation;
    }

    void OnDisable()
    {
        target.anchoredPosition = restPosition;
        target.localRotation = restRotation;
    }

    void Update()
    {
        float time = Time.unscaledTime;
        float bob = Mathf.Sin(time * Mathf.PI * 2f / bobPeriod) * bobHeight;
        float tilt = Mathf.Sin(time * Mathf.PI * 2f / tiltPeriod) * tiltDegrees;
        target.anchoredPosition = restPosition + Vector2.up * bob;
        target.localRotation = restRotation * Quaternion.Euler(0f, 0f, tilt);
    }
}
