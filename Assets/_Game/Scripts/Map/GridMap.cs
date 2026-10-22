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
        BlockTile block;
        OneWayPath path;
        RotateButton button;
        MapTile tile = prefabs.CreateTile(root, cell, code, CellToLocalPosition(cell), cellSize, out block, out path, out button);
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

    private void ReadMoveInput()
    {
        bool canMove = !LoadingManager.Ins.IsLoading && GameManager.IsState(GameState.Gameplay) && player != null && !player.IsMoving && !AreTilesRotating();
        Vector2Int direction = inputReader.ReadDirection(canMove);
        if (direction != Vector2Int.zero) TryMove(direction);
    }

    private bool AreTilesRotating()
    {
        foreach (OneWayPath path in oneWayPaths.Values)
        {
            if (path.Tile.IsRotating) return true;
        }
        return false;
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int nextCell = playerCell + direction;
        if (!CanMove(playerCell, nextCell, direction)) return;
        bool isBacktracking = IsBacktrackingToPreviousCell(nextCell);
        bool rotatesTiles = rotateButtons.ContainsKey(isBacktracking ? playerCell : nextCell);

        if (isBacktracking)
        {
            RestoreTargetAt(playerCell);
            if (rotateButtons.ContainsKey(playerCell)) RevertOneWayTiles();
            pathTrail.Remove(playerCell);
            currentRoute.RemoveAt(currentRoute.Count - 1);
        }
        else if (pathTrail.HasMark(nextCell)) return;
        else currentRoute.Add(nextCell);

        playerCell = nextCell;
        MarkCurrentCell();
        player.MoveTo(playerCell, CellToLocalPosition(playerCell));
        SoundManager.Ins.PlaySound(GameConstants.MOVE_SOUND);
        LevelManager.Ins.NotifyPlayerMoved();
        if (!isBacktracking && rotateButtons.ContainsKey(playerCell)) ActivateRotateButton();
        if (rotatesTiles) SoundManager.Ins.PlaySound(GameConstants.ROTATE_SOUND);
        RefreshFinish();
    }

    private bool CanMove(Vector2Int fromCell, Vector2Int toCell, Vector2Int direction)
    {
        if (!tiles.ContainsKey(toCell) || blocks.ContainsKey(toCell)) return false;
        if (IsBacktrackingToPreviousCell(toCell)) return true;
        return AllowsDirection(fromCell, direction) && AllowsDirection(toCell, -direction);
    }

    private bool AllowsDirection(Vector2Int cell, Vector2Int direction)
    {
        return !oneWayPaths.ContainsKey(cell) || oneWayPaths[cell].Allows(direction);
    }

    private void RotateOneWayTiles()
    {
        foreach (OneWayPath path in oneWayPaths.Values) path.ToggleState();
    }

    private void ActivateRotateButton()
    {
        RotateOneWayTiles();
        rotateButtons[playerCell].Pulse();
    }

    private void ResetOneWayPaths()
    {
        foreach (OneWayPath path in oneWayPaths.Values) path.ResetState();
    }

    private void MarkCurrentCell()
    {
        pathTrail.Mark(playerCell, CellToLocalPosition(playerCell));
        if (targets.ContainsKey(playerCell)) targets[playerCell].Collect();
    }

    private void ResetTargets()
    {
        foreach (TargetPoint target in targets.Values) target.ResetTarget();
    }

    private void RestoreTargetAt(Vector2Int cell)
    {
        if (targets.ContainsKey(cell)) targets[cell].ResetTarget();
    }

    private void RevertOneWayTiles()
    {
        foreach (OneWayPath path in oneWayPaths.Values) path.ToggleBack();
    }

    private void RefreshFinish()
    {
        if (finishPoint == null) return;
        finishPoint.SetOpen(AreAllTargetsCollected());
        if (finishPoint.IsOpen && playerCell == finishCell) LevelManager.Ins.OnWin();
    }

    private bool AreAllTargetsCollected()
    {
        foreach (TargetPoint target in targets.Values)
        {
            if (!target.IsCollected) return false;
        }

        return true;
    }

    private bool IsBacktrackingToPreviousCell(Vector2Int nextCell)
    {
        return currentRoute.Count >= 2 && currentRoute[currentRoute.Count - 2] == nextCell;
    }

}
