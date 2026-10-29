using System;

public static class MapAiFocusGenerationSelfTest
{
    public static void Run()
    {
        TestNearWinDeadEnd();
        TestFocus(MapAiPuzzleFocus.RotationTiming, 17011);
        TestFocus(MapAiPuzzleFocus.GlobalRotation, 17012);
    }

    private static void TestNearWinDeadEnd()
    {
        string[] rows =
        {
            "..SX.",
            ".F..X",
            "T....",
            ".T...",
            "..X.."
        };
        MapAiSearchBudget budget = new MapAiSearchBudget { MaximumPaths = 1 };
        MapAiAnalysisResult result = MapAiAnalyzer.Analyze(
            new MapAiMapData(rows), budget, MapAiDifficulty.Easy);
        Require(result.CanComplete, "Map with a valid route was rejected.");
        Require(result.SearchLimited, "Additional completion paths were not reported.");
        Require(result.NearWinDeadEndCount > 0,
            "A route collecting all but one target and getting stuck was not detected.");
        Require(result.LongestNearWinDeadEndPath.Count > 0,
            "The longest near-win dead-end route was not retained.");
        UnityEngine.Vector2Int lastCell = result.LongestNearWinDeadEndPath[
            result.LongestNearWinDeadEndPath.Count - 1];
        Require(lastCell != result.Finish,
            "A successful route was incorrectly marked as a near-win dead end.");
    }

    private static void TestFocus(MapAiPuzzleFocus focus, int seed)
    {
        const int size = 10;
        MapAiGenerationSettings settings = new MapAiGenerationSettings
        {
            Columns = size,
            Rows = size,
            Seed = seed,
            Difficulty = MapAiDifficulty.Hard,
            PuzzleFocus = focus,
            TargetCount = 2,
            RockRatio = 0.25f,
            RotateRatio = 0.08f,
            StraightOneWayRatio = 0.16f,
            CornerOneWayRatio = 0.14f,
            ExtraConnectionChance = 0.35f
        };
        MapAiGenerationResult result = MapAiGenerator.Generate(
            new MapAiMapData(EmptyRows(size)), new bool[size * size], settings);
        Require(result.Success, $"Generation failed for {focus}: {result.Message}");

        int eligible = size * size - Count(result.Rows, 'X')
            - Count(result.Rows, 'S') - Count(result.Rows, 'F') - Count(result.Rows, 'T');
        Require(Count(result.Rows, 'O') == UnityEngine.Mathf.RoundToInt(
            eligible * settings.RotateRatio), $"Rotate ratio changed for {focus}.");
        Require(CountStraight(result.Rows) == UnityEngine.Mathf.RoundToInt(
            eligible * settings.StraightOneWayRatio), $"Straight ratio changed for {focus}.");
        Require(CountCorner(result.Rows) == UnityEngine.Mathf.RoundToInt(
            eligible * settings.CornerOneWayRatio), $"Corner ratio changed for {focus}.");
    }

    private static string[] EmptyRows(int size)
    {
        string[] rows = new string[size];
        for (int y = 0; y < size; y++) rows[y] = new string('.', size);
        return rows;
    }

    private static int Count(string[] rows, char code)
    {
        int count = 0;
        for (int y = 0; y < rows.Length; y++)
        for (int x = 0; x < rows[y].Length; x++)
            if (rows[y][x] == code) count++;
        return count;
    }

    private static int CountStraight(string[] rows)
    {
        return Count(rows, '|') + Count(rows, '-');
    }

    private static int CountCorner(string[] rows)
    {
        return Count(rows, 'A') + Count(rows, 'B')
            + Count(rows, 'C') + Count(rows, 'E');
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
