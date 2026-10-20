using System.Collections.Generic;
using UnityEngine;

public sealed class MapAiMapData
{
    private readonly string[] rows;

    public int Width { get; }
    public int Height => rows.Length;
    public int Version { get; }

    public MapAiMapData(string[] sourceRows, int version = 0)
    {
        Width = 0;
        for (int y = 0; y < sourceRows.Length; y++) Width = Mathf.Max(Width, sourceRows[y].Length);
        rows = new string[sourceRows.Length];
        for (int y = 0; y < sourceRows.Length; y++) rows[y] = sourceRows[y].PadRight(Width, '.');
        Version = version;
    }

    public char GetCell(Vector2Int cell) { return rows[cell.y][cell.x]; }

    public bool Contains(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
    }

    public List<Vector2Int> Find(char code)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (rows[y][x] == code) cells.Add(new Vector2Int(x, y));
            }
        }
        return cells;
    }

    public string[] ToRows() { return (string[])rows.Clone(); }
}
