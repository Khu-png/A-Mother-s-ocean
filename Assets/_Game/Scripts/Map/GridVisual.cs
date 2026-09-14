using UnityEngine;

public static class GridVisual
{
    private static Sprite squareSprite;
    private static Sprite circleSprite;

    public static void BuildGrid(Transform parent, int width, int height, float cellSize, float lineWidth, Color color)
    {
        Transform gridRoot = new GameObject("Grid").transform;
        gridRoot.SetParent(parent);
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        float left = -width * cellSize * 0.5f;
        float right = width * cellSize * 0.5f;
        float bottom = -height * cellSize * 0.5f;
        float top = height * cellSize * 0.5f;

        for (int x = 0; x <= width; x++)
        {
            float worldX = left + x * cellSize;
            CreateLine(gridRoot, $"Vertical {x}", new Vector3(worldX, bottom, 0f), new Vector3(worldX, top, 0f), lineMaterial, lineWidth, color);
        }

        for (int y = 0; y <= height; y++)
        {
            float worldY = bottom + y * cellSize;
            CreateLine(gridRoot, $"Horizontal {y}", new Vector3(left, worldY, 0f), new Vector3(right, worldY, 0f), lineMaterial, lineWidth, color);
        }
    }

    public static Sprite SquareSprite()
    {
        if (squareSprite != null) return squareSprite;
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return squareSprite;
    }

    public static Sprite CircleSprite()
    {
        if (circleSprite != null) return circleSprite;
        const int textureSize = 32;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.42f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        return circleSprite;
    }

    private static void CreateLine(Transform parent, string lineName, Vector3 start, Vector3 end, Material material, float lineWidth, Color color)
    {
        LineRenderer line = new GameObject(lineName).AddComponent<LineRenderer>();
        line.transform.SetParent(parent);
        line.positionCount = 2;
        line.useWorldSpace = false;
        line.material = material;
        line.startColor = color;
        line.endColor = color;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.sortingOrder = 0;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}
