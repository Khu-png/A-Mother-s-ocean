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
        "Trống", "Bắt đầu", "Đích", "Cá con", "Đá", "Xoay",
        "Thẳng dọc", "Thẳng ngang", "Góc trái-dưới", "Góc trái-trên",
        "Góc trên-phải", "Góc phải-dưới"
    };
    private static readonly char[] BRUSH_CODES =
    {
        '.', 'S', 'F', 'T', 'X', 'O', '|', '-', 'A', 'B', 'C', 'E'
    };

    [SerializeField] private Vector2 scroll;
    [SerializeField] private int selectedBrush;
    [SerializeField] private bool lockMode;
    [SerializeField] private bool reproducibility;
    [SerializeField] private bool contentSettings = true;
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
        MapAiPuzzleFocus focus = (MapAiPuzzleFocus)EditorGUILayout.IntPopup(
            "Trọng tâm câu đố", (int)settings.PuzzleFocus,
            new[] { "Thứ tự cứu cá con", "Thời điểm xoay", "Xoay toàn cục" },
            new[] { 0, 1, 2 });
        if (focus != settings.PuzzleFocus)
        {
            settings.PuzzleFocus = focus;
            settings.ApplyPreset();
        }
        DrawContentSettings(settings);

        if (GUILayout.Button("Đổi kích thước lưới")) command = MapAiEditorCommand.Resize;
        sampleCount = EditorGUILayout.IntSlider("Số bản đồ khác nhau", sampleCount, 2, 10);
        reproducibility = EditorGUILayout.Foldout(reproducibility, "Tái tạo kết quả", true);
        if (reproducibility) settings.Seed = EditorGUILayout.IntField("Hạt giống", settings.Seed);
        if (GUILayout.Button("Tạo bản đồ mẫu", GUILayout.Height(28f)))
            command = MapAiEditorCommand.Generate;

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
        settings.RockRatio = EditorGUILayout.Slider(
            "Tỷ lệ đá", settings.RockRatio, 0f, 0.75f);
        settings.RotateRatio = EditorGUILayout.Slider(
            "Tỷ lệ ô xoay", settings.RotateRatio, 0f, 0.5f);
        settings.StraightOneWayRatio = EditorGUILayout.Slider(
            "Tỷ lệ ống thẳng một chiều", settings.StraightOneWayRatio, 0f, 0.5f);
        settings.CornerOneWayRatio = EditorGUILayout.Slider(
            "Tỷ lệ ống góc một chiều", settings.CornerOneWayRatio, 0f, 0.5f);
        settings.ExtraConnectionChance = EditorGUILayout.Slider(
            "Tỷ lệ mở thêm lối", settings.ExtraConnectionChance, 0f, 0.5f);
        if (GUILayout.Button("Áp dụng thiết lập độ khó")) settings.ApplyPreset();
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
