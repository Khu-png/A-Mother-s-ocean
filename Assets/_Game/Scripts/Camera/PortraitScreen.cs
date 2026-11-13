using UnityEngine;

// The game always plays inside a centred portrait frame. On wide screens (tablets, desktop)
// the frame is narrower than the screen and side bars fill the rest; on phones it is the
// whole screen. All layout that used to read Screen / Screen.safeArea goes through here.
public static class PortraitScreen
{
    // Widest allowed width / height ratio; 9:16 phones (0.5625) and taller fit without bars.
    public const float MAX_ASPECT = 0.62f;

    // The portrait frame in screen pixels.
    public static Rect FrameRect
    {
        get
        {
            float width = Screen.width;
            float height = Screen.height;
            float frameWidth = Mathf.Min(width, height * MAX_ASPECT);
            return new Rect((width - frameWidth) * 0.5f, 0f, frameWidth, height);
        }
    }

    // Screen.safeArea clipped to the frame, in screen pixels.
    public static Rect SafeArea
    {
        get
        {
            Rect frame = FrameRect;
            Rect safe = Screen.safeArea;
            float xMin = Mathf.Max(frame.xMin, safe.xMin);
            float xMax = Mathf.Min(frame.xMax, safe.xMax);
            return Rect.MinMaxRect(xMin, safe.yMin, Mathf.Max(xMin, xMax), safe.yMax);
        }
    }

    // A screen-pixel point expressed as 0-1 coordinates inside the frame.
    public static Vector2 ToFrameNormalized(Vector2 screenPoint)
    {
        Rect frame = FrameRect;
        return new Vector2((screenPoint.x - frame.xMin) / frame.width,
            (screenPoint.y - frame.yMin) / frame.height);
    }
}
