using UnityEngine;

// What a map's search tree says about its difficulty, turned into a score.
// Five parts, each earning points per unit with a ceiling, so the best tricks can push the
// total past 100. Slopes are read off the hand-made levels (level 3 has about 12 dead ends
// per solution, 3 solutions, traps as deep as the solution and 3 required rotators):
//   dead ends per solution 1.6 points each (max 60), depth of the deepest trap relative to
//   the solution 10 points per 1.0 (max 20), few solutions (max 15), rotators the solution
//   must step on 5 points each (max 25), shortest route 1/3 point per step (max 15).
// Bands: Easy below 50, Hard 50 to 74, Super Hard from 75 (scores can still run past 100).
// Super Hard has no hand-made example yet; it is the stretch beyond level 3.
public struct MapAiDifficultyReport
{
    public int Solutions;
    public int ShortestSteps;
    public float TrapRatio;
    public float DeepTrapShare;
    public int RotatorsNeeded;
}

public struct MapAiDifficultyScore
{
    public float Traps;
    public float Depth;
    public float Uniqueness;
    public float Rotation;
    public float Length;

    public float Total => Traps + Depth + Uniqueness + Rotation + Length;
}

public static class MapAiDifficultyProfile
{
    public const float HARD_FROM = 50f;
    public const float SUPER_HARD_FROM = 75f;
    private const int OPEN_MAP_SOLUTIONS = 10;
    private const float OPEN_MAP_DEPTH_SHARE = 0.25f;
    public const float TRAPS_MAX = 60f;
    public const float DEPTH_MAX = 20f;
    public const float UNIQUENESS_MAX = 15f;
    public const float ROTATION_MAX = 25f;
    public const float LENGTH_MAX = 15f;
    private const float POINTS_PER_TRAP = 1.6f;
    private const float POINTS_PER_DEPTH_UNIT = 10f;
    private const float POINTS_PER_ROTATOR = 5f;
    private const float POINTS_PER_STEP = 1f / 3f;
    private const float BAND_REACH = 1000f;
    private static readonly float[] UNIQUENESS_BY_SOLUTIONS = { 0f, 1f, 0.8f, 0.6f, 0.4f, 0.4f };

    public static MapAiDifficultyReport Measure(MapAiAnalysisResult analysis)
    {
        MapAiDifficultyReport report = new MapAiDifficultyReport
        {
            Solutions = analysis.SolutionCount,
            RotatorsNeeded = analysis.RotatorsOnSolution
        };
        if (report.Solutions == 0) return report;
        report.ShortestSteps = int.MaxValue;
        foreach (System.Collections.Generic.List<Vector2Int> route in analysis.Paths)
            report.ShortestSteps = Mathf.Min(report.ShortestSteps, route.Count - 1);
        report.TrapRatio = analysis.DeadEndLeaves / (float)report.Solutions;
        report.DeepTrapShare = analysis.DeepestDeadEnd / (float)Mathf.Max(1, report.ShortestSteps);
        return report;
    }

    public static MapAiDifficultyScore Score(MapAiDifficultyReport report)
    {
        if (report.Solutions == 0) return default;
        return new MapAiDifficultyScore
        {
            Traps = Mathf.Min(TRAPS_MAX, report.TrapRatio * POINTS_PER_TRAP),
            Depth = Mathf.Min(DEPTH_MAX, report.DeepTrapShare * POINTS_PER_DEPTH_UNIT) * DepthWeight(report.Solutions),
            Uniqueness = UNIQUENESS_MAX * UniquenessShare(report.Solutions),
            Rotation = Mathf.Min(ROTATION_MAX, report.RotatorsNeeded * POINTS_PER_ROTATOR),
            Length = Mathf.Min(LENGTH_MAX, report.ShortestSteps * POINTS_PER_STEP)
        };
    }

    public static MapAiDifficulty Classify(float score)
    {
        if (score >= SUPER_HARD_FROM) return MapAiDifficulty.SuperHard;
        return score >= HARD_FROM ? MapAiDifficulty.Hard : MapAiDifficulty.Easy;
    }

    // 0 while the score is inside the tier's band; grows with every point outside it.
    public static float Distance(float score, MapAiDifficulty tier)
    {
        float from = tier == MapAiDifficulty.Easy ? 0f
            : tier == MapAiDifficulty.Hard ? HARD_FROM : SUPER_HARD_FROM;
        float until = tier == MapAiDifficulty.Easy ? HARD_FROM
            : tier == MapAiDifficulty.Hard ? SUPER_HARD_FROM : BAND_REACH;
        return Mathf.Max(0f, from - score) + Mathf.Max(0f, score - until);
    }

    // Deep dead ends are cheap on an open map with many solutions, so they count for less there.
    private static float DepthWeight(int solutions)
    {
        return solutions <= OPEN_MAP_SOLUTIONS ? 1f : OPEN_MAP_DEPTH_SHARE;
    }

    private static float UniquenessShare(int solutions)
    {
        if (solutions < UNIQUENESS_BY_SOLUTIONS.Length) return UNIQUENESS_BY_SOLUTIONS[solutions];
        return solutions <= 10 ? 0.2f : 0f;
    }
}
