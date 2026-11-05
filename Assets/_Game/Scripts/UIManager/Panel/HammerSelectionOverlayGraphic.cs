using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class HammerSelectionOverlayGraphic : MaskableGraphic
{
    [SerializeField] private Color dimColor = Color.black;
    [SerializeField, Min(1f)] private float cutoutSize = 126f;
    private readonly List<Vector2> targetPositions = new();
    private readonly List<Rect> visibleAreas = new();
    private readonly List<Rect> splitAreas = new();
    private bool isVisible;

    public void Show(IReadOnlyList<Vector2> positions)
    {
        targetPositions.Clear();
        for (int i = 0; i < positions.Count; i++) targetPositions.Add(positions[i]);
        isVisible = true;
        SetVerticesDirty();
    }

    public void Hide()
    {
        isVisible = false;
        targetPositions.Clear();
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (!isVisible) return;
        Rect rect = rectTransform.rect;
        BuildDimAreas(rect);
        for (int i = 0; i < visibleAreas.Count; i++) AddRect(mesh, visibleAreas[i], dimColor);
    }

    private void BuildDimAreas(Rect bounds)
    {
        visibleAreas.Clear();
        visibleAreas.Add(bounds);
        float half = cutoutSize * 0.5f;
        for (int i = 0; i < targetPositions.Count; i++)
            CutArea(new Rect(targetPositions[i] - Vector2.one * half,
                Vector2.one * cutoutSize));
    }

    private void CutArea(Rect hole)
    {
        splitAreas.Clear();
        for (int i = 0; i < visibleAreas.Count; i++) SplitArea(visibleAreas[i], hole);
        visibleAreas.Clear();
        visibleAreas.AddRange(splitAreas);
    }

    private void SplitArea(Rect area, Rect hole)
    {
        if (!area.Overlaps(hole))
        {
            splitAreas.Add(area);
            return;
        }
        float left = Mathf.Max(area.xMin, hole.xMin);
        float right = Mathf.Min(area.xMax, hole.xMax);
        float bottom = Mathf.Max(area.yMin, hole.yMin);
        float top = Mathf.Min(area.yMax, hole.yMax);
        AddArea(area.xMin, area.yMin, left - area.xMin, area.height);
        AddArea(right, area.yMin, area.xMax - right, area.height);
        AddArea(left, area.yMin, right - left, bottom - area.yMin);
        AddArea(left, top, right - left, area.yMax - top);
    }

    private void AddArea(float x, float y, float width, float height)
    {
        if (width > 0f && height > 0f) splitAreas.Add(new Rect(x, y, width, height));
    }

    private static void AddRect(VertexHelper mesh, Rect rect, Color color)
    {
        AddQuad(mesh, rect.min, new Vector2(rect.xMin, rect.yMax),
            rect.max, new Vector2(rect.xMax, rect.yMin), color);
    }

    private static void AddQuad(VertexHelper mesh, Vector2 a, Vector2 b,
        Vector2 c, Vector2 d, Color color)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(a, color, Vector2.zero);
        mesh.AddVert(b, color, Vector2.up);
        mesh.AddVert(c, color, Vector2.one);
        mesh.AddVert(d, color, Vector2.right);
        mesh.AddTriangle(start, start + 1, start + 2);
        mesh.AddTriangle(start, start + 2, start + 3);
    }
}
