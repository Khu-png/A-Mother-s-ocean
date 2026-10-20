using UnityEngine;

public partial class GridMap
{
    private StartPoint startPoint;
    private Transform gridVisualRoot;
    private GridBoardLayout boardLayout;
    private int appliedLayoutRevision = int.MinValue;

    private void PrepareSafeArenaLayout()
    {
        if (cameraController == null || runtimeRoot == null) return;
        SafeArenaWorldLayout layout = cameraController.CalculateGridLayout(
            width, height, runtimeRoot.position.z);
        if (layout.CellSize <= Mathf.Epsilon) return;
        ApplyRootLayout(layout);
        appliedLayoutRevision = cameraController.LayoutRevision;
    }

    private void RefreshSafeArenaLayout()
    {
        if (cameraController == null || runtimeRoot == null) return;
        int revision = cameraController.LayoutRevision;
        if (revision == appliedLayoutRevision) return;

        SafeArenaWorldLayout layout = cameraController.CalculateGridLayout(
            width, height, runtimeRoot.position.z);
        if (layout.CellSize <= Mathf.Epsilon) return;

        ApplyRootLayout(layout);
        RelayoutRuntimeObjects();
        RebuildGridVisual();
        appliedLayoutRevision = revision;
    }

    private void ApplyRootLayout(SafeArenaWorldLayout layout)
    {
        float rootScale = layout.CellSize / cellSize;
        float localSpacing = layout.Spacing / rootScale;
        boardLayout = new GridBoardLayout(width, height, cellSize, localSpacing);
        runtimeRoot.position = layout.Center;
        runtimeRoot.localScale = Vector3.one * rootScale;
    }

    private void RelayoutRuntimeObjects()
    {
        foreach (MapTile tile in tiles.Values)
        {
            tile.SetLocalPosition(CellToLocalPosition(tile.Cell));
        }

        if (startPoint != null) startPoint.SetLocalPosition(CellToLocalPosition(startPoint.Cell));
        if (finishPoint != null) finishPoint.SetLocalPosition(CellToLocalPosition(finishPoint.Cell));
        foreach (TargetPoint target in targets.Values)
        {
            target.SetLocalPosition(CellToLocalPosition(target.Cell));
        }

        if (player != null) player.ApplyLayout(CellToLocalPosition(player.CurrentCell));
        if (pathTrail != null) pathTrail.Relayout(width, height, boardLayout.Spacing);
    }

    private void RebuildGridVisual()
    {
        if (gridVisualRoot != null)
        {
            gridVisualRoot.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(gridVisualRoot.gameObject);
            else DestroyImmediate(gridVisualRoot.gameObject);
        }

        gridVisualRoot = GridVisual.BuildGrid(runtimeRoot, boardLayout,
            lineWidth, gridColor, gridMaterial);
    }

}
