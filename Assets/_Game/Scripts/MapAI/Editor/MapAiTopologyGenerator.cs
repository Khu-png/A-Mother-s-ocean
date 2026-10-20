using System;
using System.Collections.Generic;
using UnityEngine;

public static class MapAiTopologyGenerator
{
    private static readonly Vector2Int[] CARVE_DIRECTIONS =
    {
        new Vector2Int(2, 0), new Vector2Int(-2, 0),
        new Vector2Int(0, 2), new Vector2Int(0, -2)
    };

    public static bool TryCreate(MapAiMapData current, bool[] locks,
        MapAiGenerationSettings settings, System.Random random, out char[,] cells,
        out List<Vector2Int> mainPath, out string issue)
    {
        cells = CreateBlocked(settings.Columns, settings.Rows);
        CarveMaze(cells, random);
        OpenExtraConnections(cells, random, settings.ExtraConnectionChance);
        ApplyLocks(cells, current, locks);
        issue = PlaceEndpoints(cells, random, locks, current);
        mainPath = new List<Vector2Int>();
        if (issue != string.Empty) return false;

        MapAiAnalysisResult topology = MapAiAnalyzer.Analyze(
            new MapAiMapData(ToRows(cells)), settings.SearchBudget, settings.Difficulty);
        if (!topology.IsReachable)
        {
            issue = "Các ô khóa làm S và F mất kết nối.";
            return false;
        }
        mainPath.AddRange(topology.ShortestPath);
        return AdjustRockRatio(cells, mainPath, current, locks,
            settings.RockRatio, random, out issue);
    }

    public static string[] ToRows(char[,] cells)
    {
        string[] rows = new string[cells.GetLength(1)];
        for (int y = 0; y < cells.GetLength(1); y++)
        {
            char[] row = new char[cells.GetLength(0)];
            for (int x = 0; x < cells.GetLength(0); x++) row[x] = cells[x, y];
            rows[y] = new string(row);
        }
        return rows;
    }

    public static bool IsLocked(Vector2Int cell, bool[] locks, MapAiMapData current)
    {
        return current.Contains(cell) && locks[cell.y * current.Width + cell.x];
    }

    private static char[,] CreateBlocked(int width, int height)
    {
        char[,] cells = new char[width, height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++) cells[x, y] = 'X';
        return cells;
    }

    private static void CarveMaze(char[,] cells, System.Random random)
    {
        List<Vector2Int> stack = new List<Vector2Int> { Vector2Int.zero };
        cells[0, 0] = '.';
        while (stack.Count > 0)
        {
            Vector2Int current = stack[stack.Count - 1];
            List<Vector2Int> candidates = UnvisitedNeighbors(cells, current);
            if (candidates.Count == 0)
            {
                stack.RemoveAt(stack.Count - 1);
                continue;
            }
            Vector2Int next = candidates[random.Next(candidates.Count)];
            Vector2Int middle = (current + next) / 2;
            cells[middle.x, middle.y] = '.';
            cells[next.x, next.y] = '.';
            stack.Add(next);
        }
    }

    private static List<Vector2Int> UnvisitedNeighbors(char[,] cells, Vector2Int cell)
    {
        List<Vector2Int> candidates = new List<Vector2Int>(4);
        for (int i = 0; i < CARVE_DIRECTIONS.Length; i++)
        {
            Vector2Int next = cell + CARVE_DIRECTIONS[i];
            if (Contains(cells, next) && cells[next.x, next.y] == 'X') candidates.Add(next);
        }
        return candidates;
    }

    private static void OpenExtraConnections(char[,] cells, System.Random random,
        float chance)
    {
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            if (cells[x, y] != 'X' || random.NextDouble() > chance) continue;
            bool horizontal = IsOpen(cells, x - 1, y) && IsOpen(cells, x + 1, y);
            bool vertical = IsOpen(cells, x, y - 1) && IsOpen(cells, x, y + 1);
            if (horizontal || vertical) cells[x, y] = '.';
        }
    }

    private static void ApplyLocks(char[,] cells, MapAiMapData current, bool[] locks)
    {
        int width = Mathf.Min(cells.GetLength(0), current.Width);
        int height = Mathf.Min(cells.GetLength(1), current.Height);
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            if (locks[y * current.Width + x]) cells[x, y] = current.GetCell(new Vector2Int(x, y));
    }

    private static string PlaceEndpoints(char[,] cells, System.Random random,
        bool[] locks, MapAiMapData current)
    {
        List<Vector2Int> starts = Find(cells, 'S');
        List<Vector2Int> finishes = Find(cells, 'F');
        if (starts.Count > 1 || finishes.Count > 1) return "Vùng khóa chứa nhiều hơn một S hoặc F.";
        List<Vector2Int> open = FindUnlockedOpen(cells, locks, current);
        int required = (starts.Count == 0 ? 1 : 0) + (finishes.Count == 0 ? 1 : 0);
        if (open.Count < required) return "Không đủ ô mở và không khóa để đặt S và F.";

        Vector2Int start = starts.Count == 1 ? starts[0] : open[random.Next(open.Count)];
        Vector2Int finish = finishes.Count == 1 ? finishes[0]
            : Farthest(cells, start, locks, current);
        if (starts.Count == 0 && finishes.Count == 1)
            start = Farthest(cells, finish, locks, current);
        if (starts.Count == 0) cells[start.x, start.y] = 'S';
        if (finishes.Count == 0) cells[finish.x, finish.y] = 'F';
        return start == finish ? "S và F không thể nằm trên cùng một ô." : string.Empty;
    }

    private static Vector2Int Farthest(char[,] cells, Vector2Int start,
        bool[] locks, MapAiMapData currentMap)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int> { { start, 0 } };
        queue.Enqueue(start);
        Vector2Int farthest = start;
        int farthestDistance = -1;
        while (queue.Count > 0)
        {
            Vector2Int candidate = queue.Dequeue();
            if (!IsLocked(candidate, locks, currentMap) && distance[candidate] > farthestDistance)
            {
                farthest = candidate;
                farthestDistance = distance[candidate];
            }
            VisitOpenNeighbors(cells, candidate, distance, queue);
        }
        return farthest;
    }

    private static void VisitOpenNeighbors(char[,] cells, Vector2Int cell,
        Dictionary<Vector2Int, int> distance, Queue<Vector2Int> queue)
    {
        for (int i = 0; i < MapAiMovementGraph.DirectionCount; i++)
        {
            Vector2Int next = cell + MapAiMovementGraph.Direction(i);
            if (!Contains(cells, next) || cells[next.x, next.y] == 'X'
                || distance.ContainsKey(next)) continue;
            distance.Add(next, distance[cell] + 1);
            queue.Enqueue(next);
        }
    }

    private static bool AdjustRockRatio(char[,] cells, List<Vector2Int> path,
        MapAiMapData current, bool[] locks, float ratio, System.Random random,
        out string issue)
    {
        int desired = Mathf.RoundToInt(cells.Length * ratio);
        int currentCount = Find(cells, 'X').Count;
        List<Vector2Int> candidates = RockCandidates(cells, path, current, locks,
            currentCount > desired);
        Shuffle(candidates, random);
        int changes = Mathf.Abs(currentCount - desired);
        if (candidates.Count < changes)
        {
            issue = "Ô khóa và tuyến chính không cho phép đạt tỷ lệ đá yêu cầu.";
            return false;
        }
        char replacement = currentCount > desired ? '.' : 'X';
        for (int i = 0; i < changes; i++) cells[candidates[i].x, candidates[i].y] = replacement;
        issue = string.Empty;
        return true;
    }

    private static List<Vector2Int> RockCandidates(char[,] cells, List<Vector2Int> path,
        MapAiMapData current, bool[] locks, bool opening)
    {
        HashSet<Vector2Int> protectedCells = new HashSet<Vector2Int>(path);
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            if (protectedCells.Contains(cell) || IsLocked(cell, locks, current)) continue;
            if (opening ? cells[x, y] == 'X' : cells[x, y] == '.') candidates.Add(cell);
        }
        return candidates;
    }

    private static List<Vector2Int> Find(char[,] cells, char code)
    {
        List<Vector2Int> found = new List<Vector2Int>();
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
            if (cells[x, y] == code) found.Add(new Vector2Int(x, y));
        return found;
    }

    private static List<Vector2Int> FindUnlockedOpen(char[,] cells, bool[] locks,
        MapAiMapData current)
    {
        List<Vector2Int> found = new List<Vector2Int>();
        for (int y = 0; y < cells.GetLength(1); y++)
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            if (cells[x, y] != 'X' && !IsLocked(cell, locks, current)) found.Add(cell);
        }
        return found;
    }

    private static void Shuffle(List<Vector2Int> cells, System.Random random)
    {
        for (int i = cells.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (cells[i], cells[swapIndex]) = (cells[swapIndex], cells[i]);
        }
    }

    private static bool Contains(char[,] cells, Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0
            && cell.x < cells.GetLength(0) && cell.y < cells.GetLength(1);
    }

    private static bool IsOpen(char[,] cells, int x, int y)
    {
        return x >= 0 && y >= 0 && x < cells.GetLength(0)
            && y < cells.GetLength(1) && cells[x, y] != 'X';
    }
}
