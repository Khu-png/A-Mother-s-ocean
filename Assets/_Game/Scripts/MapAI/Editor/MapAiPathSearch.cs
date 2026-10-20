using System.Collections.Generic;
using UnityEngine;

public static class MapAiPathSearch
{
    public static void Enumerate(MapAiMapData map, MapAiSearchBudget budget,
        MapAiAnalysisResult result)
    {
        if (!result.IsReachable) return;
        bool[,] visited = new bool[map.Width, map.Height];
        List<Vector2Int> path = new List<Vector2Int> { result.Start };
        visited[result.Start.x, result.Start.y] = true;
        Search(map, budget, result, result.Start, visited, path);
    }

    private static bool Search(MapAiMapData map, MapAiSearchBudget budget,
        MapAiAnalysisResult result, Vector2Int current, bool[,] visited,
        List<Vector2Int> path)
    {
        if (result.SearchExpansions >= budget.MaximumExpansions)
        {
            result.SearchLimited = true;
            return false;
        }
        if (path.Count > budget.MaximumPathLength)
        {
            result.SearchLimited = true;
            return true;
        }
        result.SearchExpansions++;
        if (current == result.Finish)
        {
            result.Paths.Add(new List<Vector2Int>(path));
            if (result.Paths.Count < budget.MaximumPaths) return true;
            result.SearchLimited = true;
            return false;
        }

        for (int i = 0; i < MapAiMovementGraph.DirectionCount; i++)
        {
            Vector2Int next = current + MapAiMovementGraph.Direction(i);
            if (!map.Contains(next) || visited[next.x, next.y]
                || !MapAiMovementGraph.CanMove(map, current, next)) continue;
            visited[next.x, next.y] = true;
            path.Add(next);
            bool shouldContinue = Search(map, budget, result, next, visited, path);
            path.RemoveAt(path.Count - 1);
            visited[next.x, next.y] = false;
            if (!shouldContinue) return false;
        }
        return true;
    }
}
