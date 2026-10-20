using System;
using UnityEditor;

public static class MapAiEditorSelfTest
{
    [MenuItem("Tools/A Mother's Ocean/Map AI Editor/Run Self Tests")]
    public static void Run()
    {
        RunAll();
        UnityEngine.Debug.Log("Map AI Editor self tests passed.");
    }

    public static void RunBatch()
    {
        RunAll();
    }

    private static void RunAll()
    {
        TestReachableMap();
        TestBlockedMap();
        TestDirectionalRule();
        TestLimitedSearch();
        TestDeadEnd();
        TestTargetCompletion();
        TestTargetsRequireRevisit();
        TestSupportedSizes();
        TestDeterministicGeneration();
        TestConfiguredContent();
    }

    private static void TestReachableMap()
    {
        MapAiAnalysisResult result = Analyze(new[] { "S..F" });
        Require(result.IsReachable, "Reachable map was rejected.");
        Require(result.ShortestPath.Count == 4, "Shortest path length is incorrect.");
    }

    private static void TestBlockedMap()
    {
        MapAiAnalysisResult result = Analyze(new[] { "SXXF" });
        Require(!result.IsReachable, "Blocked map was accepted.");
    }

    private static void TestDirectionalRule()
    {
        MapAiAnalysisResult result = Analyze(new[] { "S|F" });
        Require(!result.IsReachable, "Horizontal movement through a vertical path was accepted.");
    }

    private static void TestLimitedSearch()
    {
        MapAiSearchBudget budget = new MapAiSearchBudget
        {
            MaximumPaths = 1,
            MaximumPathLength = 10,
            MaximumExpansions = 100
        };
        MapAiAnalysisResult result = MapAiAnalyzer.Analyze(
            new MapAiMapData(new[] { "S.", ".F" }), budget, MapAiDifficulty.Easy);
        Require(result.SearchLimited, "Limited path search was reported as complete.");
    }

    private static void TestDeadEnd()
    {
        string[] rows = { "XXXXX", "XS.FX", "XX.XX", "XX.XX", "XXXXX" };
        MapAiAnalysisResult result = Analyze(rows);
        Require(result.DeadEnds.Count == 1, "Dead-end branch count is incorrect.");
        Require(result.DeadEnds[0].Cells.Count == 3, "Dead-end depth is incorrect.");
    }

    private static void TestTargetCompletion()
    {
        MapAiAnalysisResult result = Analyze(new[] { "ST.F" });
        Require(result.CanComplete, "Valid target route was rejected.");
        Require(result.CompletionPath.Count == 4, "Completion route length is incorrect.");
    }

    private static void TestTargetsRequireRevisit()
    {
        string[] rows = { "XXTXX", "XX.XX", "XTSFX", "XXXXX" };
        MapAiAnalysisResult result = Analyze(rows);
        Require(!result.CanComplete, "Route requiring a revisited cell was accepted.");
    }

    private static void TestDeterministicGeneration()
    {
        string[] rows =
        {
            "........", ".O......", "........", "........",
            "........", "........", "........", "........"
        };
        MapAiMapData current = new MapAiMapData(rows, 4);
        MapAiGenerationSettings settings = new MapAiGenerationSettings
        {
            Columns = 8,
            Rows = 8,
            Seed = 2468
        };
        bool[] locks = new bool[64];
        locks[9] = true;
        MapAiGenerationResult first = MapAiGenerator.Generate(current, locks, settings);
        MapAiGenerationResult second = MapAiGenerator.Generate(current, locks, settings);
        Require(first.Success && second.Success, "Deterministic generation failed.");
        Require(string.Join("\n", first.Rows) == string.Join("\n", second.Rows),
            "Same seed produced different maps.");
        Require(first.Rows[1][1] == 'O', "Generation changed a locked cell.");
    }

    private static void TestSupportedSizes()
    {
        int[,] sizes = { { 5, 5 }, { 8, 8 }, { 10, 10 }, { 7, 4 } };
        for (int i = 0; i < sizes.GetLength(0); i++)
        {
            int width = sizes[i, 0];
            int height = sizes[i, 1];
            MapAiGenerationSettings settings = new MapAiGenerationSettings
            {
                Columns = width,
                Rows = height,
                Seed = 9000 + i
            };
            MapAiGenerationResult generated = MapAiGenerator.Generate(
                new MapAiMapData(EmptyRows(width, height)),
                new bool[width * height], settings);
            Require(generated.Success, $"Generation failed for {width}x{height}.");
        }
    }

    private static void TestConfiguredContent()
    {
        const int width = 10;
        const int height = 10;
        MapAiGenerationSettings settings = new MapAiGenerationSettings
        {
            Columns = width,
            Rows = height,
            Seed = 31415,
            TargetCount = 3,
            RockRatio = 0.3f,
            RotateRatio = 0.05f,
            StraightOneWayRatio = 0.1f,
            CornerOneWayRatio = 0.08f,
            ExtraConnectionChance = 0.15f
        };
        MapAiGenerationResult generated = MapAiGenerator.Generate(
            new MapAiMapData(EmptyRows(width, height)), new bool[width * height], settings);
        Require(generated.Success, "Configured content generation failed.");
        int eligible = width * height - Count(generated.Rows, 'X')
            - Count(generated.Rows, 'S') - Count(generated.Rows, 'F')
            - Count(generated.Rows, 'T');
        Require(Count(generated.Rows, 'T') == settings.TargetCount,
            "Generated target count does not match the setting.");
        Require(Count(generated.Rows, 'X') == UnityEngine.Mathf.RoundToInt(
            width * height * settings.RockRatio), "Generated rock ratio is incorrect.");
        Require(Count(generated.Rows, 'O') == UnityEngine.Mathf.RoundToInt(
            eligible * settings.RotateRatio), "Generated rotate ratio is incorrect.");
        Require(CountStraight(generated.Rows) == UnityEngine.Mathf.RoundToInt(
            eligible * settings.StraightOneWayRatio), "Generated straight ratio is incorrect.");
        Require(CountCorner(generated.Rows) == UnityEngine.Mathf.RoundToInt(
            eligible * settings.CornerOneWayRatio), "Generated corner ratio is incorrect.");
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

    private static string[] EmptyRows(int width, int height)
    {
        string[] rows = new string[height];
        for (int y = 0; y < height; y++) rows[y] = new string('.', width);
        return rows;
    }

    private static MapAiAnalysisResult Analyze(string[] rows)
    {
        return MapAiAnalyzer.Analyze(new MapAiMapData(rows),
            new MapAiSearchBudget(), MapAiDifficulty.Easy);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
