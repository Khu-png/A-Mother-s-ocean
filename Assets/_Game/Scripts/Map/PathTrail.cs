using System.Collections.Generic;
using UnityEngine;

public class PathTrail
{
    private readonly Transform ROOT;
    private readonly float CELL_SIZE;
    private readonly Color COLOR;
    private readonly Dictionary<Vector2Int, PathMarkFeedback> MARKS = new Dictionary<Vector2Int, PathMarkFeedback>();
    private readonly Stack<PathMarkFeedback> AVAILABLE_MARKS = new Stack<PathMarkFeedback>();

    public PathTrail(Transform setupParent, float setupCellSize, Color setupColor, int capacity)
    {
        ROOT = new GameObject("PathMarks").transform;
        ROOT.SetParent(setupParent, false);
        CELL_SIZE = setupCellSize;
        COLOR = setupColor;
        // A route can visit each grid cell once; prewarm outside movement callbacks.
        for (int index = 0; index < capacity; index++) AVAILABLE_MARKS.Push(CreateMark());
    }

    public bool HasMark(Vector2Int cell)
    {
        return MARKS.ContainsKey(cell);
    }

    public void Mark(Vector2Int cell, Vector3 localPosition)
    {
        if (MARKS.ContainsKey(cell) || AVAILABLE_MARKS.Count == 0) return;
        PathMarkFeedback mark = AVAILABLE_MARKS.Pop();
        mark.Restart(localPosition);
        mark.gameObject.SetActive(true);
        MARKS.Add(cell, mark);
    }

    public void Relayout(int width, int height, float spacing)
    {
        foreach (KeyValuePair<Vector2Int, PathMarkFeedback> entry in MARKS)
        {
            entry.Value.Restart(CellPosition(entry.Key, width, height, spacing));
        }
    }

    public void Remove(Vector2Int cell)
    {
        if (!MARKS.ContainsKey(cell)) return;
        ReturnMark(MARKS[cell]);
        MARKS.Remove(cell);
    }

    public void Clear()
    {
        foreach (PathMarkFeedback mark in MARKS.Values) ReturnMark(mark);
        MARKS.Clear();
    }

    private PathMarkFeedback CreateMark()
    {
        GameObject view = new GameObject("PathMark");
        view.transform.SetParent(ROOT, false);
        SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
        renderer.sprite = GridVisual.SquareSprite();
        renderer.color = COLOR;
        renderer.sortingOrder = 1;
        PathMarkFeedback feedback = view.AddComponent<PathMarkFeedback>();
        feedback.Setup(renderer, COLOR, Vector3.one * (CELL_SIZE * 0.72f));
        view.SetActive(false);
        return feedback;
    }

    private Vector3 CellPosition(Vector2Int cell, int width, int height, float spacing)
    {
        float pitch = CELL_SIZE + spacing;
        float boardWidth = width * CELL_SIZE + (width - 1) * spacing;
        float boardHeight = height * CELL_SIZE + (height - 1) * spacing;
        return new Vector3(-boardWidth * 0.5f + CELL_SIZE * 0.5f + cell.x * pitch,
            -boardHeight * 0.5f + CELL_SIZE * 0.5f + cell.y * pitch, 0f);
    }

    private void ReturnMark(PathMarkFeedback mark)
    {
        mark.gameObject.SetActive(false);
        AVAILABLE_MARKS.Push(mark);
    }
}
