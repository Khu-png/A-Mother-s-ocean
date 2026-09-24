using UnityEngine;

public partial class GridMap
{
    private Vector3 CellToWorld(Vector2Int cell)
    {
        float left = -width * cellSize * 0.5f;
        float bottom = -height * cellSize * 0.5f;
        return new Vector3(left + (cell.x + 0.5f) * cellSize, bottom + (cell.y + 0.5f) * cellSize, 0f);
    }

    private char CellCode(int row, int column)
    {
        return column < mapRows[row].Length ? mapRows[row][column] : '.';
    }

    private Sprite TargetSprite() { return targetSprite != null ? targetSprite : GridVisual.CircleSprite(); }

    private void FitCamera()
    {
        if (cameraController != null) cameraController.FitToGrid(width, height, cellSize);
    }
}