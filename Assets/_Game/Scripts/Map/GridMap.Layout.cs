using UnityEngine;

public partial class GridMap
{
    private Vector3 CellToLocalPosition(Vector2Int cell)
    {
        return boardLayout.CellCenter(cell);
    }

    private char CellCode(int row, int column)
    {
        return column < mapRows[row].Length ? mapRows[row][column] : '.';
    }

    private Sprite TargetSprite() { return targetSprite != null ? targetSprite : GridVisual.CircleSprite(); }

}
