using System;
using UnityEditor;
using UnityEngine;

public sealed class MapAiEditorWindow : EditorWindow
{
    private const float LEFT_WIDTH = 260f;
    private const float RIGHT_WIDTH = 310f;
    private const float GAP = 4f;
    [SerializeField] private LevelData levelData;
    [SerializeField] private MapAiGridDocument document = new MapAiGridDocument();
    [SerializeField] private MapAiGenerationSettings generation = new MapAiGenerationSettings();
    [SerializeField] private MapAiControlPanel controls = new MapAiControlPanel();
    [SerializeField] private MapAiAnalysisPanel analysisPanel = new MapAiAnalysisPanel();
    [SerializeField] private MapAiGridViewport viewport = new MapAiGridViewport();
    [SerializeField] private string[] previewRows = Array.Empty<string>();
    [SerializeField] private bool hasPreview;
    [SerializeField] private bool previewCanApply;
    [SerializeField] private string status = string.Empty;
    private MapAiAnalysisResult analysis = new MapAiAnalysisResult();
    private MapAiAnalysisResult previewAnalysis = new MapAiAnalysisResult();
    [MenuItem("Tools/A Mother's Ocean/Map AI Editor")]
    public static void Open()
    {
        GetWindow<MapAiEditorWindow>("Map AI Editor");
    }
    void OnEnable()
    {
        document.Initialize();
        generation.Columns = document.Width;
        generation.Rows = document.Height;
        Undo.undoRedoPerformed += HandleUndoRedo;
        AnalyzeCurrent();
        if (hasPreview) AnalyzePreview();
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= HandleUndoRedo;
    }

    void OnGUI()
    {
        DrawToolbar();
        Rect content = new Rect(0f, EditorGUIUtility.singleLineHeight + 7f,
            position.width, position.height - EditorGUIUtility.singleLineHeight - 7f);
        Rect left = new Rect(content.x, content.y, LEFT_WIDTH, content.height);
        Rect right = new Rect(content.xMax - RIGHT_WIDTH, content.y, RIGHT_WIDTH, content.height);
        Rect center = new Rect(left.xMax + GAP, content.y,
            Mathf.Max(40f, right.xMin - left.xMax - GAP * 2f), content.height);

        MapAiEditorCommand command = controls.Draw(left, generation,
            hasPreview, previewCanApply, status);
        Execute(command);
        MapAiAnalysisResult displayedAnalysis = hasPreview ? previewAnalysis : analysis;
        MapAiGridOverlay overlay = BuildOverlay(displayedAnalysis);
        string[] displayedRows = hasPreview ? previewRows : document.ToRows();
        if (viewport.Draw(center, document, displayedRows, hasPreview,
            controls.BrushCode, controls.LockMode, overlay, this)) AnalyzeCurrent();
        if (analysisPanel.Draw(right, displayedAnalysis)) RequestAiProposal();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        levelData = (LevelData)EditorGUILayout.ObjectField(levelData,
            typeof(LevelData), false, GUILayout.Width(180f));
        using (new EditorGUI.DisabledScope(levelData == null))
        {
            if (GUILayout.Button("Open", EditorStyles.toolbarButton)) LoadLevel();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton)) SaveLevel();
        }
        if (GUILayout.Button("Save As", EditorStyles.toolbarButton)) SaveAsLevel();
        if (GUILayout.Button("Undo", EditorStyles.toolbarButton)) Undo.PerformUndo();
        if (GUILayout.Button("Redo", EditorStyles.toolbarButton)) Undo.PerformRedo();
        if (GUILayout.Button("Analyze", EditorStyles.toolbarButton)) AnalyzeCurrent();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Play Scene", EditorStyles.toolbarButton)) EditorApplication.isPlaying = true;
        EditorGUILayout.EndHorizontal();
    }

    private void Execute(MapAiEditorCommand command)
    {
        if (command == MapAiEditorCommand.Resize) Resize();
        else if (command == MapAiEditorCommand.Generate) GeneratePreview();
        else if (command == MapAiEditorCommand.ApplyPreview) ApplyPreview();
        else if (command == MapAiEditorCommand.DiscardPreview) DiscardPreview();
    }

    private void Resize()
    {
        Undo.RecordObject(this, "Resize AI Map Grid");
        document.Resize(generation.Columns, generation.Rows);
        DiscardPreview();
        AnalyzeCurrent();
    }

    private void GeneratePreview()
    {
        if (generation.Columns != document.Width || generation.Rows != document.Height)
        {
            Undo.RecordObject(this, "Resize Grid For Generation");
            document.Resize(generation.Columns, generation.Rows);
        }
        MapAiGenerationResult result = MapAiToolApi.Generate(
            document.Snapshot(), document.ToLocks(), generation);
        previewRows = result.Rows;
        previewAnalysis = result.Analysis;
        hasPreview = true;
        previewCanApply = result.Success;
        status = result.Message;
        Repaint();
    }

    private void ApplyPreview()
    {
        Undo.RecordObject(this, "Apply Generated Map");
        document.ApplyPreview(previewRows);
        DiscardPreview();
        AnalyzeCurrent();
    }

    private void DiscardPreview()
    {
        previewRows = Array.Empty<string>();
        previewAnalysis = new MapAiAnalysisResult();
        hasPreview = false;
        previewCanApply = false;
    }

    private void AnalyzeCurrent()
    {
        analysis = MapAiAnalyzer.Analyze(document.Snapshot(),
            generation.SearchBudget, generation.Difficulty);
        status = analysis.IsValid && analysis.CanComplete
            ? "Map is playable." : "Map needs attention before saving.";
        Repaint();
    }

    private MapAiGridOverlay BuildOverlay(MapAiAnalysisResult result)
    {
        MapAiGridOverlay overlay = new MapAiGridOverlay();
        if (analysisPanel.ShowShortest) overlay.Shortest.UnionWith(result.CompletionPath);
        for (int i = 0; i < result.DeadEnds.Count; i++)
        {
            MapAiBranch branch = result.DeadEnds[i];
            if (analysisPanel.ShowDeadEnds) overlay.DeadEnds.UnionWith(branch.Cells);
            if (analysisPanel.ShowMisleading && branch.IsMisleadingCandidate)
                overlay.Misleading.UnionWith(branch.Cells);
            if (analysisPanel.SelectedBranch == i) overlay.Selected.UnionWith(branch.Cells);
        }
        if (analysisPanel.SelectedPath >= 0 && analysisPanel.SelectedPath < result.Paths.Count)
            overlay.Selected.UnionWith(result.Paths[analysisPanel.SelectedPath]);
        return overlay;
    }

    private void LoadLevel()
    {
        if (!FitsSupportedSize(levelData.MapRows))
        {
            status = $"Level exceeds the supported {MapBuilderLimits.MAX_COLUMNS}x{MapBuilderLimits.MAX_ROWS} limit.";
            return;
        }
        Undo.RecordObject(this, "Open Level Data");
        document.Replace(levelData.MapRows);
        generation.Columns = document.Width;
        generation.Rows = document.Height;
        DiscardPreview();
        AnalyzeCurrent();
    }

    private void SaveLevel()
    {
        Undo.RecordObject(levelData, "Save Map AI Layout");
        levelData.SetMapRows(document.ToRows());
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        status = $"Saved {AssetDatabase.GetAssetPath(levelData)}";
    }

    private void SaveAsLevel()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Level Data",
            "Level_AI", "asset", "Choose a project path for the level.");
        if (path == string.Empty) return;
        LevelData asset = CreateInstance<LevelData>();
        asset.SetMapRows(document.ToRows());
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        levelData = asset;
        status = $"Created {path}";
    }

    private void RequestAiProposal()
    {
        MapAiMapData current = document.Snapshot();
        MapAiToolSnapshot snapshot = MapAiToolApi.ReadAndAnalyze(current,
            generation.SearchBudget, generation.Difficulty);
        MapAiProposal proposal = MapAiProviderRegistry.Provider.Propose(
            analysisPanel.AiRequest, snapshot);
        if (!MapAiToolApi.MatchesVersion(proposal, current))
        {
            status = "AI proposal targets an older map version and was rejected.";
            return;
        }
        if (!MatchesDocumentSize(proposal.Rows))
        {
            status = "AI proposal dimensions do not match the current document.";
            return;
        }
        previewRows = proposal.Rows;
        AnalyzePreview();
        hasPreview = true;
        previewCanApply = previewAnalysis.IsValid && previewAnalysis.CanComplete;
        status = proposal.Explanation;
    }

    private void AnalyzePreview()
    {
        previewAnalysis = MapAiAnalyzer.Analyze(new MapAiMapData(previewRows),
            generation.SearchBudget, generation.Difficulty);
    }

    private bool MatchesDocumentSize(string[] rows)
    {
        if (rows.Length != document.Height) return false;
        for (int y = 0; y < rows.Length; y++)
        {
            if (rows[y].Length != document.Width) return false;
        }
        return true;
    }

    private static bool FitsSupportedSize(string[] rows)
    {
        if (rows.Length < MapBuilderLimits.MIN_SIZE
            || rows.Length > MapBuilderLimits.MAX_ROWS) return false;
        int width = 0;
        for (int y = 0; y < rows.Length; y++) width = Mathf.Max(width, rows[y].Length);
        return width >= MapBuilderLimits.MIN_SIZE && width <= MapBuilderLimits.MAX_COLUMNS;
    }

    private void HandleUndoRedo()
    {
        DiscardPreview();
        AnalyzeCurrent();
    }
}
