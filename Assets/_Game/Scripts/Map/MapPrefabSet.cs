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
        out BlockTile block, out OneWayPath path, out RotateButton button)
    {
        block = null;
        path = null;
        button = null;

        if (MapCode.IsBlock(code))
        {
            block = CreateBlock(root);
            block.Tile.Setup(cell, position, cellSize, Color.white);
            block.Setup(block.Tile, Color.white);
            return block.Tile;
        }

        if (MapCode.IsOneWay(code))
        {
            path = CreateOneWay(root, MapCode.PathKind(code));
            path.Tile.Setup(cell, position, cellSize, Color.white);
            path.Setup(path.Tile, MapCode.PathKind(code), MapCode.PathState(code), Color.white);
            return path.Tile;
        }

        if (MapCode.IsRotator(code))
        {
            button = CreateRotator(root);
            button.Tile.Setup(cell, position, cellSize, Color.white);
            button.Setup(button.Tile, Color.white);
            return button.Tile;
        }

        MapTile tile = UnityEngine.Object.Instantiate(emptyTilePrefab, root);
        tile.name = $"Tile {cell.x} {cell.y}";
        tile.Setup(cell, position, cellSize, Color.white);
        return tile;
    }

    public StartPoint CreateStart(Transform root)
    {
        return UnityEngine.Object.Instantiate(startPointPrefab, root);
    }

    public TargetPoint CreateTarget(Transform root, Vector2Int cell)
    {
        string name = $"Target {cell.x} {cell.y}";
        return Rename(UnityEngine.Object.Instantiate(targetPointPrefab, root), name);
    }

    public FinishPoint CreateFinish(Transform root)
    {
        return UnityEngine.Object.Instantiate(finishPointPrefab, root);
    }

    private BlockTile CreateBlock(Transform root)
    {
        return UnityEngine.Object.Instantiate(blockTilePrefab, root);
    }

    private OneWayPath CreateOneWay(Transform root, OneWayPathKind kind)
    {
        OneWayPath prefab = kind == OneWayPathKind.Corner ? oneWayCornerPrefab : oneWayStraightPrefab;
        return UnityEngine.Object.Instantiate(prefab, root);
    }

    private RotateButton CreateRotator(Transform root)
    {
        return UnityEngine.Object.Instantiate(rotateButtonPrefab, root);
    }

    private T Rename<T>(T component, string name) where T : Component
    {
        component.name = name;
        return component;
    }
}
