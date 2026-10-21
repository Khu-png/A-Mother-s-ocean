using System;
using UnityEditor;
using UnityEngine;

public enum MapAiEditorCommand
{
    None,
    Resize,
    Generate,
    ApplyPreview,
    DiscardPreview,
    CreateLevel,
    PreviousPreview,
    NextPreview
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
    [SerializeField] private bool reproducibility;
    [SerializeField] private bool showLevelSave;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private int levelNumber = 1;
    [SerializeField, Range(2, 10)] private int sampleCount = 5;

    public char? BrushCode => selectedBrush < 0 ? (char?)null : BRUSH_CODES[selectedBrush];
    public bool LockMode => lockMode;
    public LevelManager LevelManager => levelManager;
    public int LevelNumber => levelNumber;
    public int SampleCount => sampleCount;

    public MapAiEditorCommand Draw(Rect area, MapAiGenerationSettings settings,
        bool hasPreview, bool canApplyPreview, int previewIndex,
        int previewCount, string status)
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
        MapAiPuzzleFocus focus = (MapAiPuzzleFocus)EditorGUILayout.EnumPopup(
            "Puzzle focus", settings.PuzzleFocus);
        if (focus != settings.PuzzleFocus)
        {
            settings.PuzzleFocus = focus;
            settings.ApplyPreset();
        }

        if (GUILayout.Button("Resize Grid")) command = MapAiEditorCommand.Resize;
        sampleCount = EditorGUILayout.IntSlider("Unique samples", sampleCount, 2, 10);
        reproducibility = EditorGUILayout.Foldout(reproducibility, "Reproducibility", true);
        if (reproducibility) settings.Seed = EditorGUILayout.IntField("Seed", settings.Seed);
        if (GUILayout.Button("Generate Samples", GUILayout.Height(28f)))
            command = MapAiEditorCommand.Generate;

        if (hasPreview)
        {
            DrawPreviewNavigation(previewIndex, previewCount, ref command);
            EditorGUILayout.Space(6f);
            using (new EditorGUI.DisabledScope(!canApplyPreview))
            {
                if (GUILayout.Button("Apply Preview")) command = MapAiEditorCommand.ApplyPreview;
            }
            if (GUILayout.Button("Discard Preview")) command = MapAiEditorCommand.DiscardPreview;
        }

        DrawLevelSave(hasPreview, ref command);
        DrawBrushes();
        lockMode = EditorGUILayout.ToggleLeft("Lock / unlock cells", lockMode);
        EditorGUILayout.HelpBox(lockMode
            ? "Click cells to protect them from generation."
            : selectedBrush < 0
                ? "No tile selected. Choose a tile to paint."
                : "Click a cell to paint with the selected tile.", MessageType.Info);
        if (status != string.Empty) EditorGUILayout.HelpBox(status, MessageType.None);
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
        return command;
    }

    private static void DrawPreviewNavigation(int previewIndex, int previewCount,
        ref MapAiEditorCommand command)
    {
        if (previewCount < 2) return;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous") && previewIndex > 0)
            command = MapAiEditorCommand.PreviousPreview;
        EditorGUILayout.LabelField($"Sample {previewIndex + 1} / {previewCount}",
            EditorStyles.centeredGreyMiniLabel);
        if (GUILayout.Button("Next") && previewIndex < previewCount - 1)
            command = MapAiEditorCommand.NextPreview;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLevelSave(bool hasPreview, ref MapAiEditorCommand command)
    {
        showLevelSave = EditorGUILayout.Foldout(showLevelSave, "Save As Game Level", true);
        if (!showLevelSave) return;

        levelManager = (LevelManager)EditorGUILayout.ObjectField(
            "Level Manager", levelManager, typeof(LevelManager), true);
        levelNumber = Mathf.Max(1, EditorGUILayout.IntField("Level number", levelNumber));
        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || hasPreview))
        {
            if (GUILayout.Button("Create Level Data")) command = MapAiEditorCommand.CreateLevel;
        }
        if (hasPreview)
            EditorGUILayout.HelpBox("Apply the preview before saving it as a level.", MessageType.Info);
    }

    private void DrawBrushes()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Manual Editing", EditorStyles.boldLabel);
        for (int i = 0; i < BRUSH_NAMES.Length; i += 2)
        {
            EditorGUILayout.BeginHorizontal();
            DrawBrushButton(i);
            DrawBrushButton(i + 1);
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawBrushButton(int brush)
    {
        bool selected = selectedBrush == brush;
        bool pressed = GUILayout.Toggle(selected, BRUSH_NAMES[brush], EditorStyles.miniButton);
        if (pressed) selectedBrush = brush;
        else if (selected) selectedBrush = -1;
    }
}
