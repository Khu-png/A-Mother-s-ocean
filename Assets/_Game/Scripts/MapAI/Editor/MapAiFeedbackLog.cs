using System;
using System.IO;
using UnityEngine;

[Serializable]
public sealed class MapAiFeedbackVote
{
    public int Rating;
    public string Reason;
}

[Serializable]
public sealed class MapAiFeedbackRecord
{
    public string time;
    public int rating;
    public string reason;
    public string[] rows;
    public string difficulty;
    public int seed;
    public int targetCount;
    public bool hasRocks;
    public float rockRatio;
    public bool hasOneWay;
    public float straightOneWayRatio;
    public float cornerOneWayRatio;
    public bool hasRotate;
    public float rotateRatio;
    public bool canComplete;
    public int completionSteps;
    public int decisionPoints;
    public int deadEnds;
    public int completePaths;
    public MapAiLayoutFeatures layout;
    public MapAiRotationMetrics rotation;
}

// Appends the designer's verdict on a map, with the settings and measurements that made it,
// to MapFeedback/feedback.jsonl in the project root (one JSON object per line).
public static class MapAiFeedbackLog
{
    public const string FILE_NAME = "feedback.jsonl";
    public static string FilePath =>
        Path.Combine(Directory.GetCurrentDirectory(), "MapFeedback", FILE_NAME);

    public static string Save(MapAiFeedbackVote vote, string[] rows,
        MapAiGenerationSettings settings, MapAiAnalysisResult analysis)
    {
        MapAiFeedbackRecord record = new MapAiFeedbackRecord
        {
            time = DateTime.Now.ToString("o"),
            rating = vote.Rating,
            reason = vote.Reason,
            rows = rows,
            difficulty = settings.Difficulty.ToString(),
            seed = settings.Seed,
            targetCount = settings.TargetCount,
            hasRocks = settings.HasRocks,
            rockRatio = settings.RockRatio,
            hasOneWay = settings.HasOneWay,
            straightOneWayRatio = settings.StraightOneWayRatio,
            cornerOneWayRatio = settings.CornerOneWayRatio,
            hasRotate = settings.HasRotate,
            rotateRatio = settings.RotateRatio,
            canComplete = analysis.CanComplete,
            completionSteps = analysis.CompletionPath.Count,
            decisionPoints = analysis.DecisionPoints,
            deadEnds = analysis.DeadEnds.Count,
            completePaths = analysis.Paths.Count,
            layout = MapAiLayoutRules.Measure(rows),
            rotation = MapAiRotationPuzzle.Measure(rows, settings.SearchBudget, settings.Difficulty, analysis)
        };
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
        File.AppendAllText(FilePath, JsonUtility.ToJson(record) + "\n");
        return $"Đã ghi phản hồi {(vote.Rating > 0 ? "tốt" : "xấu")} vào MapFeedback/{FILE_NAME}.";
    }
}
