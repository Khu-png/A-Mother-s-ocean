using System.Collections.Generic;
using UnityEngine;

public static class MapAiPathSearch
{
    public static void Enumerate(MapAiMapData map, MapAiSearchBudget budget,
        MapAiAnalysisResult result)
    {
        if (!result.IsValid) return;
        bool[,] visited = new bool[map.Width, map.Height];
        List<Vector2Int> path = new List<Vector2Int> { result.Start };
        HashSet<Vector2Int> collectedTargets = new HashSet<Vector2Int>();
        List<Vector2Int> targets = map.Find('T');
        visited[result.Start.x, result.Start.y] = true;
        Search(map, budget, result, result.Start, visited, path,
            collectedTargets, targets.Count, 0);
        MeasureDecisionPoints(map, targets, result);
    }

    private static bool Search(MapAiMapData map, MapAiSearchBudget budget,
        MapAiAnalysisResult result, Vector2Int current, bool[,] visited,
        List<Vector2Int> path, HashSet<Vector2Int> collectedTargets,
        int targetCount, int rotationPhase)
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
        if (current == result.Finish && collectedTargets.Count == targetCount)
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
                || !MapAiMovementGraph.CanMove(map, current, next, rotationPhase)) continue;
            if (next == result.Finish && collectedTargets.Count < targetCount) continue;
            visited[next.x, next.y] = true;
            path.Add(next);
            bool collectedTarget = map.GetCell(next) == 'T'
                && collectedTargets.Add(next);
            int nextRotationPhase = map.GetCell(next) == 'O'
                ? (rotationPhase + 1) % 4 : rotationPhase;
            bool shouldContinue = Search(map, budget, result, next, visited,
                path, collectedTargets, targetCount, nextRotationPhase);
            if (collectedTarget) collectedTargets.Remove(next);
            path.RemoveAt(path.Count - 1);
            visited[next.x, next.y] = false;
            if (!shouldContinue) return false;
        }
        return true;
    }

    private static void MeasureDecisionPoints(MapAiMapData map,
        List<Vector2Int> targets, MapAiAnalysisResult result)
    {
        Vector2Int finish = map.Find('F')[0];
        for (int i = 0; i < result.Paths.Count; i++)
        {
            int decisions = CountDecisionPoints(map, targets, finish, result.Paths[i]);
            result.DecisionPoints = Mathf.Max(result.DecisionPoints, decisions);
        }
    }

    private static int CountDecisionPoints(MapAiMapData map,
        List<Vector2Int> targets, Vector2Int finish, List<Vector2Int> route)
    {
        bool[,] visited = new bool[map.Width, map.Height];
        HashSet<Vector2Int> collected = new HashSet<Vector2Int>();
        int rotationPhase = 0;
        int decisions = 0;
        for (int i = 0; i < route.Count - 1; i++)
        {
            Vector2Int current = route[i];
            visited[current.x, current.y] = true;
            if (map.GetCell(current) == 'T') collected.Add(current);
            int alternatives = CountAvailableMoves(map, current, visited,
                rotationPhase, collected.Count, targets.Count, finish);
            if (alternatives > 1) decisions++;
            Vector2Int next = route[i + 1];
            if (map.GetCell(next) == 'O') rotationPhase = (rotationPhase + 1) % 4;
        }
        return decisions;
    }

    private static int CountAvailableMoves(MapAiMapData map,
        Vector2Int current, bool[,] visited, int rotationPhase,
        int collectedTargets, int targetCount, Vector2Int finish)
    {
        int count = 0;
        for (int i = 0; i < MapAiMovementGraph.DirectionCount; i++)
        {
            Vector2Int next = current + MapAiMovementGraph.Direction(i);
            if (!map.Contains(next) || visited[next.x, next.y]
                || !MapAiMovementGraph.CanMove(map, current, next, rotationPhase)) continue;
            if (next == finish && collectedTargets < targetCount) continue;
            count++;
        }
        return count;
    }
}
