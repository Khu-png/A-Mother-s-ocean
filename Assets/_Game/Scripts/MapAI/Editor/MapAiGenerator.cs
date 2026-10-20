using System;
using UnityEngine;

public static class MapAiGenerator
{
    public const int VERSION = 2;
    private const int MAX_ATTEMPTS = 24;

    public static MapAiGenerationResult Generate(MapAiMapData current,
        bool[] locks, MapAiGenerationSettings settings)
    {
        string validationIssue = Validate(current, locks, settings);
        if (validationIssue != string.Empty)
            return Failure(current.ToRows(), validationIssue, settings);

        string[] lastRows = current.ToRows();
        string lastIssue = "Không tìm được cấu hình hợp lệ trong ngân sách sinh map.";
        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            int attemptSeed = unchecked(settings.Seed ^ VERSION ^ (attempt * 486187739));
            System.Random random = new System.Random(attemptSeed);
            if (!MapAiTopologyGenerator.TryCreate(current, locks, settings,
                random, out char[,] cells, out System.Collections.Generic.List<Vector2Int> path,
                out lastIssue)) continue;
            if (!MapAiTileDecorator.TryDecorate(cells, path, current, locks,
                settings, random, out lastIssue)) continue;

            lastRows = MapAiTopologyGenerator.ToRows(cells);
            MapAiAnalysisResult analysis = MapAiAnalyzer.Analyze(
                new MapAiMapData(lastRows), settings.SearchBudget, settings.Difficulty);
            if (analysis.IsValid && analysis.CanComplete)
            {
                string message = $"Seed {settings.Seed}, generator v{VERSION}, attempt {attempt + 1}.";
                return new MapAiGenerationResult(true, message, lastRows, analysis);
            }
            lastIssue = analysis.CompletionSearchLimited
                ? "Ngân sách phân tích chưa chứng minh được tuyến hoàn thành."
                : "Map sinh ra không có tuyến hoàn thành hợp lệ.";
        }
        return Failure(lastRows, lastIssue, settings);
    }

    private static string Validate(MapAiMapData current, bool[] locks,
        MapAiGenerationSettings settings)
    {
        if (locks.Length != current.Width * current.Height)
            return "Dữ liệu khóa ô không khớp kích thước document.";
        if (settings.Columns != current.Width || settings.Rows != current.Height)
            return "Hãy resize document theo kích thước đã chọn trước khi sinh map.";
        if (settings.Columns < MapBuilderLimits.MIN_SIZE
            || settings.Columns > MapBuilderLimits.MAX_COLUMNS
            || settings.Rows < MapBuilderLimits.MIN_SIZE
            || settings.Rows > MapBuilderLimits.MAX_ROWS)
            return "Kích thước nằm ngoài giới hạn project hiện tại.";
        int totalCells = settings.Columns * settings.Rows;
        if (settings.TargetCount < 0 || settings.TargetCount > totalCells - 2)
            return "Số target vượt quá số ô có thể sử dụng.";
        if (!IsRatio(settings.RockRatio) || !IsRatio(settings.RotateRatio)
            || !IsRatio(settings.StraightOneWayRatio)
            || !IsRatio(settings.CornerOneWayRatio))
            return "Các tỷ lệ sinh map phải nằm trong khoảng 0% đến 100%.";
        int rockCount = Mathf.RoundToInt(totalCells * settings.RockRatio);
        if (totalCells - rockCount < settings.TargetCount + 2)
            return "Tỷ lệ đá không chừa đủ chỗ cho S, F và target.";
        float specialRatio = settings.RotateRatio + settings.StraightOneWayRatio
            + settings.CornerOneWayRatio;
        return specialRatio <= 1f
            ? string.Empty : "Tổng tỷ lệ ô xoay và one-way không được vượt 100%.";
    }

    private static bool IsRatio(float value)
    {
        return value >= 0f && value <= 1f;
    }

    private static MapAiGenerationResult Failure(string[] rows, string issue,
        MapAiGenerationSettings settings)
    {
        MapAiAnalysisResult analysis = MapAiAnalyzer.Analyze(
            new MapAiMapData(rows), settings.SearchBudget, settings.Difficulty);
        return new MapAiGenerationResult(false, issue, rows, analysis);
    }
}
