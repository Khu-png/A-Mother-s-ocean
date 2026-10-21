using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class MapAiSearchBudget
{
    [Min(1)] public int MaximumPaths = 100;
    [Min(2)] public int MaximumPathLength = 100;
    [Min(100)] public int MaximumExpansions = 50000;
}

public sealed class MapAiBranch
{
    public Vector2Int Leaf { get; }
    public Vector2Int Junction { get; }
    public List<Vector2Int> Cells { get; }
    public bool IsMisleadingCandidate { get; }

    public MapAiBranch(Vector2Int leaf, Vector2Int junction,
        List<Vector2Int> cells, bool isMisleadingCandidate)
    {
        Leaf = leaf;
        Junction = junction;
        Cells = cells;
        IsMisleadingCandidate = isMisleadingCandidate;
    }
}

public sealed class MapAiAnalysisResult
{
    public readonly List<string> Issues = new List<string>();
    public readonly List<Vector2Int> ShortestPath = new List<Vector2Int>();
    public readonly List<Vector2Int> CompletionPath = new List<Vector2Int>();
    public readonly List<List<Vector2Int>> Paths = new List<List<Vector2Int>>();
    public readonly List<MapAiBranch> DeadEnds = new List<MapAiBranch>();
    public readonly Dictionary<Vector2Int, int> DistanceToFinish = new Dictionary<Vector2Int, int>();
    public readonly HashSet<Vector2Int> ReachableFromStart = new HashSet<Vector2Int>();

    public bool IsValid { get; set; }
    public bool IsReachable { get; set; }
    public bool CanComplete { get; set; }
    public bool CompletionSearchLimited { get; set; }
    public bool SearchLimited { get; set; }
    public int SearchExpansions { get; set; }
    public int DecisionPoints { get; set; }
    public int SuggestedSeconds { get; set; }
    public Vector2Int Start { get; set; }
    public Vector2Int Finish { get; set; }

    public int MisleadingCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < DeadEnds.Count; i++)
            {
                if (DeadEnds[i].IsMisleadingCandidate) count++;
            }
            return count;
        }
    }
}
