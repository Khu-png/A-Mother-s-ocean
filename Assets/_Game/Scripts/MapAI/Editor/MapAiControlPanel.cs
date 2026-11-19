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
    private const int SAMPLE_COUNT = 5;
    private const int ICE_BRUSH = 12;
    private static readonly string[] BRUSH_NAMES =
    {
        "Trống", "Bắt đầu", "Đích", "Cá con", "Đá", "Xoay",
        "Thẳng dọc", "Thẳng ngang", "Góc trái-dưới", "Góc trái-trên",
        "Góc trên-phải", "Góc phải-dưới", "Cá con băng"
    };
    private static readonly char[] BRUSH_CODES =
    {
        '.', 'S', 'F', 'T', 'X', 'O', '|', '-', 'A', 'B', 'C', 'E'
    };

    [SerializeField] private Vector2 scroll;
    [SerializeField] private int selectedBrush;
    [SerializeField] private int iceCounter = 2;
    [SerializeField] private bool lockMode;
    [SerializeField] private bool contentSettings = true;
    [SerializeField] private bool showLevelSave;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private int timeLimitSeconds = 120;

    public char? BrushCode => selectedBrush < 0 ? (char?)null
        : selectedBrush == ICE_BRUSH ? (char)('0' + iceCounter) : BRUSH_CODES[selectedBrush];
    public bool LockMode => lockMode;
    public LevelManager LevelManager => levelManager;
    public int LevelNumber => levelNumber;
    public int TimeLimitSeconds { get => timeLimitSeconds; set => timeLimitSeconds = value; }
    public int SampleCount => SAMPLE_COUNT;

    public MapAiEditorCommand Draw(Rect area, MapAiGenerationSettings settings,
        bool hasPreview, bool canApplyPreview, int previewIndex,
        int previewCount, string status)
    {
        MapAiEditorCommand command = MapAiEditorCommand.None;
        GUILayout.BeginArea(area, EditorStyles.helpBox);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField("Tạo bản đồ", EditorStyles.boldLabel);
        settings.Columns = EditorGUILayout.IntSlider("Số cột", settings.Columns,
            MapBuilderLimits.MIN_SIZE, MapBuilderLimits.MAX_COLUMNS);
        settings.Rows = EditorGUILayout.IntSlider("Số hàng", settings.Rows,
            MapBuilderLimits.MIN_SIZE, MapBuilderLimits.MAX_ROWS);
        MapAiDifficulty difficulty = (MapAiDifficulty)EditorGUILayout.IntPopup(
            "Độ khó", (int)settings.Difficulty,
            new[] { "Dễ", "Khó", "Siêu khó" }, new[] { 0, 1, 2 });
        if (difficulty != settings.Difficulty)
        {
            settings.Difficulty = difficulty;
            settings.ApplyPreset();
        }
        DrawContentSettings(settings);

        if (GUILayout.Button("Đổi kích thước lưới")) command = MapAiEditorCommand.Resize;
        if (GUILayout.Button("Tạo bản đồ mẫu", GUILayout.Height(28f)))
        {
            // A fresh seed on every press, so each press gives different maps.
            settings.Seed = Environment.TickCount;
            command = MapAiEditorCommand.Generate;
        }

        if (hasPreview)
        {
            DrawPreviewNavigation(previewIndex, previewCount, ref command);
            EditorGUILayout.Space(6f);
            using (new EditorGUI.DisabledScope(!canApplyPreview))
            {
                if (GUILayout.Button("Áp dụng bản xem trước")) command = MapAiEditorCommand.ApplyPreview;
            }
            if (GUILayout.Button("Bỏ bản xem trước")) command = MapAiEditorCommand.DiscardPreview;
        }

        DrawLevelSave(hasPreview, ref command);
        DrawBrushes();
        lockMode = EditorGUILayout.ToggleLeft("Khóa / mở khóa ô", lockMode);
        EditorGUILayout.HelpBox(lockMode
            ? "Nhấp vào ô để khóa, không cho công cụ thay đổi khi tạo bản đồ."
            : selectedBrush < 0
                ? "Chưa chọn ô vẽ. Hãy chọn một loại ô."
                : "Nhấp vào ô để vẽ loại ô đã chọn.", MessageType.Info);
        if (status != string.Empty) EditorGUILayout.HelpBox(status, MessageType.None);
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
        return command;
    }

    private void DrawContentSettings(MapAiGenerationSettings settings)
    {
        contentSettings = EditorGUILayout.Foldout(contentSettings, "Nội dung bản đồ", true);
        if (!contentSettings) return;

        int maximumTargets = Mathf.Max(0, settings.Columns * settings.Rows - 2);
        settings.TargetCount = EditorGUILayout.IntSlider(
            "Số cá con", settings.TargetCount, 0, maximumTargets);
        settings.HasRocks = EditorGUILayout.ToggleLeft("Có ô đá", settings.HasRocks);
        if (settings.HasRocks) settings.RockRatio = RatioField("Tỷ lệ đá", settings.RockRatio, 0.75f);
        settings.HasOneWay = EditorGUILayout.ToggleLeft("Có ô một chiều", settings.HasOneWay);
        if (settings.HasOneWay)
        {
            settings.StraightOneWayRatio = RatioField("Tỷ lệ ống thẳng", settings.StraightOneWayRatio, 0.5f);
            settings.CornerOneWayRatio = RatioField("Tỷ lệ ống góc", settings.CornerOneWayRatio, 0.5f);
        }
        settings.HasRotate = EditorGUILayout.ToggleLeft("Có ô đổi chiều", settings.HasRotate);
        if (settings.HasRotate) settings.RotateRatio = RatioField("Tỷ lệ ô đổi chiều", settings.RotateRatio, 0.5f);
    }

    private static float RatioField(string label, float value, float maximum)
    {
        EditorGUI.indentLevel++;
        float result = EditorGUILayout.Slider(label, value, 0f, maximum);
        EditorGUI.indentLevel--;
        return result;
    }

    private static void DrawPreviewNavigation(int previewIndex, int previewCount,
        ref MapAiEditorCommand command)
    {
        if (previewCount < 2) return;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Trước") && previewIndex > 0)
            command = MapAiEditorCommand.PreviousPreview;
        EditorGUILayout.LabelField($"Bản {previewIndex + 1} / {previewCount}",
            EditorStyles.centeredGreyMiniLabel);
        if (GUILayout.Button("Sau") && previewIndex < previewCount - 1)
            command = MapAiEditorCommand.NextPreview;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLevelSave(bool hasPreview, ref MapAiEditorCommand command)
    {
        showLevelSave = EditorGUILayout.Foldout(showLevelSave, "Lưu thành màn chơi", true);
        if (!showLevelSave) return;

        levelManager = (LevelManager)EditorGUILayout.ObjectField(
            "Quản lý màn chơi", levelManager, typeof(LevelManager), true);
        levelNumber = Mathf.Max(1, EditorGUILayout.IntField("Số màn", levelNumber));
        timeLimitSeconds = Mathf.Max(1, EditorGUILayout.IntField("Thời gian (giây)", timeLimitSeconds));
        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || hasPreview))
        {
            if (GUILayout.Button("Tạo dữ liệu màn chơi")) command = MapAiEditorCommand.CreateLevel;
        }
        if (hasPreview)
            EditorGUILayout.HelpBox("Hãy áp dụng bản xem trước trước khi lưu thành màn chơi.", MessageType.Info);
    }

    private void DrawBrushes()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Chỉnh sửa thủ công", EditorStyles.boldLabel);
        for (int i = 0; i < ICE_BRUSH; i += 2)
        {
            EditorGUILayout.BeginHorizontal();
            DrawBrushButton(i);
            DrawBrushButton(i + 1);
            EditorGUILayout.EndHorizontal();
        }
        DrawIceBrush();
    }

    private void DrawIceBrush()
    {
        EditorGUILayout.BeginHorizontal();
        DrawBrushButton(ICE_BRUSH);
        EditorGUI.BeginDisabledGroup(selectedBrush != ICE_BRUSH);
        iceCounter = EditorGUILayout.IntSlider(iceCounter, 1, 9);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawBrushButton(int brush)
    {
        bool selected = selectedBrush == brush;
        bool pressed = GUILayout.Toggle(selected, BRUSH_NAMES[brush], EditorStyles.miniButton);
        if (pressed) selectedBrush = brush;
        else if (selected) selectedBrush = -1;
    }
}
