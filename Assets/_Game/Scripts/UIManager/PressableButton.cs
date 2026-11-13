using UnityEngine;
using UnityEngine.UI;

// A Button that sinks slightly while held and springs back with a small overshoot on release.
// It keeps every Button setting, so existing prefabs switch to it without losing data.
// The spring runs on unscaled time, so it also works while the game is paused.
public sealed class PressableButton : Button
{
    [SerializeField, Range(0.7f, 1f)] private float pressedScale = 0.92f;
    [SerializeField, Min(1f)] private float stiffness = 420f;
    [SerializeField, Range(0f, 1f)] private float damping = 0.72f;
    private const float SETTLE_THRESHOLD = 0.0005f;
    private Vector3 restScale;
    private float currentScale = 1f;
    private float targetScale = 1f;
    private float velocity;
    private bool isSettled = true;

    protected override void Awake()
    {
        base.Awake();
        restScale = transform.localScale;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Only restore the scale this component changed; other scale owners keep theirs.
        if (!isSettled || !Mathf.Approximately(currentScale, 1f)) transform.localScale = restScale;
        currentScale = 1f;
        targetScale = 1f;
        velocity = 0f;
        isSettled = true;
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        float nextTarget = state == SelectionState.Pressed ? pressedScale : 1f;
        // Leave the transform alone unless the press state really changes it.
        if (isSettled && Mathf.Approximately(nextTarget, targetScale)) return;
        targetScale = nextTarget;
        isSettled = false;
    }

    void Update()
    {
        if (isSettled) return;
        float deltaTime = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        velocity += (targetScale - currentScale) * stiffness * deltaTime;
        velocity *= Mathf.Pow(1f - damping, deltaTime * 10f);
        currentScale += velocity * deltaTime;
        bool atRest = Mathf.Abs(targetScale - currentScale) < SETTLE_THRESHOLD
            && Mathf.Abs(velocity) < SETTLE_THRESHOLD;
        if (atRest)
        {
            currentScale = targetScale;
            isSettled = true;
        }
        transform.localScale = restScale * currentScale;
    }
}
