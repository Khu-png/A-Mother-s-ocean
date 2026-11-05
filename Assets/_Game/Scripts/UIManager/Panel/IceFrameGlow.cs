using UnityEngine;
using UnityEngine.UI;

public sealed class IceFrameGlow : MaskableGraphic
{
    [SerializeField] private Color glowColor = new(0.1f, 0.75f, 1f, 0.18f);
    [SerializeField] private float glowDepth = 260f;

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = rectTransform.rect;
        float depth = Mathf.Min(glowDepth, rect.width * 0.5f, rect.height * 0.5f);
        Color clear = glowColor;
        clear.a = 0f;
        AddQuad(mesh,
            new Vector2(rect.xMin, rect.yMax - depth), new Vector2(rect.xMin, rect.yMax),
            new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMax - depth),
            clear, glowColor, glowColor, clear);
        AddQuad(mesh,
            new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMin + depth),
            new Vector2(rect.xMax, rect.yMin + depth), new Vector2(rect.xMax, rect.yMin),
            glowColor, clear, clear, glowColor);
        AddQuad(mesh,
            new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax),
            new Vector2(rect.xMin + depth, rect.yMax), new Vector2(rect.xMin + depth, rect.yMin),
            glowColor, glowColor, clear, clear);
        AddQuad(mesh,
            new Vector2(rect.xMax - depth, rect.yMin), new Vector2(rect.xMax - depth, rect.yMax),
            new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin),
            clear, clear, glowColor, glowColor);
    }

    private static void AddQuad(VertexHelper mesh, Vector2 bottomLeft,
        Vector2 topLeft, Vector2 topRight, Vector2 bottomRight,
        Color bottomLeftColor, Color topLeftColor,
        Color topRightColor, Color bottomRightColor)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(bottomLeft, bottomLeftColor, Vector2.zero);
        mesh.AddVert(topLeft, topLeftColor, Vector2.up);
        mesh.AddVert(topRight, topRightColor, Vector2.one);
        mesh.AddVert(bottomRight, bottomRightColor, Vector2.right);
        mesh.AddTriangle(start, start + 1, start + 2);
        mesh.AddTriangle(start, start + 2, start + 3);
    }
}
