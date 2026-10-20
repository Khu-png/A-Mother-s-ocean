using System.Collections.Generic;
using UnityEngine;

public static class MapAiMovementGraph
{
    private static readonly Vector2Int[] DIRECTIONS =
    {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
    };

    public static int DirectionCount => DIRECTIONS.Length;

    public static Vector2Int Direction(int index) { return DIRECTIONS[index]; }

    public static void GetOutgoing(MapAiMapData map, Vector2Int cell,
        List<Vector2Int> results)
    {
        results.Clear();
        for (int i = 0; i < DIRECTIONS.Length; i++)
        {
            Vector2Int next = cell + DIRECTIONS[i];
            if (CanMove(map, cell, next)) results.Add(next);
        }
    }

    public static void GetIncoming(MapAiMapData map, Vector2Int cell,
        List<Vector2Int> results)
    {
        results.Clear();
        for (int i = 0; i < DIRECTIONS.Length; i++)
        {
            Vector2Int previous = cell + DIRECTIONS[i];
            if (CanMove(map, previous, cell)) results.Add(previous);
        }
    }

    public static void GetTwoWay(MapAiMapData map, Vector2Int cell,
        List<Vector2Int> results)
    {
        results.Clear();
        for (int i = 0; i < DIRECTIONS.Length; i++)
        {
            Vector2Int next = cell + DIRECTIONS[i];
            if (CanMove(map, cell, next) && CanMove(map, next, cell)) results.Add(next);
        }
    }

    public static bool CanMove(MapAiMapData map, Vector2Int from, Vector2Int to)
    {
        if (!map.Contains(from) || !map.Contains(to)) return false;
        char fromCode = map.GetCell(from);
        char toCode = map.GetCell(to);
        if (MapCode.IsBlock(fromCode) || MapCode.IsBlock(toCode)) return false;
        Vector2Int editorDirection = to - from;
        Vector2Int gameDirection = new Vector2Int(editorDirection.x, -editorDirection.y);
        return Allows(fromCode, gameDirection) && Allows(toCode, -gameDirection);
    }

    private static bool Allows(char code, Vector2Int direction)
    {
        if (!MapCode.IsOneWay(code)) return true;
        OneWayPathState state = MapCode.PathState(code);
        if (state == OneWayPathState.Vertical) return direction == Vector2Int.up || direction == Vector2Int.down;
        if (state == OneWayPathState.Horizontal) return direction == Vector2Int.left || direction == Vector2Int.right;
        if (state == OneWayPathState.LeftDown) return direction == Vector2Int.left || direction == Vector2Int.down;
        if (state == OneWayPathState.LeftUp) return direction == Vector2Int.left || direction == Vector2Int.up;
        if (state == OneWayPathState.UpRight) return direction == Vector2Int.up || direction == Vector2Int.right;
        return direction == Vector2Int.right || direction == Vector2Int.down;
    }
}
