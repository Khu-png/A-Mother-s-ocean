using System.Collections.Generic;
using UnityEngine;

public partial class GridMap : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private Color gridColor = new Color(0.15f, 0.55f, 0.7f, 0.7f);
    [SerializeField] private Material gridMaterial;
    [SerializeField] private Color pathColor = new Color(0.12f, 0.8f, 0.95f, 0.28f);

    [Header("Layout")]
    [SerializeField] private LevelData levelData;
    [SerializeField] private string[] mapRows =
    {
        "F|T|A.",
        ".SAO-.",
        "XOTOTX",
        ".XOT-.",
        "..XO.."
    };

    [Header("Visual")]
    [SerializeField] private Sprite targetSprite;
    [SerializeField] private MapPrefabSet prefabs = new MapPrefabSet();

    [Header("References")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private CameraController cameraController;

    private int width;
    private int height;
    private Player player;
    private FinishPoint finishPoint;
    private PathTrail pathTrail;
    private Transform runtimeRoot;
    private Vector2Int startCell;
    private Vector2Int finishCell;
    private Vector2Int playerCell;
    private readonly Dictionary<Vector2Int, MapTile> tiles = new Dictionary<Vector2Int, MapTile>();
    private readonly Dictionary<Vector2Int, BlockTile> blocks = new Dictionary<Vector2Int, BlockTile>();
    private readonly Dictionary<Vector2Int, OneWayPath> oneWayPaths = new Dictionary<Vector2Int, OneWayPath>();
    private readonly Dictionary<Vector2Int, RotateButton> rotateButtons = new Dictionary<Vector2Int, RotateButton>();
    private readonly Dictionary<Vector2Int, TargetPoint> targets = new Dictionary<Vector2Int, TargetPoint>();
    private readonly List<Vector2Int> currentRoute = new List<Vector2Int>();
    private readonly GridInputReader inputReader = new GridInputReader();

    void Update()
    {
        if (isClearingBoard) return;
        RefreshSafeArenaLayout();
        ReadMoveInput();
    }

    public void ResetPlayerToStart()
    {
        inputReader.Reset();
        if (player == null) return;
        pathTrail.Clear();
        ResetTargets();
        ResetOneWayPaths();
        playerCell = startCell;
        currentRoute.Clear();
        currentRoute.Add(playerCell);
        player.SetCell(playerCell, CellToLocalPosition(playerCell), cellSize);
        MarkCurrentCell();
        RefreshFinish();
    }

    public void SetCameraController(CameraController controller)
    {
        cameraController = controller;
        RefreshSafeArenaLayout();
    }

    private void BuildTile(Transform root, Vector2Int cell, char code)
    {
        MapTile tile = prefabs.CreateTile(root, cell, code,
            CellToLocalPosition(cell), cellSize, out BlockTile block,
            out OneWayPath path, out RotateButton button);
        tiles.Add(cell, tile);
        if (block != null) blocks.Add(cell, block);
        if (path != null) oneWayPaths.Add(cell, path);
        if (button != null) rotateButtons.Add(cell, button);

        if (MapCode.IsStart(code)) BuildStart(cell);
        else if (MapCode.IsFinish(code)) BuildFinish(cell);
        else if (MapCode.IsTarget(code)) BuildTarget(cell);
    }

    private void BuildStart(Vector2Int cell)
    {
        startCell = cell;
        startPoint = prefabs.CreateStart(runtimeRoot);
        startPoint.Setup(cell, CellToLocalPosition(cell), Color.white);
    }

    private void BuildFinish(Vector2Int cell)
    {
        finishCell = cell;
        finishPoint = prefabs.CreateFinish(runtimeRoot);
        finishPoint.Setup(cell, CellToLocalPosition(cell), Color.white, Color.white);
    }

    private void BuildTarget(Vector2Int cell)
    {
        TargetPoint target = prefabs.CreateTarget(runtimeRoot, cell);
        target.Setup(cell, CellToLocalPosition(cell), targetSprite, Color.white);
        targets.Add(cell, target);
    }

    private void BuildPlayer()
    {
        player = Instantiate(playerPrefab, runtimeRoot);
        playerCell = startCell;
        player.SetCell(playerCell, CellToLocalPosition(playerCell), cellSize);
        currentRoute.Clear();
        currentRoute.Add(playerCell);
    }
}
