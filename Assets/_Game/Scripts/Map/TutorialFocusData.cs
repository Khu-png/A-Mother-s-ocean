using UnityEngine;

public enum TutorialFocusKind
{
    Player,
    Target,
    Finish,
    Rock,
    OneWay,
    Whirlpool
}

public readonly struct TutorialFocusData
{
    // A zero screen size means the icon keeps its default size (characters).
    public TutorialFocusData(Vector2 screenPosition, Sprite sprite, Color color,
        Vector2 screenSize = default, float rotationDegrees = 0f)
    {
        ScreenPosition = screenPosition;
        Sprite = sprite;
        Color = color;
        ScreenSize = screenSize;
        RotationDegrees = rotationDegrees;
    }

    public Vector2 ScreenPosition { get; }
    public Sprite Sprite { get; }
    public Color Color { get; }
    public Vector2 ScreenSize { get; }
    public float RotationDegrees { get; }
}
