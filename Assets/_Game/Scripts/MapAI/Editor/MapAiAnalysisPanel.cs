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
    SelectedBranch,
    NearWinDeadEnds
}

[Serializable]
public sealed class MapAiAnalysisPanel
{
    private static readonly string[] OVERLAY_NAMES =
    {
        "Bình thường", "Đường hoàn thành", "Ngõ cụt",
        "Nhánh đánh lạc hướng", "Tuyến đang chọn", "Nhánh đang chọn",
        "Bế tắc sau khi gần đủ target"
    };

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
        EditorGUILayout.LabelField("Phân tích", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Trạng thái", result.IsValid && result.CanComplete
            ? "Chơi được" : "Cần kiểm tra");
        EditorGUILayout.LabelField("Đường hoàn thành ngắn nhất", result.CanComplete
            ? $"{Mathf.Max(0, result.CompletionPath.Count - 1)} bước" : "Chưa tìm thấy");
        EditorGUILayout.LabelField("Số lời giải hoàn chỉnh", result.Paths.Count.ToString());
        EditorGUILayout.LabelField("Điểm lựa chọn", result.DecisionPoints.ToString());
        EditorGUILayout.LabelField("Số nhánh cụt", result.DeadEnds.Count.ToString());
        EditorGUILayout.LabelField("Mồi nhử gần F (ước lượng hình học)",
            result.MisleadingCount.ToString());
        EditorGUILayout.LabelField("Lối bế tắc sau khi lấy gần hết target",
            result.NearWinDeadEndCount.ToString());
        EditorGUILayout.LabelField("Thời gian gợi ý", $"{result.SuggestedSeconds} giây");
        EditorGUILayout.HelpBox(result.SearchLimited
            ? "Đã chạm giới hạn tìm đường. Kết quả chỉ là một phần."
            : "Đã hoàn tất tìm đường trong giới hạn đã đặt.",
            result.SearchLimited ? MessageType.Warning : MessageType.Info);
        if (result.CompletionSearchLimited)
            EditorGUILayout.HelpBox("Đã chạm giới hạn tìm đường hoàn chỉnh; chưa thể xác nhận bản đồ có lời giải.",
                MessageType.Warning);
        for (int i = 0; i < result.Issues.Count; i++)
            EditorGUILayout.HelpBox(result.Issues[i], MessageType.Error);
    }

    private void DrawOverlays()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Tô sáng bản đồ", EditorStyles.boldLabel);
        overlayMode = (MapAiOverlayMode)EditorGUILayout.IntPopup("Chế độ hiển thị",
            (int)overlayMode, OVERLAY_NAMES, new[] { 0, 1, 2, 3, 4, 5, 6 });
        DrawLegend();
    }

    private static void DrawLegend()
    {
        EditorGUILayout.BeginHorizontal();
        DrawLegendItem(new Color(0.2f, 0.85f, 0.25f), "Đường giải");
        DrawLegendItem(new Color(1f, 0.65f, 0.1f), "Ngõ cụt");
        DrawLegendItem(new Color(1f, 0.2f, 0.65f), "Đánh lạc hướng");
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
        EditorGUILayout.LabelField("Các tuyến đường", EditorStyles.boldLabel);
        int visibleCount = Mathf.Min(result.Paths.Count, 12);
        for (int i = 0; i < visibleCount; i++)
        {
            string label = $"Tuyến {i + 1} - {result.Paths[i].Count - 1} bước";
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
            EditorGUILayout.LabelField($"+ {result.Paths.Count - visibleCount} tuyến khác");
    }

    private void DrawBranches(MapAiAnalysisResult result)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Phân tích nhánh cụt", EditorStyles.boldLabel);
        for (int i = 0; i < result.DeadEnds.Count; i++)
        {
            MapAiBranch branch = result.DeadEnds[i];
            string marker = branch.IsMisleadingCandidate ? "Mồi nhử" : "Nhánh";
            string label = $"{marker} {i + 1} - độ sâu {branch.Cells.Count - 1}";
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
            "Phân tích nhánh dùng trạng thái ống ban đầu; bộ tìm đường có tính đến xoay và quay lui. Lối bế tắc gần thắng đã lấy ít nhất một target, còn đúng một target rồi hết nước đi.",
            MessageType.Info);
    }

    private bool DrawAiSection()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Tích hợp AI", EditorStyles.boldLabel);
        IMapAiEditorProvider provider = MapAiProviderRegistry.Provider;
        EditorGUILayout.LabelField("Nhà cung cấp", provider == null ? "Chưa cấu hình" : provider.DisplayName);
        aiRequest = EditorGUILayout.TextArea(aiRequest, GUILayout.MinHeight(54f));
        using (new EditorGUI.DisabledScope(provider == null || aiRequest.Trim().Length == 0))
        {
            return GUILayout.Button("Yêu cầu AI đề xuất");
        }
    }
}
