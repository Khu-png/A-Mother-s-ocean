using System;

public sealed class MapAiToolSnapshot
{
    public MapAiMapData Map { get; }
    public MapAiAnalysisResult Analysis { get; }

    public MapAiToolSnapshot(MapAiMapData map, MapAiAnalysisResult analysis)
    {
        Map = map;
        Analysis = analysis;
    }
}

public sealed class MapAiProposal
{
    public int BaseVersion { get; }
    public string[] Rows { get; }
    public string Explanation { get; }

    public MapAiProposal(int baseVersion, string[] rows, string explanation)
    {
        BaseVersion = baseVersion;
        Rows = rows;
        Explanation = explanation;
    }
}

public interface IMapAiEditorProvider
{
    string DisplayName { get; }
    MapAiProposal Propose(string request, MapAiToolSnapshot snapshot);
}

public static class MapAiProviderRegistry
{
    public static IMapAiEditorProvider Provider { get; set; }
}

public static class MapAiToolApi
{
    public static MapAiToolSnapshot ReadAndAnalyze(MapAiMapData map,
        MapAiSearchBudget budget, MapAiDifficulty difficulty)
    {
        MapAiAnalysisResult analysis = MapAiAnalyzer.Analyze(map, budget, difficulty);
        return new MapAiToolSnapshot(map, analysis);
    }

    public static MapAiGenerationResult Generate(MapAiMapData current,
        bool[] locks, MapAiGenerationSettings settings)
    {
        return MapAiGenerator.Generate(current, locks, settings);
    }

    public static bool MatchesVersion(MapAiProposal proposal, MapAiMapData current)
    {
        return proposal.BaseVersion == current.Version;
    }
}
