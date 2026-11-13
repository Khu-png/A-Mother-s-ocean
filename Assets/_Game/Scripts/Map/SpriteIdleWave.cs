using UnityEngine;

// Feeds the Sprite Fish Idle shader the sprite's area inside its texture, so the idle wave
// works on sprites cut from a sheet, plus a phase so several fish do not move in lockstep.
public static class SpriteIdleWave
{
    private static readonly int SPRITE_RECT = Shader.PropertyToID("_SpriteRect");
    private static readonly int PHASE = Shader.PropertyToID("_Phase");

    // Call again whenever the renderer's sprite changes (for example after a skin change).
    public static void Bind(SpriteRenderer renderer, MaterialPropertyBlock block, float phase)
    {
        Sprite sprite = renderer.sprite;
        Rect rect = sprite.textureRect;
        Texture texture = sprite.texture;
        renderer.GetPropertyBlock(block);
        block.SetVector(SPRITE_RECT, new Vector4(rect.x / texture.width, rect.y / texture.height,
            rect.width / texture.width, rect.height / texture.height));
        block.SetFloat(PHASE, phase);
        renderer.SetPropertyBlock(block);
    }
}
