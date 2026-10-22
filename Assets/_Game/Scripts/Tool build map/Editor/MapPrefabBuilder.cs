using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MapPrefabBuilder
{
    private const string ROOT = "Assets/_Game/Prefab";
    private const string TILE_ROOT = ROOT + "/Map/Tiles";
    private const string POINT_ROOT = ROOT + "/Map/Points";
    private const string ART_ROOT = "Assets/_Game/Resources/MapTiles/";

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
        CreateOneWay("OneWayStraight", "pipe_horizontal.png", OneWayPathKind.Straight, OneWayPathState.Vertical);
        CreateOneWay("OneWayCorner", "pipe_left_down.png", OneWayPathKind.Corner, OneWayPathState.LeftDown);
        CreateRotate();
        CreateStart();
        CreateTarget();
        CreateFinish();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolders()
    {
        CreateFolder(ROOT + "/Map");
        CreateFolder(TILE_ROOT);
        CreateFolder(POINT_ROOT);
        CreateFolder(ROOT + "/UI");
        CreateFolder(ROOT + "/Background");
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
        string path = TILE_ROOT + "/EmptyTile.prefab";
        if (Exists(path)) return;
        GameObject view = NewTileView("EmptyTile", "tile_empty.png", out MapTile tile);
        tile.Setup(Vector2Int.zero, Vector3.zero, 1f, Color.white);
        Save(view, path);
    }

    private static void CreateBlock()
    {
        string path = TILE_ROOT + "/BlockTile.prefab";
        if (Exists(path)) return;
        GameObject view = NewTileView("BlockTile", "tile_block.png", out MapTile tile);
        BlockTile block = view.AddComponent<BlockTile>();
        block.BindTile(tile);
        block.Setup(tile, Color.white);
        Save(view, path);
    }

    private static void CreateOneWay(string name, string spriteName, OneWayPathKind kind, OneWayPathState state)
    {
        string path = TILE_ROOT + "/" + name + ".prefab";
        if (Exists(path)) return;
        GameObject view = NewTileView(name, spriteName, out MapTile tile);
        OneWayPath oneWay = view.AddComponent<OneWayPath>();
        oneWay.BindTile(tile);
        oneWay.Setup(tile, kind, state, Color.white);
        Save(view, path);
    }

    private static void CreateRotate()
    {
        string path = TILE_ROOT + "/RotateTile.prefab";
        if (Exists(path)) return;
        GameObject view = NewTileView("RotateTile", "tile_rotate.png", out MapTile tile);
        RotateButton button = view.AddComponent<RotateButton>();
        button.BindTile(tile);
        button.Setup(tile, Color.white);
        Save(view, path);
    }

    private static void CreateStart()
    {
        string path = POINT_ROOT + "/StartPoint.prefab";
        if (Exists(path)) return;
        GameObject view = NewView("Start Point");
        SpriteRenderer renderer = AddRenderer(view, "tile_start.png");
        StartPoint point = view.AddComponent<StartPoint>();
        point.ConfigureRenderer(renderer);
        point.Setup(Vector2Int.zero, Vector3.zero, Color.white);
        Save(view, path);
    }

    private static void CreateTarget()
    {
        string path = POINT_ROOT + "/TargetPoint.prefab";
        if (Exists(path)) return;
        GameObject view = NewView("Target Point");
        Sprite sprite = LoadSprite("Assets/_Game/Resources/Art/Generated/baby_shark_sprite.png");
        SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        TargetPoint point = view.AddComponent<TargetPoint>();
        point.ConfigureRenderer(renderer);
        point.Setup(Vector2Int.zero, Vector3.zero, sprite, Color.white);
        Save(view, path);
    }

    private static void CreateFinish()
    {
        string path = POINT_ROOT + "/FinishPoint.prefab";
        if (Exists(path)) return;
        GameObject view = NewView("Finish Point");
        Sprite closedSprite = LoadSprite(ART_ROOT + "tile_finish_closed.png");
        Sprite openSprite = LoadSprite(ART_ROOT + "tile_finish_open.png");
        SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
        renderer.sprite = closedSprite;
        FinishPoint point = view.AddComponent<FinishPoint>();
        point.ConfigureVisuals(renderer, closedSprite, openSprite);
        point.Setup(Vector2Int.zero, Vector3.zero, Color.white, Color.white);
        Save(view, path);
    }

    private static GameObject NewTileView(string name, string spriteName, out MapTile tile)
    {
        GameObject view = NewView(name);
        SpriteRenderer renderer = AddRenderer(view, spriteName);
        TextMesh label = NewLabel(view.transform);
        tile = view.AddComponent<MapTile>();
        tile.ConfigureVisuals(renderer, label);
        tile.Setup(Vector2Int.zero, Vector3.zero, 1f, Color.white);
        return view;
    }

    private static SpriteRenderer AddRenderer(GameObject view, string spriteName)
    {
        SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite(ART_ROOT + spriteName);
        return renderer;
    }

    private static TextMesh NewLabel(Transform root)
    {
        GameObject labelView = new GameObject("Label");
        labelView.transform.SetParent(root, false);
        return labelView.AddComponent<TextMesh>();
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
