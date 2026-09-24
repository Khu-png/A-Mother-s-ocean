using UnityEditor;
using UnityEngine;

public class MapBuilderWindow : EditorWindow
{
    private readonly string[] brushNames =
    {
        "Empty .", "Start S", "Finish F", "Target T", "Block X", "Rotate O",
        "Straight |", "Straight -", "Corner A LD", "Corner B LU", "Corner C UR", "Corner E RD"
    };

    private readonly char[] brushCodes = { '.', 'S', 'F', 'T', 'X', 'O', '|', '-', 'A', 'B', 'C', 'E' };
    private MapBuilderGridState grid = new MapBuilderGridState(6, 5);
    private LevelData levelData;
    private Vector2 scroll;
    private int selectedBrush;
    private int width = 6;
    private int height = 5;
    private bool createLevel;
    private int levelNumber = 1;
    private string saveMessage = string.Empty;

    [MenuItem("Tools/A Mother's Ocean/Map Builder")]
    public static void Open()
    {
        GetWindow<MapBuilderWindow>("Map Builder");
    }

    private void OnGUI()
    {
        DrawTarget();
        DrawSizeTools();
        DrawPalette();
        DrawGrid();
        DrawOutputTools();
    }

    private void DrawTarget()
    {
        EditorGUILayout.LabelField("Level Data", EditorStyles.boldLabel);
        levelData = (LevelData)EditorGUILayout.ObjectField("Level", levelData, typeof(LevelData), false);

        using (new EditorGUI.DisabledScope(levelData == null))
        {
            if (GUILayout.Button("Load From Level Data")) LoadFromLevelData();
        }
    }

    private void DrawSizeTools()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Size", EditorStyles.boldLabel);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        if (GUILayout.Button("Resize Grid"))
        {
            grid.Resize(width, height);
            width = grid.Width;
            height = grid.Height;
        }
    }

    private void DrawPalette()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
        selectedBrush = GUILayout.SelectionGrid(selectedBrush, brushNames, 3);
    }

    private void DrawGrid()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int y = 0; y < grid.Height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < grid.Width; x++)
            {
                DrawCellButton(x, y);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawCellButton(int x, int y)
    {
        char code = grid.GetCell(x, y);
        GUI.backgroundColor = CellColor(code);
        if (GUILayout.Button(CellLabel(code), GUILayout.Width(46f), GUILayout.Height(34f)))
        {
            grid.SetCell(x, y, brushCodes[selectedBrush]);
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawOutputTools()
    {
        EditorGUILayout.Space(8f);

        using (new EditorGUI.DisabledScope(levelData == null))
        {
            if (GUILayout.Button("Apply To Level Data")) ApplyToLevelData();
        }

        createLevel = EditorGUILayout.Toggle("Create this as level", createLevel);
        using (new EditorGUI.DisabledScope(!createLevel)) levelNumber = EditorGUILayout.IntField("Which Level", levelNumber);
        using (new EditorGUI.DisabledScope(!createLevel))
        {
            if (GUILayout.Button("Create Level Data")) SaveLevelData();
        }

        if (saveMessage != string.Empty)
        {
            GUIStyle saveStyle = new GUIStyle(EditorStyles.label);
            saveStyle.normal.textColor = new Color(0.2f, 0.8f, 0.3f);
            EditorGUILayout.LabelField(saveMessage, saveStyle);
        }

        if (GUILayout.Button("Copy mapRows")) EditorGUIUtility.systemCopyBuffer = grid.ToCSharpRows();
        EditorGUILayout.HelpBox("A=left+down, B=left+up, C=up+right, E=right+down.", MessageType.Info);
        EditorGUILayout.TextArea(grid.ToCSharpRows(), GUILayout.MinHeight(90f));
    }

    private void LoadFromLevelData()
    {
        grid.LoadRows(levelData.MapRows);
        width = grid.Width;
        height = grid.Height;
    }

    private void ApplyToLevelData()
    {
        Undo.RecordObject(levelData, "Apply Map Layout");
        levelData.SetMapRows(grid.ToRows());
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        saveMessage = $"Save thành công vào level data: {AssetDatabase.GetAssetPath(levelData)}";
    }

    private void SaveLevelData()
    {
        levelNumber = Mathf.Max(1, levelNumber);
        EnsureLevelDataFolder();
        string path = $"Assets/_Game/ScriptableObject/Map/Level_{levelNumber}.asset";
        LevelData asset = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (asset == null)
        {
            asset = CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.SetMapRows(grid.ToRows());
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        levelData = asset;
        saveMessage = $"Save thành công vào level data: {path}";
    }

    private void EnsureLevelDataFolder()
    {
        const string root = "Assets/_Game/ScriptableObject";
        const string map = root + "/Map";
        if (!AssetDatabase.IsValidFolder(root)) AssetDatabase.CreateFolder("Assets/_Game/Scripts", "ScriptableObject");
        if (!AssetDatabase.IsValidFolder(map)) AssetDatabase.CreateFolder(root, "Map");
    }

    private string CellLabel(char code)
    {
        if (code == '.') return ".";
        if (code == '|') return "|";
        if (code == '-') return "-";
        return code.ToString();
    }

    private Color CellColor(char code)
    {
        if (code == 'S') return new Color(0.35f, 1f, 0.65f);
        if (code == 'F') return new Color(0.45f, 0.65f, 1f);
        if (code == 'T') return new Color(1f, 0.72f, 0.15f);
        if (code == 'X') return new Color(0.12f, 0.12f, 0.12f);
        if (code == 'O') return new Color(1f, 0.48f, 0.12f);
        if (MapCode.IsOneWay(code)) return new Color(0.1f, 0.55f, 0.75f);
        return new Color(0.25f, 0.32f, 0.35f);
    }
}
