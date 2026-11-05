using UnityEngine;

public readonly struct GridBoardLayout
{
    public readonly int Columns;
    public readonly int Rows;
    public readonly float CellSize;
    public readonly float Spacing;

    public float Pitch => CellSize + Spacing;
    public float Width => Columns * CellSize + Mathf.Max(0, Columns - 1) * Spacing;
    public float Height => Rows * CellSize + Mathf.Max(0, Rows - 1) * Spacing;
    public float Left => Width * -0.5f;
    public float Right => Width * 0.5f;
    public float Bottom => Height * -0.5f;
    public float Top => Height * 0.5f;

    public GridBoardLayout(int columns, int rows, float cellSize, float spacing)
    {
        Columns = Mathf.Max(1, columns);
        Rows = Mathf.Max(1, rows);
        CellSize = Mathf.Max(0.01f, cellSize);
        Spacing = Mathf.Max(0f, spacing);
    }

    public Vector3 CellCenter(Vector2Int cell)
    {
        return new Vector3(Left + CellSize * 0.5f + cell.x * Pitch,
            Bottom + CellSize * 0.5f + cell.y * Pitch, 0f);
    }

    public bool TryGetCell(Vector2 localPosition, out Vector2Int cell)
    {
        int column = Mathf.RoundToInt((localPosition.x - Left - CellSize * 0.5f) / Pitch);
        int row = Mathf.RoundToInt((localPosition.y - Bottom - CellSize * 0.5f) / Pitch);
        cell = new Vector2Int(column, row);
        if (column < 0 || column >= Columns || row < 0 || row >= Rows) return false;
        Vector2 center = CellCenter(cell);
        float halfSize = CellSize * 0.5f;
        return Mathf.Abs(localPosition.x - center.x) <= halfSize
            && Mathf.Abs(localPosition.y - center.y) <= halfSize;
    }

    public float VerticalBoundary(int index)
    {
        if (index <= 0) return Left;
        if (index >= Columns) return Right;
        return Left + index * Pitch - Spacing * 0.5f;
    }

    public float HorizontalBoundary(int index)
    {
        if (index <= 0) return Bottom;
        if (index >= Rows) return Top;
        return Bottom + index * Pitch - Spacing * 0.5f;
    }
}
