using System.Collections.Generic;
using UnityEngine;

public static class MapAiBranchAnalyzer
{
    public static void FindBranches(MapAiMapData map, MapAiAnalysisResult result)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>(4);
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (MapCode.IsBlock(map.GetCell(cell))) continue;
                MapAiMovementGraph.GetTwoWay(map, cell, neighbors);
                if (neighbors.Count != 1 || cell == result.Start || cell == result.Finish) continue;
                MapAiBranch branch = TraceBranch(map, cell, neighbors[0], result);
                if (branch.Cells.Count > 1) result.DeadEnds.Add(branch);
            }
        }
    }

    private static MapAiBranch TraceBranch(MapAiMapData map, Vector2Int leaf,
        Vector2Int next, MapAiAnalysisResult result)
    {
        List<Vector2Int> cells = new List<Vector2Int> { leaf };
        List<Vector2Int> neighbors = new List<Vector2Int>(4);
        Vector2Int previous = leaf;
        Vector2Int current = next;

        while (true)
        {
            cells.Add(current);
            MapAiMovementGraph.GetTwoWay(map, current, neighbors);
            if (neighbors.Count != 2) break;
            Vector2Int following = neighbors[0] == previous ? neighbors[1] : neighbors[0];
            previous = current;
            current = following;
        }

        bool approachesFinish = (leaf - result.Finish).sqrMagnitude
            < (current - result.Finish).sqrMagnitude;
        bool requiresReturn = result.DistanceToFinish.TryGetValue(leaf, out int leafDistance)
            && result.DistanceToFinish.TryGetValue(current, out int junctionDistance)
            && leafDistance > junctionDistance;
        return new MapAiBranch(leaf, current, cells, approachesFinish && requiresReturn);
    }
}
