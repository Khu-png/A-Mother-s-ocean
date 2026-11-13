using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Gives an Image the data the UI Fish Idle and UI Shine Sweep shaders need: each vertex's
// position inside the sprite (TEXCOORD1.xy), a phase (TEXCOORD1.z) and the sprite's uv rect
// (TEXCOORD2). The mesh rebuilds whenever the Image's sprite changes, so skin swaps stay correct.
public sealed class UISpriteLocalMesh : MonoBehaviour, IMeshModifier
{
    [SerializeField] private Image image;
    [SerializeField] private float phase;
    private readonly List<UIVertex> vertices = new List<UIVertex>();

    void OnEnable()
    {
        image.SetVerticesDirty();
    }

    void OnDisable()
    {
        image.SetVerticesDirty();
    }

    public void ModifyMesh(Mesh mesh)
    {
        using (VertexHelper helper = new VertexHelper(mesh))
        {
            ModifyMesh(helper);
            helper.FillMesh(mesh);
        }
    }

    public void ModifyMesh(VertexHelper helper)
    {
        // Graphic calls every IMeshModifier, even disabled ones; a disabled effect adds nothing.
        if (!isActiveAndEnabled) return;
        Vector4 rect = SpriteUvRect(image.sprite);
        vertices.Clear();
        helper.GetUIVertexStream(vertices);
        for (int index = 0; index < vertices.Count; index++)
        {
            UIVertex vertex = vertices[index];
            Vector2 uv = vertex.uv0;
            vertex.uv1 = new Vector4((uv.x - rect.x) / rect.z, (uv.y - rect.y) / rect.w, phase, 0f);
            vertex.uv2 = rect;
            vertices[index] = vertex;
        }
        helper.Clear();
        helper.AddUIVertexTriangleStream(vertices);
    }

    private static Vector4 SpriteUvRect(Sprite sprite)
    {
        Rect rect = sprite.textureRect;
        Texture texture = sprite.texture;
        return new Vector4(rect.x / texture.width, rect.y / texture.height,
            rect.width / texture.width, rect.height / texture.height);
    }
}
