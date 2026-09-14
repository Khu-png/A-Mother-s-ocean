using System.Text;

public class MapBuilderGridState
{
    private char[,] cells;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public MapBuilderGridState(int width, int height)
    {
        Resize(width, height);
    }

    public char GetCell(int x, int y)
    {
        return cells[x, y];
    }

    public void SetCell(int x, int y, char code)
    {
        cells[x, y] = code;
    }

    public void Resize(int width, int height)
    {
        Width = UnityEngine.Mathf.Max(1, width);
        Height = UnityEngine.Mathf.Max(1, height);
        char[,] nextCells = CreateEmptyCells(Width, Height);

        if (cells != null)
        {
            int copyWidth = UnityEngine.Mathf.Min(Width, cells.GetLength(0));
            int copyHeight = UnityEngine.Mathf.Min(Height, cells.GetLength(1));
            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++) nextCells[x, y] = cells[x, y];
            }
        }

        cells = nextCells;
    }

    public void LoadRows(string[] rows)
    {
        int width = 1;
        int height = UnityEngine.Mathf.Max(1, rows.Length);
        for (int i = 0; i < rows.Length; i++) width = UnityEngine.Mathf.Max(width, rows[i].Length);
        Resize(width, height);

        for (int y = 0; y < Height; y++)
        {
            string row = y < rows.Length ? rows[y] : string.Empty;
            for (int x = 0; x < Width; x++) cells[x, y] = x < row.Length ? row[x] : '.';
        }
    }

    public string[] ToRows()
    {
        string[] rows = new string[Height];
        for (int y = 0; y < Height; y++)
        {
            StringBuilder builder = new StringBuilder(Width);
            for (int x = 0; x < Width; x++) builder.Append(cells[x, y]);
            rows[y] = builder.ToString();
        }

        return rows;
    }

    public string ToCSharpRows()
    {
        string[] rows = ToRows();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < rows.Length; i++) builder.AppendLine($"\"{rows[i]}\",");
        return builder.ToString();
    }

    private char[,] CreateEmptyCells(int width, int height)
    {
        char[,] nextCells = new char[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++) nextCells[x, y] = '.';
        }

        return nextCells;
    }
}
