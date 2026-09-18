using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LevelPrefabBuilder
{
    private const string LevelRoot = "Assets/_Game/Prefab/Levels";
    private const string LevelPath = LevelRoot + "/Level_01.prefab";

    static LevelPrefabBuilder()
    {
        EditorApplication.delayCall += CreateMissingLevelPrefab;
    }

    [MenuItem("Tools/A Mother's Ocean/Create Level Prefabs")]
    public static void CreateMissingLevelPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(LevelPath) != null) return;
        EnsureFolder(LevelRoot);
        MapPrefabBuilder.CreateMissingPrefabs();

        GameObject view = new GameObject("Level 01");
        Level level = view.AddComponent<Level>();
        GridMap gridMap = view.AddComponent<GridMap>();

        SetupLevel(level, gridMap);
        SetupGridMap(gridMap);

        PrefabUtility.SaveAsPrefabAsset(view, LevelPath);
        Object.DestroyImmediate(view);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void SetupLevel(Level level, GridMap gridMap)
    {
        SerializedObject data = new SerializedObject(level);
        data.FindProperty("gridMap").objectReferenceValue = gridMap;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetupGridMap(GridMap gridMap)
    {
        SerializedObject data = new SerializedObject(gridMap);
        data.FindProperty("cellSize").floatValue = 1.2f;
        data.FindProperty("lineWidth").floatValue = 0.06f;
        data.FindProperty("gridColor").colorValue = new Color(0.15f, 0.55f, 0.7f, 0.7f);
        data.FindProperty("pathColor").colorValue = new Color(0.12f, 0.8f, 0.95f, 0.28f);
        SetRows(data.FindProperty("mapRows"));
        data.FindProperty("targetSprite").objectReferenceValue = Load<Sprite>("Assets/_Game/Art/Generated/baby_shark_sprite.png");
        data.FindProperty("playerPrefab").objectReferenceValue = Load<Player>("Assets/_Game/Prefab/Player.prefab");
        data.FindProperty("cameraController").objectReferenceValue = null;
        SetupPrefabSet(data.FindProperty("prefabs"));
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetRows(SerializedProperty rows)
    {
        string[] values = { "F|T|B.", ".SAO-.", "XOTOTX", ".XOT-.", "..XO.." };
        rows.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) rows.GetArrayElementAtIndex(i).stringValue = values[i];
    }

    private static void SetupPrefabSet(SerializedProperty prefabs)
    {
        prefabs.FindPropertyRelative("emptyTilePrefab").objectReferenceValue = Load<MapTile>("Assets/_Game/Prefab/Map/Tiles/TileEmpty.prefab");
        prefabs.FindPropertyRelative("blockTilePrefab").objectReferenceValue = Load<BlockTile>("Assets/_Game/Prefab/Map/Tiles/BlockTile.prefab");
        prefabs.FindPropertyRelative("oneWayStraightPrefab").objectReferenceValue = Load<OneWayPath>("Assets/_Game/Prefab/Map/Tiles/OneWayStraight.prefab");
        prefabs.FindPropertyRelative("oneWayCornerPrefab").objectReferenceValue = Load<OneWayPath>("Assets/_Game/Prefab/Map/Tiles/OneWayCorner.prefab");
        prefabs.FindPropertyRelative("rotateButtonPrefab").objectReferenceValue = Load<RotateButton>("Assets/_Game/Prefab/Map/Tiles/RotateButton.prefab");
        prefabs.FindPropertyRelative("startPointPrefab").objectReferenceValue = Load<StartPoint>("Assets/_Game/Prefab/Map/Points/StartPoint.prefab");
        prefabs.FindPropertyRelative("targetPointPrefab").objectReferenceValue = Load<TargetPoint>("Assets/_Game/Prefab/Map/Points/TargetPoint.prefab");
        prefabs.FindPropertyRelative("finishPointPrefab").objectReferenceValue = Load<FinishPoint>("Assets/_Game/Prefab/Map/Points/FinishPoint.prefab");
    }

    private static T Load<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        string name = path.Substring(slash + 1);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
