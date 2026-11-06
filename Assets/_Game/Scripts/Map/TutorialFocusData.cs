using UnityEngine;

public enum TutorialFocusKind
{
    Player,
    Target,
    Finish
}

public readonly struct TutorialFocusData
{
    public TutorialFocusData(Vector2 screenPosition, Sprite sprite, Color color)
    {
        ScreenPosition = screenPosition;
        Sprite = sprite;
        Color = color;
    }

    public Vector2 ScreenPosition { get; }
    public Sprite Sprite { get; }
    public Color Color { get; }
}
