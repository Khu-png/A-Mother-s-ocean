using System;
using UnityEngine;

public static class MapAiGenerator
{
    public const int VERSION = 3;
    private const int MAX_ATTEMPTS = 8;

    public static MapAiGenerationResult Generate(MapAiMapData current,
        bool[] locks, MapAiGenerationSettings settings)
    {
        string validationIssue = Validate(current, locks, settings);
        if (validationIssue != string.Empty)
            return Failure(current.ToRows(), validationIssue, settings);

        string[] lastRows = current.ToRows();
        string lastIssue = "Không tìm được cấu hình hợp lệ trong ngân sách sinh map.";
        bool foundCandidate = false;
        int bestScore = int.MinValue;
        MapAiAnalysisResult bestAnalysis = new MapAiAnalysisResult();
        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            int attemptSeed = unchecked(settings.Seed ^ VERSION ^ (attempt * 486187739));
            System.Random random = new System.Random(attemptSeed);
            if (!MapAiTopologyGenerator.TryCreate(current, locks, settings,
                random, out char[,] cells, out System.Collections.Generic.List<Vector2Int> path,
                out lastIssue)) continue;
            if (!ApplyPuzzleFocus(cells, path, current, locks,
                settings.PuzzleFocus, out lastIssue)) continue;
            if (!MapAiTileDecorator.TryDecorate(cells, path, current, locks,
                settings, random, out lastIssue)) continue;

            string[] candidateRows = MapAiTopologyGenerator.ToRows(cells);
            MapAiAnalysisResult analysis = MapAiAnalyzer.Analyze(
                new MapAiMapData(candidateRows), settings.SearchBudget, settings.Difficulty);
            if (!analysis.IsValid || !analysis.CanComplete)
            {
                lastIssue = analysis.CompletionSearchLimited
                    ? "Ngân sách phân tích chưa chứng minh được tuyến hoàn thành."
                    : "Map sinh ra không có tuyến hoàn thành hợp lệ.";
                continue;
            }
            int score = ScoreCandidate(analysis, settings.Difficulty);
            if (foundCandidate && score <= bestScore) continue;
            foundCandidate = true;
            bestScore = score;
            lastRows = candidateRows;
            bestAnalysis = analysis;
        }
        if (foundCandidate)
            return new MapAiGenerationResult(true,
                $"Hạt giống {settings.Seed}, bộ tạo v{VERSION}; điểm tốt nhất {bestScore}.",
                lastRows, bestAnalysis);
        return Failure(lastRows, lastIssue, settings);
    }

    private static bool ApplyPuzzleFocus(char[,] cells,
        System.Collections.Generic.List<Vector2Int> path, MapAiMapData current,
        bool[] locks, MapAiPuzzleFocus focus, out string issue)
    {
        issue = string.Empty;
        if (focus == MapAiPuzzleFocus.TargetOrder) return true;
        int rotatorIndex = FindRouteRotator(cells, path, current, locks);
        if (rotatorIndex < 0)
        {
            issue = "Không có ô hợp lệ để đặt nút xoay trên tuyến chính.";
            return false;
        }
        Vector2Int rotator = path[rotatorIndex];
        cells[rotator.x, rotator.y] = 'O';
        if (!SetRotatedRoutePipes(cells, path, current, locks, rotatorIndex))
        {
            issue = "Không có pipe phù hợp để tạo thử thách thời điểm xoay.";
            return false;
        }
        if (focus != MapAiPuzzleFocus.GlobalRotation) return true;
        if (SetGlobalRotationPipe(cells, path, current, locks)) return true;
        issue = "Không tìm thấy ngã ba phù hợp để thể hiện ảnh hưởng xoay toàn cục.";
        return false;
    }

    private static int FindRouteRotator(char[,] cells,
        System.Collections.Generic.List<Vector2Int> path, MapAiMapData current,
        bool[] locks)
    {
        for (int i = path.Count / 2; i < path.Count - 1; i++)
        {
            Vector2Int cell = path[i];
            if (MapCode.IsRotator(cells[cell.x, cell.y])) return i;
            if (cells[cell.x, cell.y] == '.'
                && !MapAiTopologyGenerator.IsLocked(cell, locks, current)) return i;
        }
        return -1;
    }

    private static bool SetRotatedRoutePipes(char[,] cells,
        System.Collections.Generic.List<Vector2Int> path, MapAiMapData current,
        bool[] locks, int rotatorIndex)
    {
        for (int i = rotatorIndex + 1; i < path.Count - 1; i++)
        {
            Vector2Int cell = path[i];
            if (MapAiTopologyGenerator.IsLocked(cell, locks, current)) continue;
            if (cells[cell.x, cell.y] != '.' && !MapCode.IsOneWay(cells[cell.x, cell.y]))
                continue;
            char routeCode = MapAiTileDecorator.RouteCode(path[i - 1],
                cell, path[i + 1], false);
            if (routeCode == '\0') routeCode = MapAiTileDecorator.RouteCode(
                path[i - 1], cell, path[i + 1], true);
            if (routeCode == '\0') continue;
            cells[cell.x, cell.y] = InverseRotation(routeCode);
            return true;
        }
        return false;
    }

    private static bool SetGlobalRotationPipe(char[,] cells,
        System.Collections.Generic.List<Vector2Int> path, MapAiMapData current,
        bool[] locks)
    {
        System.Collections.Generic.HashSet<Vector2Int> route =
            new System.Collections.Generic.HashSet<Vector2Int>(path);
        for (int y = 1; y < cells.GetLength(1) - 1; y++)
        for (int x = 1; x < cells.GetLength(0) - 1; x++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            if (route.Contains(cell) || MapCode.IsBlock(cells[x, y])
                || MapCode.IsStart(cells[x, y]) || MapCode.IsFinish(cells[x, y])
                || MapCode.IsTarget(cells[x, y]) || MapCode.IsRotator(cells[x, y])
                || MapAiTopologyGenerator.IsLocked(cell, locks, current)) continue;
            char pipeCode = PipeCodeAtJunction(cells, x, y);
            if (pipeCode == '\0') continue;
            cells[x, y] = pipeCode;
            return true;
        }
        return false;
    }

    private static char PipeCodeAtJunction(char[,] cells, int x, int y)
    {
        bool left = cells[x - 1, y] != 'X';
        bool right = cells[x + 1, y] != 'X';
        bool up = cells[x, y - 1] != 'X';
        bool down = cells[x, y + 1] != 'X';
        if (left && right) return '-';
        if (up && down) return '|';
        if (left && down) return 'A';
        if (left && up) return 'B';
        if (up && right) return 'C';
        if (right && down) return 'E';
        return '\0';
    }

    private static char InverseRotation(char code)
    {
        if (code == '|') return '-';
        if (code == '-') return '|';
        if (code == 'A') return 'B';
        if (code == 'B') return 'C';
        if (code == 'C') return 'E';
        return 'A';
    }

    private static int ScoreCandidate(MapAiAnalysisResult analysis,
        MapAiDifficulty difficulty)
    {
        int complexity = analysis.DecisionPoints * 4
            + analysis.MisleadingCount * 5
            + Mathf.Min(analysis.DeadEnds.Count, 8) * 2
            + Mathf.Min(analysis.CompletionPath.Count / 3, 6);
        int ideal = difficulty == MapAiDifficulty.Easy ? 5
            : difficulty == MapAiDifficulty.Hard ? 18 : 30;
        int score = -Mathf.Abs(complexity - ideal);
        if (difficulty != MapAiDifficulty.Easy)
            score -= Mathf.Min(Mathf.Max(0, analysis.Paths.Count - 1), 10) * 2;
        if (analysis.SearchLimited) score--;
        return score;
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
