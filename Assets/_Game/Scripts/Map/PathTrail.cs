using System.Collections.Generic;
using UnityEngine;

public class PathTrail
{
    private readonly Transform parent;
    private readonly float cellSize;
    private readonly Color color;
    private readonly Dictionary<Vector2Int, SpriteRenderer> marks = new Dictionary<Vector2Int, SpriteRenderer>();
    private Transform root;

    public PathTrail(Transform setupParent, float setupCellSize, Color setupColor)
    {
        parent = setupParent;
        cellSize = setupCellSize;
        color = setupColor;
    }

    public bool HasMark(Vector2Int cell)
    {
        return marks.ContainsKey(cell);
    }

    public void Mark(Vector2Int cell, Vector3 worldPosition)
    {
        if (marks.ContainsKey(cell)) return;
        if (root == null)
        {
            root = new GameObject("Path Marks").transform;
            root.SetParent(parent);
        }

        SpriteRenderer mark = new GameObject($"Path {cell.x} {cell.y}").AddComponent<SpriteRenderer>();
        mark.transform.SetParent(root);
        mark.transform.position = worldPosition;
        Vector3 scale = Vector3.one * (cellSize * 0.72f);
        mark.transform.localScale = scale;
        mark.sprite = GridVisual.SquareSprite();
        mark.color = color;
        mark.sortingOrder = 1;
        mark.gameObject.AddComponent<PathMarkFeedback>().Setup(mark, color, scale);
        marks.Add(cell, mark);
    }

    public void Remove(Vector2Int cell)
    {
        if (!marks.ContainsKey(cell)) return;
        SpriteRenderer mark = marks[cell];
        if (mark != null) Object.Destroy(mark.gameObject);
        marks.Remove(cell);
    }

    public void Clear()
    {
        foreach (SpriteRenderer mark in marks.Values)
        {
            if (mark != null) Object.Destroy(mark.gameObject);
        }

        marks.Clear();
    }
}
