using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MapPrefabBuilder
{
    private const string Root = "Assets/_Game/Prefab";
    private const string TileRoot = Root + "/Map/Tiles";
    private const string PointRoot = Root + "/Map/Points";

    static MapPrefabBuilder()
    {
        EditorApplication.delayCall += CreateMissingPrefabs;
    }

    [MenuItem("Tools/A Mother's Ocean/Create Map Prefabs")]
    public static void CreateMissingPrefabs()
    {
        EnsureFolders();
        CreateEmpty();
        CreateBlock();
        CreateOneWay("OneWayStraight", OneWayPathKind.Straight, OneWayPathState.Vertical);
        CreateOneWay("OneWayCorner", OneWayPathKind.Corner, OneWayPathState.LeftDown);
        CreateRotate();
        CreateStart();
        CreateTarget();
        CreateFinish();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolders()
    {
        CreateFolder(Root + "/Map");
        CreateFolder(TileRoot);
        CreateFolder(PointRoot);
        CreateFolder(Root + "/UI");
        CreateFolder(Root + "/Background");
    }

    private static void CreateFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        string name = path.Substring(slash + 1);
        if (!AssetDatabase.IsValidFolder(parent)) CreateFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void CreateEmpty()
    {
        string path = TileRoot + "/TileEmpty.prefab";
        if (Exists(path)) return;
        GameObject view = NewView("Tile Empty");
        MapTile tile = view.AddComponent<MapTile>();
        tile.Setup(Vector2Int.zero, Vector3.zero, 1f, GridVisual.SquareSprite(), Color.white);
        Save(view, path);
    }

    private static void CreateBlock()
    {
        string path = TileRoot + "/BlockTile.prefab";
        if (Exists(path)) return;
        GameObject view = NewTileRule("Block Tile", out MapTile tile);
        BlockTile block = view.AddComponent<BlockTile>();
        block.BindTile(tile);
        block.Setup(tile, Color.white);
        Save(view, path);
    }

    private static void CreateOneWay(string name, OneWayPathKind kind, OneWayPathState state)
    {
        string path = TileRoot + "/" + name + ".prefab";
        if (Exists(path)) return;
        GameObject view = NewTileRule(name, out MapTile tile);
        OneWayPath oneWay = view.AddComponent<OneWayPath>();
        oneWay.BindTile(tile);
        oneWay.Setup(tile, kind, state, Color.white);
        Save(view, path);
    }

    private static void CreateRotate()
    {
        string path = TileRoot + "/RotateButton.prefab";
        if (Exists(path)) return;
        GameObject view = NewTileRule("Rotate Button", out MapTile tile);
        RotateButton button = view.AddComponent<RotateButton>();
        button.BindTile(tile);
        button.Setup(tile, Color.white);
        Save(view, path);
    }

    private static void CreateStart()
    {
        string path = PointRoot + "/StartPoint.prefab";
        if (Exists(path)) return;
        StartPoint point = NewComponent<StartPoint>("Start Point");
        point.Setup(Vector2Int.zero, Vector3.zero, 1f, GridVisual.SquareSprite(), Color.white);
        Save(point.gameObject, path);
    }

    private static void CreateTarget()
    {
        string path = PointRoot + "/TargetPoint.prefab";
        if (Exists(path)) return;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Generated/baby_shark_sprite.png");
        TargetPoint point = NewComponent<TargetPoint>("Target Point");
        point.Setup(Vector2Int.zero, Vector3.zero, 1f, sprite != null ? sprite : GridVisual.CircleSprite(), Color.white);
        Save(point.gameObject, path);
    }

    private static void CreateFinish()
    {
        string path = PointRoot + "/FinishPoint.prefab";
        if (Exists(path)) return;
        FinishPoint point = NewComponent<FinishPoint>("Finish Point");
        point.Setup(Vector2Int.zero, Vector3.zero, 1f, GridVisual.SquareSprite(), Color.white, Color.white);
        Save(point.gameObject, path);
    }

    private static GameObject NewTileRule(string name, out MapTile tile)
    {
        GameObject view = NewView(name);
        tile = view.AddComponent<MapTile>();
        tile.Setup(Vector2Int.zero, Vector3.zero, 1f, GridVisual.SquareSprite(), Color.white);
        return view;
    }

    private static T NewComponent<T>(string name) where T : Component
    {
        return NewView(name).AddComponent<T>();
    }

    private static GameObject NewView(string name)
    {
        GameObject view = new GameObject(name);
        view.transform.position = Vector3.zero;
        return view;
    }

    private static void Save(GameObject view, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(view, path);
        Object.DestroyImmediate(view);
    }

    private static bool Exists(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Object>(path) != null;
    }
}
