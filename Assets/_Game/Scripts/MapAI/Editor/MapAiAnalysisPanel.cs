using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public sealed class MapAiAnalysisPanel
{
    [SerializeField] private Vector2 scroll;
    [SerializeField] private bool showShortest = true;
    [SerializeField] private bool showDeadEnds = true;
    [SerializeField] private bool showMisleading = true;
    [SerializeField] private int selectedBranch = -1;
    [SerializeField] private int selectedPath = -1;
    [SerializeField] private string aiRequest = string.Empty;

    public bool ShowShortest => showShortest;
    public bool ShowDeadEnds => showDeadEnds;
    public bool ShowMisleading => showMisleading;
    public int SelectedBranch => selectedBranch;
    public int SelectedPath => selectedPath;
    public string AiRequest => aiRequest;

    public bool Draw(Rect area, MapAiAnalysisResult result)
    {
        bool requestAi = false;
        GUILayout.BeginArea(area, EditorStyles.helpBox);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawSummary(result);
        DrawOverlays();
        DrawPaths(result);
        DrawBranches(result);
        requestAi = DrawAiSection();
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
        return requestAi;
    }

    private static void DrawSummary(MapAiAnalysisResult result)
    {
        EditorGUILayout.LabelField("Analysis", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Status", result.IsValid && result.CanComplete
            ? "Playable" : "Needs attention");
        EditorGUILayout.LabelField("Shortest completion", result.CanComplete
            ? $"{Mathf.Max(0, result.CompletionPath.Count - 1)} steps" : "Unavailable");
        EditorGUILayout.LabelField("Enumerated S-F paths", result.Paths.Count.ToString());
        EditorGUILayout.LabelField("Dead-end branches", result.DeadEnds.Count.ToString());
        EditorGUILayout.LabelField("Misleading candidates", result.MisleadingCount.ToString());
        EditorGUILayout.LabelField("Suggested time", $"{result.SuggestedSeconds} seconds");
        EditorGUILayout.HelpBox(result.SearchLimited
            ? "Path search reached its configured limit. Results are partial."
            : "Path search completed within the configured budget.",
            result.SearchLimited ? MessageType.Warning : MessageType.Info);
        if (result.CompletionSearchLimited)
            EditorGUILayout.HelpBox("Completion search reached its budget; completion is not proven.",
                MessageType.Warning);
        for (int i = 0; i < result.Issues.Count; i++)
            EditorGUILayout.HelpBox(result.Issues[i], MessageType.Error);
    }

    private void DrawOverlays()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Overlays", EditorStyles.boldLabel);
        showShortest = EditorGUILayout.ToggleLeft("Shortest path", showShortest);
        showDeadEnds = EditorGUILayout.ToggleLeft("Dead ends", showDeadEnds);
        showMisleading = EditorGUILayout.ToggleLeft("Misleading candidates", showMisleading);
    }

    private void DrawPaths(MapAiAnalysisResult result)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Routes", EditorStyles.boldLabel);
        int visibleCount = Mathf.Min(result.Paths.Count, 12);
        for (int i = 0; i < visibleCount; i++)
        {
            string label = $"Route {i + 1} - {result.Paths[i].Count - 1} steps";
            if (GUILayout.Toggle(selectedPath == i, label, "Button")) selectedPath = i;
        }
        if (result.Paths.Count > visibleCount)
            EditorGUILayout.LabelField($"+ {result.Paths.Count - visibleCount} more routes");
    }

    private void DrawBranches(MapAiAnalysisResult result)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Dead-end Analysis", EditorStyles.boldLabel);
        for (int i = 0; i < result.DeadEnds.Count; i++)
        {
            MapAiBranch branch = result.DeadEnds[i];
            string marker = branch.IsMisleadingCandidate ? "Candidate" : "Branch";
            string label = $"{marker} {i + 1} - depth {branch.Cells.Count - 1}";
            if (GUILayout.Toggle(selectedBranch == i, label, "Button")) selectedBranch = i;
        }
        EditorGUILayout.HelpBox(
            "Candidates use geometric distance and required return distance. Line of sight is not verified.",
            MessageType.Info);
    }

    private bool DrawAiSection()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("AI Integration", EditorStyles.boldLabel);
        IMapAiEditorProvider provider = MapAiProviderRegistry.Provider;
        EditorGUILayout.LabelField("Provider", provider == null ? "Not configured" : provider.DisplayName);
        aiRequest = EditorGUILayout.TextArea(aiRequest, GUILayout.MinHeight(54f));
        using (new EditorGUI.DisabledScope(provider == null || aiRequest.Trim().Length == 0))
        {
            return GUILayout.Button("Request AI Proposal");
        }
    }
}
