using UnityEngine;

public partial class GridMap
{
    private void Awake()
    {
        if (levelData == null) return;
        BuildMap();
    }

    public void Initialize(LevelData data)
    {
        levelData = data;
        mapRows = data.MapRows;
        BuildMap();
    }

    public void ClearRuntimeMap()
    {
        for (int index = transform.childCount - 1; index >= 0; index--)
        {
            GameObject child = transform.GetChild(index).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }

        tiles.Clear();
        blocks.Clear();
        oneWayPaths.Clear();
        rotateButtons.Clear();
        targets.Clear();
        currentRoute.Clear();
        player = null;
        finishPoint = null;
        pathTrail = null;
    }

    private void BuildMap()
    {
        ClearRuntimeMap();
        NormalizeSettings();
        pathTrail = new PathTrail(transform, cellSize, pathColor);
        GridVisual.BuildGrid(transform, width, height, cellSize, lineWidth, gridColor);
        BuildMapTiles();
        BuildPlayer();
        MarkCurrentCell();
        FitCamera();
    }
}