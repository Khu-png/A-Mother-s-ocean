using System.Collections.Generic;
using UnityEngine;

public static class MapAiAnalyzer
{
    public static MapAiAnalysisResult Analyze(MapAiMapData map,
        MapAiSearchBudget budget, MapAiDifficulty difficulty)
    {
        MapAiAnalysisResult result = new MapAiAnalysisResult();
        List<Vector2Int> starts = map.Find('S');
        List<Vector2Int> finishes = map.Find('F');
        ValidateEndpoints(starts, finishes, result);
        if (!result.IsValid) return result;

        result.Start = starts[0];
        result.Finish = finishes[0];
        FindShortestPath(map, result);
        FindDistancesToFinish(map, result);
        ValidateTargets(map, result);
        MapAiBranchAnalyzer.FindBranches(map, result);
        MapAiPathSearch.Enumerate(map, budget, result);
        FindCompletionPath(map, result);
        result.SuggestedSeconds = SuggestedTime(result, difficulty);
        return result;
    }

    private static void ValidateEndpoints(List<Vector2Int> starts,
        List<Vector2Int> finishes, MapAiAnalysisResult result)
    {
        if (starts.Count != 1) result.Issues.Add($"Map cần đúng 1 điểm S, hiện có {starts.Count}.");
        if (finishes.Count != 1) result.Issues.Add($"Map cần đúng 1 điểm F, hiện có {finishes.Count}.");
        result.IsValid = result.Issues.Count == 0;
    }

    private static void FindShortestPath(MapAiMapData map, MapAiAnalysisResult result)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> previous = new Dictionary<Vector2Int, Vector2Int>();
        result.ReachableFromStart.Add(result.Start);
        List<Vector2Int> neighbors = new List<Vector2Int>(4);
        queue.Enqueue(result.Start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == result.Finish) break;
            MapAiMovementGraph.GetOutgoing(map, current, neighbors);
            for (int i = 0; i < neighbors.Count; i++)
            {
                Vector2Int next = neighbors[i];
                if (!result.ReachableFromStart.Add(next)) continue;
                previous.Add(next, current);
                queue.Enqueue(next);
            }
        }

        result.IsReachable = result.ReachableFromStart.Contains(result.Finish);
        if (!result.IsReachable)
        {
            result.Issues.Add("Không có đường hợp lệ từ S tới F theo luật di chuyển hiện tại.");
            return;
        }

        Vector2Int step = result.Finish;
        result.ShortestPath.Add(step);
        while (step != result.Start)
        {
            step = previous[step];
            result.ShortestPath.Add(step);
        }
        result.ShortestPath.Reverse();
    }

    private static void FindDistancesToFinish(MapAiMapData map, MapAiAnalysisResult result)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> incoming = new List<Vector2Int>(4);
        result.DistanceToFinish.Add(result.Finish, 0);
        queue.Enqueue(result.Finish);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            MapAiMovementGraph.GetIncoming(map, current, incoming);
            for (int i = 0; i < incoming.Count; i++)
            {
                Vector2Int previous = incoming[i];
                if (result.DistanceToFinish.ContainsKey(previous)) continue;
                result.DistanceToFinish.Add(previous, result.DistanceToFinish[current] + 1);
                queue.Enqueue(previous);
            }
        }
    }

    private static void ValidateTargets(MapAiMapData map, MapAiAnalysisResult result)
    {
        List<Vector2Int> targets = map.Find('T');
        for (int i = 0; i < targets.Count; i++)
        {
            Vector2Int target = targets[i];
            if (!result.ReachableFromStart.Contains(target))
            {
                result.Issues.Add($"Không thể đi từ S tới target ({target.x}, {target.y}).");
                result.IsValid = false;
            }
            if (!result.DistanceToFinish.ContainsKey(target))
            {
                result.Issues.Add($"Target tại ({target.x}, {target.y}) không thể tới F.");
                result.IsValid = false;
            }
        }
    }

    private static int SuggestedTime(MapAiAnalysisResult result, MapAiDifficulty difficulty)
    {
        int steps = Mathf.Max(0, result.CompletionPath.Count - 1);
        float multiplier = difficulty == MapAiDifficulty.Easy ? 2.2f
            : difficulty == MapAiDifficulty.Hard ? 1.6f : 1.25f;
        float branchAllowance = difficulty == MapAiDifficulty.Easy
            ? result.DeadEnds.Count * 2f : result.MisleadingCount * 1.5f;
        return Mathf.CeilToInt(steps * multiplier + branchAllowance);
    }

    private static void FindCompletionPath(MapAiMapData map, MapAiAnalysisResult result)
    {
        List<Vector2Int> targets = map.Find('T');
        if (targets.Count == 0)
        {
            result.CanComplete = result.IsReachable;
            result.CompletionPath.AddRange(result.ShortestPath);
            return;
        }

        for (int i = 0; i < result.Paths.Count; i++)
        {
            List<Vector2Int> path = result.Paths[i];
            if (!ContainsAll(path, targets)) continue;
            if (result.CompletionPath.Count == 0 || path.Count < result.CompletionPath.Count)
            {
                result.CompletionPath.Clear();
                result.CompletionPath.AddRange(path);
            }
        }

        result.CanComplete = result.CompletionPath.Count > 0;
        result.CompletionSearchLimited = !result.CanComplete && result.SearchLimited;
        if (!result.CanComplete && !result.CompletionSearchLimited)
        {
            result.IsValid = false;
            result.Issues.Add("Không có đường không lặp ô thu đủ target rồi tới F.");
        }
    }

    private static bool ContainsAll(List<Vector2Int> path, List<Vector2Int> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (!path.Contains(targets[i])) return false;
        }
        return true;
    }
}
