using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public sealed class MapAiGridViewport
{
    [SerializeField, Range(0.5f, 2.5f)] private float zoom = 1f;
    [SerializeField] private Vector2 pan;

    public bool Draw(Rect area, MapAiGridDocument document, string[] displayedRows,
        bool isPreview, char? brush, bool lockMode, MapAiGridOverlay overlay,
        UnityEngine.Object undoOwner)
    {
        EditorGUI.DrawRect(area, new Color(0.09f, 0.11f, 0.13f));
        HandleNavigation(area);
        MapAiMapData map = new MapAiMapData(displayedRows);
        float cellSize = CalculateCellSize(area, map) * zoom;
        Vector2 origin = area.center - new Vector2(map.Width * cellSize,
            map.Height * cellSize) * 0.5f + pan;
        GUIStyle cellStyle = CreateCenteredStyle();
        bool changed = false;

        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Rect cellRect = new Rect(origin.x + x * cellSize,
                    origin.y + y * cellSize, cellSize, cellSize);
                DrawCell(cellRect, map.GetCell(cell), overlay, cell,
                    !isPreview && document.IsLocked(x, y), cellStyle);
                if (!isPreview && HandlePaint(cellRect, document, x, y,
                    brush, lockMode, undoOwner)) changed = true;
            }
        }

        DrawPreviewLabel(area, isPreview, cellStyle);
        return changed;
    }

    private void HandleNavigation(Rect area)
    {
        Event current = Event.current;
        if (!area.Contains(current.mousePosition)) return;
        if (current.type == EventType.ScrollWheel)
        {
            zoom = Mathf.Clamp(zoom * (1f - current.delta.y * 0.04f), 0.5f, 2.5f);
            current.Use();
        }
        if (current.type == EventType.MouseDrag && current.button == 2)
        {
            pan += current.delta;
            current.Use();
        }
    }

    private static float CalculateCellSize(Rect area, MapAiMapData map)
    {
        float availableWidth = Mathf.Max(1f, area.width - 32f);
        float availableHeight = Mathf.Max(1f, area.height - 32f);
        return Mathf.Clamp(Mathf.Min(availableWidth / map.Width,
            availableHeight / map.Height), 24f, 76f);
    }

    private static void DrawCell(Rect rect, char code, MapAiGridOverlay overlay,
        Vector2Int cell, bool locked, GUIStyle cellStyle)
    {
        Color color = CellColor(code);
        if (overlay.Shortest.Contains(cell)) color = Color.Lerp(color, new Color(0.2f, 0.85f, 0.25f), 0.55f);
        if (overlay.DeadEnds.Contains(cell)) color = Color.Lerp(color, new Color(1f, 0.65f, 0.1f), 0.5f);
        if (overlay.Misleading.Contains(cell)) color = Color.Lerp(color, new Color(1f, 0.2f, 0.65f), 0.65f);
        EditorGUI.DrawRect(new Rect(rect.x + 1f, rect.y + 1f,
            rect.width - 2f, rect.height - 2f), color);
        GUI.Label(rect, code.ToString(), cellStyle);
        if (locked) GUI.Label(new Rect(rect.xMax - 18f, rect.y + 2f, 16f, 16f), "L");
    }

    private static bool HandlePaint(Rect rect, MapAiGridDocument document,
        int x, int y, char? brush, bool lockMode, UnityEngine.Object undoOwner)
    {
        Event current = Event.current;
        if (current.type != EventType.MouseDown || current.button != 0
            || !rect.Contains(current.mousePosition)) return false;
        if (!lockMode && (!brush.HasValue || document.IsLocked(x, y))) return false;
        Undo.RecordObject(undoOwner, lockMode ? "Khóa/mở khóa ô bản đồ" : "Vẽ ô bản đồ");
        if (lockMode) document.ToggleLock(x, y);
        else document.SetCell(x, y, brush.Value);
        current.Use();
        return true;
    }

    private static GUIStyle CreateCenteredStyle()
    {
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13
        };
        style.normal.textColor = Color.white;
        return style;
    }

    private static Color CellColor(char code)
    {
        if (code == 'X') return new Color(0.08f, 0.1f, 0.12f);
        if (code == 'S') return new Color(0.1f, 0.62f, 0.34f);
        if (code == 'F') return new Color(0.18f, 0.42f, 0.78f);
        if (code == 'T') return new Color(0.82f, 0.55f, 0.08f);
        if (code == 'O') return new Color(0.78f, 0.3f, 0.08f);
        if (MapCode.IsOneWay(code)) return new Color(0.08f, 0.42f, 0.58f);
        return new Color(0.2f, 0.25f, 0.28f);
    }

    private static void DrawPreviewLabel(Rect area, bool isPreview, GUIStyle style)
    {
        if (!isPreview) return;
        Rect label = new Rect(area.x + 10f, area.y + 10f, 150f, 22f);
        EditorGUI.DrawRect(label, new Color(0.12f, 0.45f, 0.62f, 0.9f));
        GUI.Label(label, "XEM TRƯỚC - CHƯA ÁP DỤNG", style);
    }
}

public sealed class MapAiGridOverlay
{
    public readonly HashSet<Vector2Int> Shortest = new HashSet<Vector2Int>();
    public readonly HashSet<Vector2Int> DeadEnds = new HashSet<Vector2Int>();
    public readonly HashSet<Vector2Int> Misleading = new HashSet<Vector2Int>();

    public static MapAiGridOverlay Build(MapAiAnalysisResult result,
        MapAiAnalysisPanel analysisPanel)
    {
        MapAiGridOverlay overlay = new MapAiGridOverlay();
        if (analysisPanel.OverlayMode == MapAiOverlayMode.CompletionPath)
            overlay.Shortest.UnionWith(result.CompletionPath);
        for (int i = 0; i < result.DeadEnds.Count; i++)
        {
            MapAiBranch branch = result.DeadEnds[i];
            if (analysisPanel.OverlayMode == MapAiOverlayMode.DeadEnds)
                overlay.DeadEnds.UnionWith(branch.Cells);
            if (analysisPanel.OverlayMode == MapAiOverlayMode.MisleadingCandidates
                && branch.IsMisleadingCandidate)
                overlay.Misleading.UnionWith(branch.Cells);
            if (analysisPanel.OverlayMode == MapAiOverlayMode.SelectedBranch
                && analysisPanel.SelectedBranch == i)
            {
                if (branch.IsMisleadingCandidate) overlay.Misleading.UnionWith(branch.Cells);
                else overlay.DeadEnds.UnionWith(branch.Cells);
            }
        }
        if (analysisPanel.OverlayMode == MapAiOverlayMode.SelectedRoute
            && analysisPanel.SelectedPath >= 0 && analysisPanel.SelectedPath < result.Paths.Count)
            overlay.Shortest.UnionWith(result.Paths[analysisPanel.SelectedPath]);
        return overlay;
    }
}
