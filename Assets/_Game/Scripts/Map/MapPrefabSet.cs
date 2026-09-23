using System;
using UnityEngine;

[Serializable]
public class MapPrefabSet
{
    [Header("Tile Prefabs")]
    [Tooltip("Prefab cho Empty Tile (EmptyTile.prefab)")]
    [SerializeField] private MapTile emptyTilePrefab;

    [Tooltip("Prefab cho Block Tile (BlockTile.prefab)")]
    [SerializeField] private BlockTile blockTilePrefab;

    [Tooltip("Prefab cho OneWay Straight (OneWayStraight.prefab)")]
    [SerializeField] private OneWayPath oneWayStraightPrefab;

    [Tooltip("Prefab cho OneWay Corner (OneWayCorner.prefab)")]
    [SerializeField] private OneWayPath oneWayCornerPrefab;

    [Tooltip("Prefab cho Rotate Tile (RotateTile.prefab)")]
    [SerializeField] private RotateButton rotateButtonPrefab;

    [Header("Point Prefabs")]
    [Tooltip("Prefab cho Start Point (StartPoint.prefab)")]
    [SerializeField] private StartPoint startPointPrefab;

    [Tooltip("Prefab cho Target Point (TargetPoint.prefab)")]
    [SerializeField] private TargetPoint targetPointPrefab;

    [Tooltip("Prefab cho Finish Point (FinishPoint.prefab)")]
    [SerializeField] private FinishPoint finishPointPrefab;

    public MapTile EmptyTilePrefab => emptyTilePrefab;
    public BlockTile BlockTilePrefab => blockTilePrefab;
    public OneWayPath OneWayStraightPrefab => oneWayStraightPrefab;
    public OneWayPath OneWayCornerPrefab => oneWayCornerPrefab;
    public RotateButton RotateButtonPrefab => rotateButtonPrefab;
    public StartPoint StartPointPrefab => startPointPrefab;
    public TargetPoint TargetPointPrefab => targetPointPrefab;
    public FinishPoint FinishPointPrefab => finishPointPrefab;

    public MapTile CreateTile(Transform root, Vector2Int cell, char code, Vector3 position, float cellSize,
        Sprite emptySprite, out BlockTile block, out OneWayPath path, out RotateButton button)
    {
        block = null;
        path = null;
        button = null;

        if (MapCode.IsBlock(code))
        {
            block = CreateBlock(root);
            block.Tile.Setup(cell, position, cellSize, emptySprite, Color.white);
            block.Setup(block.Tile, Color.white);
            return block.Tile;
        }

        if (MapCode.IsOneWay(code))
        {
            path = CreateOneWay(root, MapCode.PathKind(code));
            path.Tile.Setup(cell, position, cellSize, emptySprite, Color.white);
            path.Setup(path.Tile, MapCode.PathKind(code), MapCode.PathState(code), Color.white);
            return path.Tile;
        }

        if (MapCode.IsRotator(code))
        {
            button = CreateRotator(root);
            button.Tile.Setup(cell, position, cellSize, emptySprite, Color.white);
            button.Setup(button.Tile, Color.white);
            return button.Tile;
        }

        MapTile tile = emptyTilePrefab != null ? UnityEngine.Object.Instantiate(emptyTilePrefab, root) : NewTile(root);
        tile.name = $"Tile {cell.x} {cell.y}";
        tile.Setup(cell, position, cellSize, emptySprite, Color.white);
        return tile;
    }

    public StartPoint CreateStart(Transform root)
    {
        return startPointPrefab != null ? UnityEngine.Object.Instantiate(startPointPrefab, root) : NewComponent<StartPoint>("Start Point", root);
    }

    public TargetPoint CreateTarget(Transform root, Vector2Int cell)
    {
        string name = $"Target {cell.x} {cell.y}";
        return targetPointPrefab != null ? Rename(UnityEngine.Object.Instantiate(targetPointPrefab, root), name) : NewComponent<TargetPoint>(name, root);
    }

    public FinishPoint CreateFinish(Transform root)
    {
        return finishPointPrefab != null ? UnityEngine.Object.Instantiate(finishPointPrefab, root) : NewComponent<FinishPoint>("Finish Point", root);
    }

    private BlockTile CreateBlock(Transform root)
    {
        return blockTilePrefab != null ? UnityEngine.Object.Instantiate(blockTilePrefab, root) : NewRule<BlockTile>("Block Tile", root);
    }

    private OneWayPath CreateOneWay(Transform root, OneWayPathKind kind)
    {
        OneWayPath prefab = kind == OneWayPathKind.Corner ? oneWayCornerPrefab : oneWayStraightPrefab;
        string name = kind == OneWayPathKind.Corner ? "One Way Corner" : "One Way Straight";
        return prefab != null ? UnityEngine.Object.Instantiate(prefab, root) : NewRule<OneWayPath>(name, root);
    }

    private RotateButton CreateRotator(Transform root)
    {
        return rotateButtonPrefab != null ? UnityEngine.Object.Instantiate(rotateButtonPrefab, root) : NewRule<RotateButton>("Rotate Button", root);
    }

    private T NewRule<T>(string name, Transform root) where T : Component
    {
        GameObject view = new GameObject(name);
        view.transform.SetParent(root);
        MapTile tile = view.AddComponent<MapTile>();
        T rule = view.AddComponent<T>();
        BindRule(rule, tile);
        return rule;
    }

    private void BindRule(Component rule, MapTile tile)
    {
        if (rule is BlockTile block) block.BindTile(tile);
        else if (rule is OneWayPath path) path.BindTile(tile);
        else if (rule is RotateButton button) button.BindTile(tile);
    }

    private MapTile NewTile(Transform root)
    {
        GameObject view = new GameObject("Tile");
        view.transform.SetParent(root);
        return view.AddComponent<MapTile>();
    }

    private T NewComponent<T>(string name, Transform root) where T : Component
    {
        GameObject view = new GameObject(name);
        view.transform.SetParent(root);
        return view.AddComponent<T>();
    }

    private T Rename<T>(T component, string name) where T : Component
    {
        component.name = name;
        return component;
    }
}
