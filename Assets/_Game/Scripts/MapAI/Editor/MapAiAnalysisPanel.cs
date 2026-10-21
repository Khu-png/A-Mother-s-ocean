using System;
using UnityEditor;
using UnityEngine;

public enum MapAiOverlayMode
{
    Normal,
    CompletionPath,
    DeadEnds,
    MisleadingCandidates,
    SelectedRoute,
    SelectedBranch
}

[Serializable]
public sealed class MapAiAnalysisPanel
{
    [SerializeField] private Vector2 scroll;
    [SerializeField] private MapAiOverlayMode overlayMode;
    [SerializeField] private int selectedBranch = -1;
    [SerializeField] private int selectedPath = -1;
    [SerializeField] private string aiRequest = string.Empty;

    public MapAiOverlayMode OverlayMode => overlayMode;
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
        EditorGUILayout.LabelField("Complete solutions", result.Paths.Count.ToString());
        EditorGUILayout.LabelField("Decision points", result.DecisionPoints.ToString());
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
        EditorGUILayout.LabelField("Map Highlight", EditorStyles.boldLabel);
        overlayMode = (MapAiOverlayMode)EditorGUILayout.EnumPopup("View mode", overlayMode);
        DrawLegend();
    }

    private static void DrawLegend()
    {
        EditorGUILayout.BeginHorizontal();
        DrawLegendItem(new Color(0.2f, 0.85f, 0.25f), "Completion path");
        DrawLegendItem(new Color(1f, 0.65f, 0.1f), "Dead end");
        DrawLegendItem(new Color(1f, 0.2f, 0.65f), "Misleading");
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawLegendItem(Color color, string label)
    {
        Rect swatch = GUILayoutUtility.GetRect(10f, 10f, GUILayout.Width(10f));
        EditorGUI.DrawRect(swatch, color);
        GUILayout.Label(label, EditorStyles.miniLabel);
    }

    private void DrawPaths(MapAiAnalysisResult result)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Routes", EditorStyles.boldLabel);
        int visibleCount = Mathf.Min(result.Paths.Count, 12);
        for (int i = 0; i < visibleCount; i++)
        {
            string label = $"Route {i + 1} - {result.Paths[i].Count - 1} steps";
            bool selected = selectedPath == i;
            bool active = GUILayout.Toggle(selected, label, "Button");
            if (active)
            {
                selectedPath = i;
                selectedBranch = -1;
                overlayMode = MapAiOverlayMode.SelectedRoute;
            }
            else if (selected)
            {
                selectedPath = -1;
                overlayMode = MapAiOverlayMode.Normal;
            }
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
            bool selected = selectedBranch == i;
            bool active = GUILayout.Toggle(selected, label, "Button");
            if (active)
            {
                selectedBranch = i;
                selectedPath = -1;
                overlayMode = MapAiOverlayMode.SelectedBranch;
            }
            else if (selected)
            {
                selectedBranch = -1;
                overlayMode = MapAiOverlayMode.Normal;
            }
        }
        EditorGUILayout.HelpBox(
            "Branch analysis uses the initial pipe state; route solving tracks rotation and backtracking. Line of sight is not verified.",
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
