using System;
using UnityEngine;

[Serializable]
public struct SafeArenaPadding
{
    [Min(0f)] public float Left;
    [Min(0f)] public float Right;
    [Min(0f)] public float Top;
    [Min(0f)] public float Bottom;

    public SafeArenaPadding(float left, float right, float top, float bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }
}

[Serializable]
public sealed class SafeArenaSettings
{
    [SerializeField] private Rect normalizedRect = new Rect(0.04f, 0.2f, 0.92f, 0.64f);
    [SerializeField] private SafeArenaPadding padding = new SafeArenaPadding(16f, 16f, 16f, 16f);
    [SerializeField, Min(0f)] private float cellSpacing = 6f;
    [SerializeField, Min(1f)] private float maximumCellSize = 180f;
    [SerializeField] private bool showEditorPreview = true;
    [SerializeField] private Color previewColor = new Color(0.1f, 0.9f, 0.8f, 0.9f);

    public Rect NormalizedRect => normalizedRect;
    public SafeArenaPadding Padding => padding;
    public float CellSpacing => Mathf.Max(0f, cellSpacing);
    public float MaximumCellSize => Mathf.Max(1f, maximumCellSize);
    public bool ShowEditorPreview => showEditorPreview;
    public Color PreviewColor => previewColor;
}

public readonly struct SafeArenaScreenLayout
{
    public readonly Rect Arena;
    public readonly Rect UsableArea;
    public readonly Vector2 BoardCenter;
    public readonly Vector2 BoardSize;
    public readonly float CellSize;
    public readonly float Spacing;

    public SafeArenaScreenLayout(Rect arena, Rect usableArea, Vector2 boardCenter,
        Vector2 boardSize, float cellSize, float spacing)
    {
        Arena = arena;
        UsableArea = usableArea;
        BoardCenter = boardCenter;
        BoardSize = boardSize;
        CellSize = cellSize;
        Spacing = spacing;
    }
}

public readonly struct SafeArenaWorldLayout
{
    public readonly Vector3 Center;
    public readonly float CellSize;
    public readonly float Spacing;

    public SafeArenaWorldLayout(Vector3 center, float cellSize, float spacing)
    {
        Center = center;
        CellSize = cellSize;
        Spacing = spacing;
    }
}

public static class SafeArenaCalculator
{
    public static SafeArenaScreenLayout Calculate(Rect deviceSafeArea,
        SafeArenaSettings settings, int columns, int rows)
    {
        Rect arena = CalculateArena(deviceSafeArea, settings.NormalizedRect);
        Rect usableArea = ApplyPadding(arena, settings.Padding);
        int safeColumns = Mathf.Max(1, columns);
        int safeRows = Mathf.Max(1, rows);
        float spacing = settings.CellSpacing;
        float widthForCells = usableArea.width - spacing * (safeColumns - 1);
        float heightForCells = usableArea.height - spacing * (safeRows - 1);
        float widthCellSize = Mathf.Max(0f, widthForCells) / safeColumns;
        float heightCellSize = Mathf.Max(0f, heightForCells) / safeRows;
        float cellSize = Mathf.Min(widthCellSize, heightCellSize, settings.MaximumCellSize);
        Vector2 boardSize = new Vector2(
            cellSize * safeColumns + spacing * (safeColumns - 1),
            cellSize * safeRows + spacing * (safeRows - 1));
        return new SafeArenaScreenLayout(arena, usableArea, usableArea.center,
            boardSize, cellSize, spacing);
    }

    public static Rect CalculateArena(Rect safeArea, Rect normalizedRect)
    {
        float x = Mathf.Clamp01(normalizedRect.x);
        float y = Mathf.Clamp01(normalizedRect.y);
        float width = Mathf.Clamp(normalizedRect.width, 0f, 1f - x);
        float height = Mathf.Clamp(normalizedRect.height, 0f, 1f - y);
        return new Rect(safeArea.x + safeArea.width * x,
            safeArea.y + safeArea.height * y,
            safeArea.width * width, safeArea.height * height);
    }

    public static Rect ApplyPadding(Rect arena, SafeArenaPadding padding)
    {
        float left = Mathf.Max(0f, padding.Left);
        float right = Mathf.Max(0f, padding.Right);
        float top = Mathf.Max(0f, padding.Top);
        float bottom = Mathf.Max(0f, padding.Bottom);
        float x = arena.x + left;
        float y = arena.y + bottom;
        float width = Mathf.Max(0f, arena.width - left - right);
        float height = Mathf.Max(0f, arena.height - top - bottom);
        return new Rect(x, y, width, height);
    }
}
