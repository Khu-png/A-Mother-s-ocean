using UnityEngine;

public partial class GridMap
{
    void Awake()
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
        if (runtimeRoot != null)
        {
            runtimeRoot.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(runtimeRoot.gameObject);
            else DestroyImmediate(runtimeRoot.gameObject);
            runtimeRoot = null;
        }

        tiles.Clear();
        blocks.Clear();
        oneWayPaths.Clear();
        rotateButtons.Clear();
        targets.Clear();
        currentRoute.Clear();
        player = null;
        startPoint = null;
        finishPoint = null;
        pathTrail = null;
        gridVisualRoot = null;
        boardLayout = default;
        appliedLayoutRevision = int.MinValue;
    }

    private void BuildMap()
    {
        ClearRuntimeMap();
        NormalizeSettings();
        runtimeRoot = new GameObject("RuntimeMap").transform;
        runtimeRoot.SetParent(transform, false);
        PrepareSafeArenaLayout();
        pathTrail = new PathTrail(runtimeRoot, cellSize, pathColor, width * height);
        BuildMapTiles();
        BuildPlayer();
        MarkCurrentCell();
        RelayoutRuntimeObjects();
        RebuildGridVisual();
    }

    private void NormalizeSettings()
    {
        cellSize = Mathf.Max(0.1f, cellSize);
        lineWidth = Mathf.Max(0.01f, lineWidth);
        height = Mathf.Max(1, mapRows.Length);
        width = 1;
        for (int i = 0; i < mapRows.Length; i++) width = Mathf.Max(width, mapRows[i].Length);
        boardLayout = new GridBoardLayout(width, height, cellSize, 0f);
    }

    private void BuildMapTiles()
    {
        Transform tileRoot = new GameObject("MapTiles").transform;
        tileRoot.SetParent(runtimeRoot, false);
        for (int row = 0; row < height; row++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int cell = new Vector2Int(x, height - 1 - row);
                BuildTile(tileRoot, cell, CellCode(row, x));
            }
        }
    }
}
