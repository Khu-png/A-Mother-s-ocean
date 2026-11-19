using System.Collections.Generic;
using UnityEngine;

// How much the rotation tiles matter to a map's solution. In the best hand-made levels the
// player must step on the right number of rotators, and routes that look fine on the
// untouched pipes turn out to be traps.
[System.Serializable]
public struct MapAiRotationMetrics
{
    public int RotatorCount;
    public int RotatorsOnRouteMin;
    public int RotatorsOnRouteMax;
    // True when no route completes the map while the pipes never turn.
    public bool RequiresRotation;
    // Routes that work on the untouched pipes but are not real solutions.
    public int DecoyRoutes;
}

public static class MapAiRotationPuzzle
{
    private const int SPARE_ROTATOR_BONUS = 6;
    private const int DECOY_ROUTE_BONUS = 3;
    private const int DECOY_ROUTES_COUNTED = 3;

    public static MapAiRotationMetrics Measure(string[] rows, MapAiSearchBudget budget,
        MapAiDifficulty difficulty, MapAiAnalysisResult analysis)
    {
        MapAiMapData map = new MapAiMapData(rows);
        MapAiRotationMetrics metrics = new MapAiRotationMetrics
        {
            RotatorCount = map.Find('O').Count,
            RotatorsOnRouteMin = int.MaxValue
        };
        HashSet<string> realRoutes = new HashSet<string>();
        foreach (List<Vector2Int> route in analysis.Paths)
        {
            int rotators = CountRotators(map, route);
            metrics.RotatorsOnRouteMin = Mathf.Min(metrics.RotatorsOnRouteMin, rotators);
            metrics.RotatorsOnRouteMax = Mathf.Max(metrics.RotatorsOnRouteMax, rotators);
            realRoutes.Add(string.Join(";", route));
        }
        if (analysis.Paths.Count == 0) metrics.RotatorsOnRouteMin = 0;
        if (metrics.RotatorCount == 0) return metrics;

        // Same map with the rotators turned into plain floor: the pipes never move.
        MapAiAnalysisResult untouched = MapAiAnalyzer.Analyze(
            new MapAiMapData(WithoutRotators(rows)), budget, difficulty);
        metrics.RequiresRotation = !untouched.CanComplete && !untouched.CompletionSearchLimited;
        foreach (List<Vector2Int> route in untouched.Paths)
        {
            if (!realRoutes.Contains(string.Join(";", route))) metrics.DecoyRoutes++;
        }
        return metrics;
    }

    // Extra points for a rotation puzzle that tricks: spare rotators that can be stepped on
    // wrongly, and routes that look right until the pipes turn.
    public static int Bonus(MapAiRotationMetrics metrics)
    {
        int bonus = metrics.RotatorCount > metrics.RotatorsOnRouteMax ? SPARE_ROTATOR_BONUS : 0;
        return bonus + Mathf.Min(metrics.DecoyRoutes, DECOY_ROUTES_COUNTED) * DECOY_ROUTE_BONUS;
    }

    private static int CountRotators(MapAiMapData map, List<Vector2Int> route)
    {
        int count = 0;
        foreach (Vector2Int cell in route)
        {
            if (map.GetCell(cell) == 'O') count++;
        }
        return count;
    }

    private static string[] WithoutRotators(string[] rows)
    {
        string[] copy = new string[rows.Length];
        for (int i = 0; i < rows.Length; i++) copy[i] = rows[i].Replace('O', '.');
        return copy;
    }
}
