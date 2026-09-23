using UnityEngine;

public enum OneWayPathKind
{
    Straight,
    Corner
}

public enum OneWayPathState
{
    Vertical,
    Horizontal,
    LeftDown,
    LeftUp,
    UpRight,
    RightDown
}

public enum OneWayPathRotation
{
    Rotation0,
    Rotation90,
    Rotation180,
    Rotation270
}

public class OneWayPath : MonoBehaviour
{
    [SerializeField] private MapTile tile;
    [SerializeField] private OneWayPathKind kind;
    [SerializeField] private OneWayPathState state;
    [SerializeField] private OneWayPathState initialState;
    [SerializeField] private OneWayPathRotation visualRotation;
    [SerializeField] private Color pathColor = new Color(0.02f, 0.45f, 0.62f, 1f);

    public OneWayPathKind Kind => kind;
    public OneWayPathState State => state;
    public MapTile Tile => tile;

    public void BindTile(MapTile setupTile)
    {
        tile = setupTile;
    }

    public void Setup(MapTile setupTile, OneWayPathKind setupKind, OneWayPathState setupState, Color color)
    {
        tile = setupTile;
        kind = setupKind;
        state = setupState;
        initialState = setupState;
        pathColor = color;
        ApplyVisual();
    }

    public bool Allows(Vector2Int moveDirection)
    {
        if (state == OneWayPathState.Vertical) return moveDirection == Vector2Int.up || moveDirection == Vector2Int.down;
        if (state == OneWayPathState.Horizontal) return moveDirection == Vector2Int.left || moveDirection == Vector2Int.right;
        if (state == OneWayPathState.LeftDown) return moveDirection == Vector2Int.left || moveDirection == Vector2Int.down;
        if (state == OneWayPathState.LeftUp) return moveDirection == Vector2Int.left || moveDirection == Vector2Int.up;
        if (state == OneWayPathState.UpRight) return moveDirection == Vector2Int.up || moveDirection == Vector2Int.right;
        return moveDirection == Vector2Int.right || moveDirection == Vector2Int.down;
    }

    public void ToggleState()
    {
        if (kind == OneWayPathKind.Straight)
        {
            state = state == OneWayPathState.Vertical ? OneWayPathState.Horizontal : OneWayPathState.Vertical;
        }
        else
        {
            ToggleCornerState();
        }

        ApplyVisual();
    }

    public void ToggleBack()
    {
        if (kind == OneWayPathKind.Straight)
        {
            state = state == OneWayPathState.Vertical ? OneWayPathState.Horizontal : OneWayPathState.Vertical;
        }
        else
        {
            ToggleCornerBack();
        }

        ApplyVisual();
    }

    public void ResetState()
    {
        state = initialState;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        visualRotation = RotationForState();
        Sprite sprite = tile.HasPrefabSprite ? tile.Sprite : MapTileAssetLibrary.Sprite(SpriteName());
        tile.SetSprite(sprite);
        tile.SetVisualRotation(RotationDegrees(visualRotation));
        tile.SetColor(sprite != null ? Color.white : pathColor);
        tile.SetLabel(sprite != null ? string.Empty : DirectionSymbol(), Color.white);
    }

    private string SpriteName()
    {
        return kind == OneWayPathKind.Straight ? "pipe_vertical" : "pipe_left_down";
    }

    private OneWayPathRotation RotationForState()
    {
        if (kind == OneWayPathKind.Straight) return state == OneWayPathState.Vertical ? OneWayPathRotation.Rotation90 : OneWayPathRotation.Rotation0;
        // Positive Z rotation is counterclockwise: left/down becomes right/down at 90 degrees.
        if (state == OneWayPathState.RightDown) return OneWayPathRotation.Rotation90;
        if (state == OneWayPathState.UpRight) return OneWayPathRotation.Rotation180;
        if (state == OneWayPathState.LeftUp) return OneWayPathRotation.Rotation270;
        return OneWayPathRotation.Rotation0;
    }

    private float RotationDegrees(OneWayPathRotation rotation)
    {
        if (rotation == OneWayPathRotation.Rotation90) return 90f;
        if (rotation == OneWayPathRotation.Rotation180) return 180f;
        if (rotation == OneWayPathRotation.Rotation270) return 270f;
        return 0f;
    }

    private void ToggleCornerState()
    {
        if (state == OneWayPathState.LeftDown) state = OneWayPathState.RightDown;
        else if (state == OneWayPathState.RightDown) state = OneWayPathState.UpRight;
        else if (state == OneWayPathState.UpRight) state = OneWayPathState.LeftUp;
        else state = OneWayPathState.LeftDown;
    }

    private void ToggleCornerBack()
    {
        if (state == OneWayPathState.LeftDown) state = OneWayPathState.LeftUp;
        else if (state == OneWayPathState.LeftUp) state = OneWayPathState.UpRight;
        else if (state == OneWayPathState.UpRight) state = OneWayPathState.RightDown;
        else state = OneWayPathState.LeftDown;
    }

    private string DirectionSymbol()
    {
        if (state == OneWayPathState.Vertical) return "|";
        if (state == OneWayPathState.Horizontal) return "-";
        if (state == OneWayPathState.LeftDown) return "LD";
        if (state == OneWayPathState.LeftUp) return "LU";
        if (state == OneWayPathState.UpRight) return "UR";
        return "RD";
    }
}
