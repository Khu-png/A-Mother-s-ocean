using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapAiDifficulty
{
    Easy,
    Hard,
    SuperHard
}

public enum MapAiPuzzleFocus
{
    TargetOrder,
    RotationTiming,
    GlobalRotation
}

[Serializable]
public sealed class MapAiGenerationSettings
{
    public int Columns = 6;
    public int Rows = 5;
    public int Seed = 12345;
    public MapAiDifficulty Difficulty = MapAiDifficulty.Easy;
    public MapAiPuzzleFocus PuzzleFocus = MapAiPuzzleFocus.TargetOrder;
    [Min(0)] public int TargetCount = 2;
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
        ApplyFocusPreset();
    }

    public MapAiGenerationSettings CreateForSeed(int seed)
    {
        return new MapAiGenerationSettings
        {
            Columns = Columns,
            Rows = Rows,
            Seed = seed,
            Difficulty = Difficulty,
            PuzzleFocus = PuzzleFocus,
            TargetCount = TargetCount,
            RockRatio = RockRatio,
            RotateRatio = RotateRatio,
            StraightOneWayRatio = StraightOneWayRatio,
            CornerOneWayRatio = CornerOneWayRatio,
            ExtraConnectionChance = ExtraConnectionChance,
            SearchBudget = new MapAiSearchBudget
            {
                MaximumPaths = SearchBudget.MaximumPaths,
                MaximumPathLength = SearchBudget.MaximumPathLength,
                MaximumExpansions = SearchBudget.MaximumExpansions
            }
        };
    }

    private void ApplyEasy()
    {
        TargetCount = 1;
        RockRatio = 0.22f;
        RotateRatio = 0.03f;
        StraightOneWayRatio = 0.05f;
        CornerOneWayRatio = 0.03f;
        ExtraConnectionChance = 0.12f;
    }

    private void ApplyHard()
    {
        TargetCount = 2;
        RockRatio = 0.32f;
        RotateRatio = 0.08f;
        StraightOneWayRatio = 0.12f;
        CornerOneWayRatio = 0.1f;
        ExtraConnectionChance = 0.2f;
    }

    private void ApplySuperHard()
    {
        TargetCount = 3;
        RockRatio = 0.38f;
        RotateRatio = 0.12f;
        StraightOneWayRatio = 0.16f;
        CornerOneWayRatio = 0.14f;
        ExtraConnectionChance = 0.3f;
    }

    private void ApplyFocusPreset()
    {
        if (PuzzleFocus == MapAiPuzzleFocus.TargetOrder)
            TargetCount = Difficulty == MapAiDifficulty.Easy ? 2
                : Difficulty == MapAiDifficulty.Hard ? 3 : 4;
        else if (PuzzleFocus == MapAiPuzzleFocus.RotationTiming)
        {
            TargetCount = Mathf.Max(1, Difficulty == MapAiDifficulty.Easy ? 1 : TargetCount);
            RotateRatio = Mathf.Max(RotateRatio, 0.08f);
            StraightOneWayRatio = Mathf.Max(StraightOneWayRatio, 0.12f);
            ExtraConnectionChance = Mathf.Max(ExtraConnectionChance, 0.18f);
        }
        else
        {
            TargetCount = Mathf.Max(TargetCount, 2);
            RotateRatio = Mathf.Max(RotateRatio, 0.08f);
            StraightOneWayRatio = Mathf.Max(StraightOneWayRatio, 0.12f);
            ExtraConnectionChance = Mathf.Max(ExtraConnectionChance, 0.35f);
        }
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

[Serializable]
public sealed class MapAiPreviewSample
{
    public string[] Rows;
    public string Message;

    public MapAiPreviewSample(string[] rows, string message)
    {
        Rows = rows;
        Message = message;
    }
}

[Serializable]
public sealed class MapAiPreviewCollection
{
    private const int SEED_STEP = 104729;
    private const int MAX_ATTEMPTS_PER_SAMPLE = 4;
    [SerializeField] private List<MapAiPreviewSample> samples =
        new List<MapAiPreviewSample>();
    [SerializeField] private int selectedIndex;

    public int Count => samples.Count;
    public int SelectedIndex => selectedIndex;
    public MapAiPreviewSample Current => samples[selectedIndex];

    public string Generate(MapAiMapData current, bool[] locks,
        MapAiGenerationSettings settings, int requestedCount)
    {
        Clear();
        HashSet<string> uniqueMaps = new HashSet<string>();
        string lastIssue = "No unique valid maps were generated.";
        int maximumAttempts = requestedCount * MAX_ATTEMPTS_PER_SAMPLE;
        for (int attempt = 0; attempt < maximumAttempts; attempt++)
        {
            int seed = unchecked(settings.Seed + attempt * SEED_STEP);
            MapAiGenerationResult result = MapAiToolApi.Generate(
                current, locks, settings.CreateForSeed(seed));
            if (!result.Success)
            {
                lastIssue = result.Message;
                continue;
            }

            string key = string.Join("\n", result.Rows);
            if (!uniqueMaps.Add(key)) continue;
            samples.Add(new MapAiPreviewSample(result.Rows, result.Message));
            if (samples.Count == requestedCount) break;
        }

        if (samples.Count == 0) return $"No valid samples generated. {lastIssue}";
        return $"Generated {samples.Count} of {requestedCount} unique valid samples.";
    }

    public void SetSingle(string[] rows, string message)
    {
        Clear();
        samples.Add(new MapAiPreviewSample(rows, message));
    }

    public bool Move(int direction)
    {
        int nextIndex = selectedIndex + direction;
        if (nextIndex < 0 || nextIndex >= samples.Count) return false;
        selectedIndex = nextIndex;
        return true;
    }

    public void Clear()
    {
        samples.Clear();
        selectedIndex = 0;
    }
}
