using System;
using UnityEditor;
using UnityEngine;

public enum MapAiEditorCommand
{
    None,
    Resize,
    Generate,
    ApplyPreview,
    DiscardPreview
}

[Serializable]
public sealed class MapAiControlPanel
{
    private static readonly string[] BRUSH_NAMES =
    {
        "Empty", "Start", "Finish", "Target", "Block", "Rotate",
        "Vertical", "Horizontal", "Left Down", "Left Up", "Up Right", "Right Down"
    };
    private static readonly char[] BRUSH_CODES =
    {
        '.', 'S', 'F', 'T', 'X', 'O', '|', '-', 'A', 'B', 'C', 'E'
    };

    [SerializeField] private Vector2 scroll;
    [SerializeField] private int selectedBrush;
    [SerializeField] private bool lockMode;
    [SerializeField] private bool advanced;

    public char BrushCode => BRUSH_CODES[selectedBrush];
    public bool LockMode => lockMode;

    public MapAiEditorCommand Draw(Rect area, MapAiGenerationSettings settings,
        bool hasPreview, bool canApplyPreview, string status)
    {
        MapAiEditorCommand command = MapAiEditorCommand.None;
        GUILayout.BeginArea(area, EditorStyles.helpBox);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
        settings.Columns = EditorGUILayout.IntSlider("Columns", settings.Columns,
            MapBuilderLimits.MIN_SIZE, MapBuilderLimits.MAX_COLUMNS);
        settings.Rows = EditorGUILayout.IntSlider("Rows", settings.Rows,
            MapBuilderLimits.MIN_SIZE, MapBuilderLimits.MAX_ROWS);
        MapAiDifficulty difficulty = (MapAiDifficulty)EditorGUILayout.EnumPopup(
            "Difficulty", settings.Difficulty);
        if (difficulty != settings.Difficulty)
        {
            settings.Difficulty = difficulty;
            settings.ApplyPreset();
        }
        DrawGenerationSettings(settings);

        if (GUILayout.Button("Resize Grid")) command = MapAiEditorCommand.Resize;
        advanced = EditorGUILayout.Foldout(advanced, "Advanced", true);
        if (advanced) DrawAdvanced(settings);
        if (GUILayout.Button("Generate Preview", GUILayout.Height(28f)))
            command = MapAiEditorCommand.Generate;

        if (hasPreview)
        {
            EditorGUILayout.Space(6f);
            using (new EditorGUI.DisabledScope(!canApplyPreview))
            {
                if (GUILayout.Button("Apply Preview")) command = MapAiEditorCommand.ApplyPreview;
            }
            if (GUILayout.Button("Discard Preview")) command = MapAiEditorCommand.DiscardPreview;
        }

        DrawBrushes();
        lockMode = EditorGUILayout.ToggleLeft("Lock / unlock cells", lockMode);
        EditorGUILayout.HelpBox(lockMode
            ? "Click cells to protect them from generation."
            : "Click a cell to paint with the selected tile.", MessageType.Info);
        if (status != string.Empty) EditorGUILayout.HelpBox(status, MessageType.None);
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
        return command;
    }

    private static void DrawGenerationSettings(MapAiGenerationSettings settings)
    {
        int maximumTargets = Mathf.Max(0, settings.Columns * settings.Rows - 2);
        settings.TargetCount = EditorGUILayout.IntSlider(
            "Targets", settings.TargetCount, 0, maximumTargets);
        settings.RockRatio = DrawPercent("Rocks (all cells)", settings.RockRatio, 75f);
        settings.RotateRatio = DrawPercent("Rotate (walkable)", settings.RotateRatio, 50f);
        settings.StraightOneWayRatio = DrawPercent(
            "Straight 1-way (walkable)", settings.StraightOneWayRatio, 50f);
        settings.CornerOneWayRatio = DrawPercent(
            "Corner 1-way (walkable)", settings.CornerOneWayRatio, 50f);
        settings.Seed = EditorGUILayout.IntField("Seed", settings.Seed);
    }

    private static float DrawPercent(string label, float ratio, float maximum)
    {
        return EditorGUILayout.Slider(label, ratio * 100f, 0f, maximum) * 0.01f;
    }

    private static void DrawAdvanced(MapAiGenerationSettings settings)
    {
        settings.ExtraConnectionChance = EditorGUILayout.Slider(
            "Extra connections", settings.ExtraConnectionChance, 0f, 0.5f);
        settings.SearchBudget.MaximumPaths = EditorGUILayout.IntField(
            "Maximum paths", settings.SearchBudget.MaximumPaths);
        settings.SearchBudget.MaximumPathLength = EditorGUILayout.IntField(
            "Maximum path length", settings.SearchBudget.MaximumPathLength);
        settings.SearchBudget.MaximumExpansions = EditorGUILayout.IntField(
            "Search expansions", settings.SearchBudget.MaximumExpansions);
        settings.SearchBudget.MaximumPaths = Mathf.Max(1, settings.SearchBudget.MaximumPaths);
        settings.SearchBudget.MaximumPathLength = Mathf.Max(2, settings.SearchBudget.MaximumPathLength);
        settings.SearchBudget.MaximumExpansions = Mathf.Max(100, settings.SearchBudget.MaximumExpansions);
    }

    private void DrawBrushes()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Manual Editing", EditorStyles.boldLabel);
        selectedBrush = GUILayout.SelectionGrid(selectedBrush, BRUSH_NAMES, 2);
    }
}
