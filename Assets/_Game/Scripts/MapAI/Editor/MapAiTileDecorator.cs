using System.Collections.Generic;
using UnityEngine;

public static class MapAiTileDecorator
{
    private static readonly char[] STRAIGHT_CODES = { '|', '-' };
    private static readonly char[] CORNER_CODES = { 'A', 'B', 'C', 'E' };

    public static bool TryDecorate(char[,] cells, List<Vector2Int> mainPath,
        MapAiMapData current, bool[] locks, MapAiGenerationSettings settings,
        System.Random random, out string issue)
    {
        if (!PlaceTargets(cells, mainPath, current, locks,
            settings.TargetCount, random, out issue)) return false;

        List<Vector2Int> eligible = FindEligible(cells);
        int rotateCount = Mathf.RoundToInt(eligible.Count * settings.RotateRatio);
        int straightCount = Mathf.RoundToInt(eligible.Count * settings.StraightOneWayRatio);
        int cornerCount = Mathf.RoundToInt(eligible.Count * settings.CornerOneWayRatio);
        if (!ValidateExistingCounts(cells, rotateCount,
            straightCount, cornerCount, out issue)) return false;

        HashSet<Vector2Int> route = new HashSet<Vector2Int>(mainPath);
        int missingRotate = rotateCount - CountAll(cells, IsRotate);
        int missingStraight = straightCount - CountAll(cells, IsStraight);
        int missingCorner = cornerCount - CountAll(cells, IsCorner);
        if (!PlaceRotateOffRoute(cells, current, locks, route, missingRotate, random))
        {
            issue = "Không đủ ô ngoài tuyến chính để đặt tỷ lệ ô xoay yêu cầu.";
            return false;
        }
        if (!PlaceDirectional(cells, mainPath, current, locks, route,
            missingStraight, STRAIGHT_CODES, true, random)
            || !PlaceDirectional(cells, mainPath, current, locks, route,
                missingCorner, CORNER_CODES, false, random))
        {
            issue = "Không đủ vị trí hợp hướng để đặt tỷ lệ ô one-way yêu cầu.";
            return false;
        }
        issue = string.Empty;
        return true;
    }

    private static bool PlaceTargets(char[,] cells, List<Vector2Int> path,
        MapAiMapData current, bool[] locks, int desired, System.Random random,
        out string issue)
    {
        int lockedCount = CountLocked(cells, current, locks, IsTarget);
        if (lockedCount > desired)
        {
            issue = "Số target bị khóa lớn hơn số target yêu cầu.";
            return false;
        }
        if (HasTargetOutsidePath(cells, path, current, locks))
        {
            issue = "Target bị khóa không nằm trên tuyến hoàn thành.";
            return false;
        }
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2Int cell = path[i];
            if (cells[cell.x, cell.y] == '.'
                && !MapAiTopologyGenerator.IsLocked(cell, locks, current)) candidates.Add(cell);
        }
        Shuffle(candidates, random);
        int missing = desired - lockedCount;
        if (candidates.Count < missing)
        {
            issue = "Tuyến chính không đủ vị trí để đặt số target yêu cầu.";
            return false;
        }
        for (int i = 0; i < missing; i++) cells[candidates[i].x, candidates[i].y] = 'T';
        issue = string.Empty;
        return true;
    }

    private static bool ValidateExistingCounts(char[,] cells,
        int rotate, int straight, int corner, out string issue)
    {
        if (CountAll(cells, IsRotate) > rotate)
            issue = "Puzzle focus hoặc ô khóa cần nhiều ô xoay hơn tỷ lệ đã chọn.";
        else if (CountAll(cells, IsStraight) > straight)
            issue = "Puzzle focus hoặc ô khóa cần nhiều one-way thẳng hơn tỷ lệ đã chọn.";
        else if (CountAll(cells, IsCorner) > corner)
            issue = "Puzzle focus hoặc ô khóa cần nhiều one-way góc hơn tỷ lệ đã chọn.";
        else
        {
            issue = string.Empty;
            return true;
        }
        return false;
    }

    private static int CountAll(char[,] cells, System.Predicate<char> matches)
    {
        int count = 0;
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
            if (matches(cells[x, y])) count++;
        return count;
    }

    private static bool PlaceDirectional(char[,] cells, List<Vector2Int> path,
        MapAiMapData current, bool[] locks, HashSet<Vector2Int> route, int count,
        char[] codes, bool straight, System.Random random)
    {
        int placed = PlaceOffRouteCount(cells, current, locks, route,
            count, codes, random);
        for (int i = 1; i < path.Count - 1 && placed < count; i++)
        {
            Vector2Int cell = path[i];
            if (cells[cell.x, cell.y] != '.'
                || MapAiTopologyGenerator.IsLocked(cell, locks, current)) continue;
            char code = RouteCode(path[i - 1], cell, path[i + 1], straight);
            if (code == '\0') continue;
            cells[cell.x, cell.y] = code;
            placed++;
        }
        return placed == count;
    }

    private static bool PlaceRotateOffRoute(char[,] cells, MapAiMapData current,
        bool[] locks, HashSet<Vector2Int> route, int count, System.Random random)
    {
        return PlaceOffRouteCount(cells, current, locks, route,
            count, new[] { 'O' }, random) == count;
    }

    private static int PlaceOffRouteCount(char[,] cells, MapAiMapData current,
        bool[] locks, HashSet<Vector2Int> route, int count, char[] codes,
        System.Random random)
    {
        List<Vector2Int> candidates = FindOpenUnlocked(cells, current, locks, route);
        Shuffle(candidates, random);
        int placed = Mathf.Min(count, candidates.Count);
        for (int i = 0; i < placed; i++)
            cells[candidates[i].x, candidates[i].y] = codes[random.Next(codes.Length)];
        return placed;
    }

    public static char RouteCode(Vector2Int previous, Vector2Int cell,
        Vector2Int next, bool straight)
    {
        Vector2Int first = ToGameDirection(previous - cell);
        Vector2Int second = ToGameDirection(next - cell);
        if (straight)
        {
            if (first.x != 0 && second.x != 0) return '-';
            if (first.y != 0 && second.y != 0) return '|';
            return '\0';
        }
        if (Has(first, second, Vector2Int.left, Vector2Int.down)) return 'A';
        if (Has(first, second, Vector2Int.left, Vector2Int.up)) return 'B';
        if (Has(first, second, Vector2Int.up, Vector2Int.right)) return 'C';
        if (Has(first, second, Vector2Int.right, Vector2Int.down)) return 'E';
        return '\0';
    }

    private static Vector2Int ToGameDirection(Vector2Int direction)
    {
        return new Vector2Int(direction.x, -direction.y);
    }

    private static bool Has(Vector2Int first, Vector2Int second,
        Vector2Int expectedFirst, Vector2Int expectedSecond)
    {
        return first == expectedFirst && second == expectedSecond
            || first == expectedSecond && second == expectedFirst;
    }

    private static List<Vector2Int> FindEligible(char[,] cells)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
            if (cells[x, y] != 'X' && cells[x, y] != 'S'
                && cells[x, y] != 'F' && cells[x, y] != 'T')
                result.Add(new Vector2Int(x, y));
        return result;
    }

    private static List<Vector2Int> FindOpenUnlocked(char[,] cells,
        MapAiMapData current, bool[] locks, HashSet<Vector2Int> excluded)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            if (cells[x, y] == '.' && !excluded.Contains(cell)
                && !MapAiTopologyGenerator.IsLocked(cell, locks, current)) result.Add(cell);
        }
        return result;
    }

    private static int CountLocked(char[,] cells, MapAiMapData current,
        bool[] locks, System.Predicate<char> matches)
    {
        int count = 0;
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            if (MapAiTopologyGenerator.IsLocked(cell, locks, current)
                && matches(cells[x, y])) count++;
        }
        return count;
    }

    private static bool HasTargetOutsidePath(char[,] cells, List<Vector2Int> path,
        MapAiMapData current, bool[] locks)
    {
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            if (cells[x, y] == 'T' && MapAiTopologyGenerator.IsLocked(cell, locks, current)
                && !path.Contains(cell)) return true;
        }
        return false;
    }

    private static bool IsTarget(char code) { return code == 'T'; }
    private static bool IsRotate(char code) { return code == 'O'; }
    private static bool IsStraight(char code) { return code == '|' || code == '-'; }
    private static bool IsCorner(char code) { return code == 'A' || code == 'B' || code == 'C' || code == 'E'; }

    private static void Shuffle(List<Vector2Int> cells, System.Random random)
    {
        for (int i = cells.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (cells[i], cells[swapIndex]) = (cells[swapIndex], cells[i]);
        }
    }
}
