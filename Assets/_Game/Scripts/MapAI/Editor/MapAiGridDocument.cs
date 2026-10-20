using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class MapAiGridDocument
{
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 5;
    [SerializeField] private int version;
    [SerializeField] private List<string> rows = new List<string>();
    [SerializeField] private List<bool> lockedCells = new List<bool>();

    public int Width => width;
    public int Height => height;
    public int Version => version;

    public void Initialize()
    {
        if (rows.Count == height && lockedCells.Count == width * height) return;
        Replace(CreateEmptyRows(width, height));
        lockedCells = CreateLocks(width, height);
    }

    public char GetCell(int x, int y) { return rows[y][x]; }

    public bool IsLocked(int x, int y) { return lockedCells[y * width + x]; }

    public void SetCell(int x, int y, char code)
    {
        if (code == 'S' || code == 'F') RemoveCode(code);
        char[] row = rows[y].ToCharArray();
        row[x] = code;
        rows[y] = new string(row);
        version++;
    }

    public void ToggleLock(int x, int y)
    {
        int index = y * width + x;
        lockedCells[index] = !lockedCells[index];
        version++;
    }

    public void Resize(int newWidth, int newHeight)
    {
        newWidth = Mathf.Clamp(newWidth, MapBuilderLimits.MIN_SIZE, MapBuilderLimits.MAX_COLUMNS);
        newHeight = Mathf.Clamp(newHeight, MapBuilderLimits.MIN_SIZE, MapBuilderLimits.MAX_ROWS);
        string[] resized = CreateEmptyRows(newWidth, newHeight);
        List<bool> resizedLocks = CreateLocks(newWidth, newHeight);
        int copyWidth = Mathf.Min(width, newWidth);
        int copyHeight = Mathf.Min(height, newHeight);

        for (int y = 0; y < copyHeight; y++)
        {
            char[] row = resized[y].ToCharArray();
            for (int x = 0; x < copyWidth; x++)
            {
                row[x] = GetCell(x, y);
                resizedLocks[y * newWidth + x] = IsLocked(x, y);
            }
            resized[y] = new string(row);
        }

        width = newWidth;
        height = newHeight;
        rows = new List<string>(resized);
        lockedCells = resizedLocks;
        version++;
    }

    public void Replace(string[] replacement)
    {
        height = Mathf.Clamp(replacement.Length, MapBuilderLimits.MIN_SIZE, MapBuilderLimits.MAX_ROWS);
        width = MapBuilderLimits.MIN_SIZE;
        for (int y = 0; y < height; y++) width = Mathf.Max(width, replacement[y].Length);
        width = Mathf.Min(width, MapBuilderLimits.MAX_COLUMNS);
        rows = new List<string>(NormalizeRows(replacement, width, height));
        lockedCells = CreateLocks(width, height);
        version++;
    }

    public void ApplyPreview(string[] replacement)
    {
        rows = new List<string>(NormalizeRows(replacement, width, height));
        version++;
    }

    public MapAiMapData Snapshot() { return new MapAiMapData(ToRows(), version); }

    public string[] ToRows() { return rows.ToArray(); }

    public bool[] ToLocks() { return lockedCells.ToArray(); }

    private void RemoveCode(char code)
    {
        for (int y = 0; y < height; y++)
        {
            int x = rows[y].IndexOf(code);
            if (x < 0) continue;
            char[] row = rows[y].ToCharArray();
            row[x] = '.';
            rows[y] = new string(row);
        }
    }

    private static string[] NormalizeRows(string[] source, int columns, int rowCount)
    {
        string[] normalized = CreateEmptyRows(columns, rowCount);
        for (int y = 0; y < rowCount; y++)
        {
            char[] row = normalized[y].ToCharArray();
            int copyLength = Mathf.Min(columns, source[y].Length);
            for (int x = 0; x < copyLength; x++) row[x] = source[y][x];
            normalized[y] = new string(row);
        }
        return normalized;
    }

    private static string[] CreateEmptyRows(int columns, int rowCount)
    {
        string[] emptyRows = new string[rowCount];
        for (int y = 0; y < rowCount; y++) emptyRows[y] = new string('.', columns);
        return emptyRows;
    }

    private static List<bool> CreateLocks(int columns, int rowCount)
    {
        return new List<bool>(new bool[columns * rowCount]);
    }
}
