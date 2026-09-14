using UnityEngine;

public static class MapCode
{
    public static bool IsStart(char code)
    {
        return code == 'S';
    }

    public static bool IsFinish(char code)
    {
        return code == 'F';
    }

    public static bool IsTarget(char code)
    {
        return code == 'T';
    }

    public static bool IsBlock(char code)
    {
        return code == 'X';
    }

    public static bool IsOneWay(char code)
    {
        return code == 'U' || code == 'R' || code == 'D' || code == 'L'
            || code == '|' || code == '-' || code == 'A' || code == 'B'
            || code == 'C' || code == 'E';
    }

    public static bool IsRotator(char code)
    {
        return code == 'O';
    }

    public static OneWayPathKind PathKind(char code)
    {
        if (code == 'A' || code == 'B' || code == 'C' || code == 'E') return OneWayPathKind.Corner;
        return OneWayPathKind.Straight;
    }

    public static OneWayPathState PathState(char code)
    {
        if (code == 'R' || code == 'L' || code == '-') return OneWayPathState.Horizontal;
        if (code == 'A') return OneWayPathState.LeftDown;
        if (code == 'B') return OneWayPathState.LeftUp;
        if (code == 'C') return OneWayPathState.UpRight;
        if (code == 'E') return OneWayPathState.RightDown;
        return OneWayPathState.Vertical;
    }
}
