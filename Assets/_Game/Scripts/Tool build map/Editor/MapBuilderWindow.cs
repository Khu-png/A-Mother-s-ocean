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
    private GridMap templateMap;
    private GameObject levelPrefab;
    private Vector2 scroll;
    private int selectedBrush;
    private int width = 6;
    private int height = 5;
    private bool createPrefab;
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
        EditorGUILayout.LabelField("Template", EditorStyles.boldLabel);
        templateMap = (GridMap)EditorGUILayout.ObjectField("Grid Map", templateMap, typeof(GridMap), true);

        EditorGUILayout.LabelField("Level Asset", EditorStyles.boldLabel);
        levelPrefab = (GameObject)EditorGUILayout.ObjectField("Level Prefab", levelPrefab, typeof(GameObject), false);

        using (new EditorGUI.DisabledScope(levelPrefab == null))
        {
            if (GUILayout.Button("Load From Prefab")) LoadFromPrefab();
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

        using (new EditorGUI.DisabledScope(levelPrefab == null))
        {
            if (GUILayout.Button("Apply To Prefab")) ApplyToPrefab();
        }

        createPrefab = EditorGUILayout.Toggle("Create this as prefab", createPrefab);
        using (new EditorGUI.DisabledScope(!createPrefab)) levelNumber = EditorGUILayout.IntField("Which Level", levelNumber);
        using (new EditorGUI.DisabledScope(!createPrefab || templateMap == null))
        {
            if (GUILayout.Button("Create Level Prefab")) SaveLevelPrefab();
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

    private void LoadFromPrefab()
    {
        SerializedProperty rows = RowsProperty();
        string[] loadedRows = new string[rows.arraySize];
        for (int i = 0; i < rows.arraySize; i++) loadedRows[i] = rows.GetArrayElementAtIndex(i).stringValue;
        grid.LoadRows(loadedRows);
        width = grid.Width;
        height = grid.Height;
    }

    private void ApplyToPrefab()
    {
        GridMap prefabMap = PrefabGridMap();
        Undo.RecordObject(prefabMap, "Apply Map Layout");
        SerializedObject serializedMap = new SerializedObject(prefabMap);
        SerializedProperty rows = serializedMap.FindProperty("mapRows");
        string[] mapRows = grid.ToRows();
        rows.arraySize = mapRows.Length;

        for (int i = 0; i < mapRows.Length; i++) rows.GetArrayElementAtIndex(i).stringValue = mapRows[i];
        serializedMap.ApplyModifiedProperties();
        EditorUtility.SetDirty(prefabMap);
        AssetDatabase.SaveAssets();
        saveMessage = $"Save thành công vào prefab: {AssetDatabase.GetAssetPath(levelPrefab)}";
    }

    private void SaveLevelPrefab()
    {
        levelNumber = Mathf.Max(1, levelNumber);
        EnsureLevelFolder();
        string path = $"Assets/_Game/Prefab/Levels/Level_{levelNumber}.prefab";
        PrefabUtility.SaveAsPrefabAsset(templateMap.gameObject, path);
        AssetDatabase.SaveAssets();
        saveMessage = $"Save thành công vào prefab: {path}";
    }

    private void EnsureLevelFolder()
    {
        if (AssetDatabase.IsValidFolder("Assets/_Game/Prefab/Levels")) return;
        if (!AssetDatabase.IsValidFolder("Assets/_Game/Prefab")) AssetDatabase.CreateFolder("Assets/_Game", "Prefab");
        AssetDatabase.CreateFolder("Assets/_Game/Prefab", "Levels");
    }

    private SerializedProperty RowsProperty()
    {
        return new SerializedObject(PrefabGridMap()).FindProperty("mapRows");
    }

    private GridMap PrefabGridMap()
    {
        string path = AssetDatabase.GetAssetPath(levelPrefab);
        GridMap prefabMap = AssetDatabase.LoadAssetAtPath<GridMap>(path);
        if (prefabMap == null) throw new UnityException("Chọn prefab không hợp lệ.");
        return prefabMap;
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
