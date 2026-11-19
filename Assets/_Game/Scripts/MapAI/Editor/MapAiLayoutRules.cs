using System;
using UnityEngine;

// Measurements of how a map looks (not how hard it is). Also written to the feedback log
// so later work can learn from them.
[Serializable]
public struct MapAiLayoutFeatures
{
    public int CellCount;
    public int FullRockLines;
    public int RockEdgeLines;
    public int AdjacentRotators;
    public int AdjacentSameCorners;
    public int AdjacentTargets;
    public int MaxTargetsInLine;
}

// Layout taste turned into rules: some reject a map outright, the rest cost score.
public static class MapAiLayoutRules
{
    private const float EDGE_ROCK_SHARE = 0.8f;
    private const int EDGE_PENALTY = 6;
    private const int CORNER_PAIR_PENALTY = 3;
    private const int ADJACENT_TARGET_PENALTY = 6;
    private const int LINED_UP_TARGET_PENALTY = 4;
    private const int SHORT_ROUTE_PENALTY = 2;
    private const int TARGETS_ALLOWED_IN_LINE = 2;

    public static MapAiLayoutFeatures Measure(string[] rows)
    {
        MapAiMapData map = new MapAiMapData(rows);
        MapAiLayoutFeatures features = new MapAiLayoutFeatures { CellCount = map.Width * map.Height };
        MeasureLines(map, ref features);
        MeasureNeighbours(map, ref features);
        return features;
    }

    // Reasons a map must be thrown away; empty when the layout is acceptable.
    public static string HardIssue(string[] rows)
    {
        MapAiLayoutFeatures features = Measure(rows);
        if (features.FullRockLines > 0) return "Bản đồ có cả một hàng hoặc cột toàn đá.";
        return string.Empty;
    }

    // Points to subtract from a candidate's score for layouts that look off.
    public static int Penalty(string[] rows, MapAiAnalysisResult analysis,
        MapAiDifficulty difficulty)
    {
        MapAiLayoutFeatures features = Measure(rows);
        int penalty = features.RockEdgeLines * EDGE_PENALTY
            + features.AdjacentSameCorners * CORNER_PAIR_PENALTY
            + features.AdjacentTargets * ADJACENT_TARGET_PENALTY
            + Mathf.Max(0, features.MaxTargetsInLine - TARGETS_ALLOWED_IN_LINE) * LINED_UP_TARGET_PENALTY;
        float routeShare = difficulty == MapAiDifficulty.Easy ? 0.2f
            : difficulty == MapAiDifficulty.Hard ? 0.3f : 0.4f;
        int wantedSteps = Mathf.RoundToInt(features.CellCount * routeShare);
        return penalty + Mathf.Max(0, wantedSteps - analysis.CompletionPath.Count) * SHORT_ROUTE_PENALTY;
    }

    private static void MeasureLines(MapAiMapData map, ref MapAiLayoutFeatures features)
    {
        for (int y = 0; y < map.Height; y++)
        {
            int rocks = 0;
            int targets = 0;
            for (int x = 0; x < map.Width; x++)
            {
                char code = map.GetCell(new Vector2Int(x, y));
                if (MapCode.IsBlock(code)) rocks++;
                if (MapCode.IsTarget(code)) targets++;
            }
            AddLine(ref features, rocks, targets, map.Width, y == 0 || y == map.Height - 1);
        }
        for (int x = 0; x < map.Width; x++)
        {
            int rocks = 0;
            int targets = 0;
            for (int y = 0; y < map.Height; y++)
            {
                char code = map.GetCell(new Vector2Int(x, y));
                if (MapCode.IsBlock(code)) rocks++;
                if (MapCode.IsTarget(code)) targets++;
            }
            AddLine(ref features, rocks, targets, map.Height, x == 0 || x == map.Width - 1);
        }
    }

    private static void AddLine(ref MapAiLayoutFeatures features, int rocks, int targets,
        int length, bool isEdge)
    {
        if (length >= 3 && rocks == length) features.FullRockLines++;
        else if (isEdge && length >= 3 && rocks >= length * EDGE_ROCK_SHARE) features.RockEdgeLines++;
        features.MaxTargetsInLine = Mathf.Max(features.MaxTargetsInLine, targets);
    }

    private static void MeasureNeighbours(MapAiMapData map, ref MapAiLayoutFeatures features)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                char code = map.GetCell(new Vector2Int(x, y));
                if (x + 1 < map.Width) CountPair(code, map.GetCell(new Vector2Int(x + 1, y)), ref features);
                if (y + 1 < map.Height) CountPair(code, map.GetCell(new Vector2Int(x, y + 1)), ref features);
            }
        }
    }

    private static void CountPair(char first, char second, ref MapAiLayoutFeatures features)
    {
        if (MapCode.IsRotator(first) && MapCode.IsRotator(second)) features.AdjacentRotators++;
        if (MapCode.IsTarget(first) && MapCode.IsTarget(second)) features.AdjacentTargets++;
        if (first == second && MapCode.IsOneWay(first)
            && MapCode.PathKind(first) == OneWayPathKind.Corner) features.AdjacentSameCorners++;
    }
}
