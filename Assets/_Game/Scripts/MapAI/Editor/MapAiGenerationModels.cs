using System;
using UnityEngine;

public enum MapAiDifficulty
{
    Easy,
    Hard,
    SuperHard
}

[Serializable]
public sealed class MapAiGenerationSettings
{
    public int Columns = 6;
    public int Rows = 5;
    public int Seed = 12345;
    public MapAiDifficulty Difficulty = MapAiDifficulty.Easy;
    [Min(0)] public int TargetCount = 1;
    [Range(0f, 0.75f)] public float RockRatio = 0.25f;
    [Range(0f, 0.5f)] public float RotateRatio = 0.03f;
    [Range(0f, 0.5f)] public float StraightOneWayRatio = 0.05f;
    [Range(0f, 0.5f)] public float CornerOneWayRatio = 0.03f;
    [Range(0f, 0.5f)] public float ExtraConnectionChance = 0.18f;
    public MapAiSearchBudget SearchBudget = new MapAiSearchBudget();

    public void ApplyPreset()
    {
        if (Difficulty == MapAiDifficulty.Easy) ApplyEasy();
        else if (Difficulty == MapAiDifficulty.Hard) ApplyHard();
        else ApplySuperHard();
    }

    private void ApplyEasy()
    {
        TargetCount = 1;
        RockRatio = 0.25f;
        RotateRatio = 0.03f;
        StraightOneWayRatio = 0.05f;
        CornerOneWayRatio = 0.03f;
        ExtraConnectionChance = 0.22f;
    }

    private void ApplyHard()
    {
        TargetCount = 2;
        RockRatio = 0.38f;
        RotateRatio = 0.06f;
        StraightOneWayRatio = 0.1f;
        CornerOneWayRatio = 0.08f;
        ExtraConnectionChance = 0.1f;
    }

    private void ApplySuperHard()
    {
        TargetCount = 3;
        RockRatio = 0.48f;
        RotateRatio = 0.1f;
        StraightOneWayRatio = 0.15f;
        CornerOneWayRatio = 0.12f;
        ExtraConnectionChance = 0.03f;
    }
}

public sealed class MapAiGenerationResult
{
    public bool Success { get; }
    public string Message { get; }
    public string[] Rows { get; }
    public MapAiAnalysisResult Analysis { get; }

    public MapAiGenerationResult(bool success, string message,
        string[] rows, MapAiAnalysisResult analysis)
    {
        Success = success;
        Message = message;
        Rows = rows;
        Analysis = analysis;
    }
}
