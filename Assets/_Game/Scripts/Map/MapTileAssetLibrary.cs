using UnityEngine;

public static class MapTileAssetLibrary
{
    private const string RootPath = "MapTiles/";

    public static Sprite Sprite(string assetName)
    {
        Sprite sprite = Resources.Load<Sprite>(RootPath + assetName);
        if (sprite != null) return sprite;

        Sprite[] sprites = Resources.LoadAll<Sprite>(RootPath + assetName);
        return sprites.Length > 0 ? sprites[0] : null;
    }

    public static Sprite SpriteOrFallback(string assetName, Sprite fallback)
    {
        Sprite sprite = Sprite(assetName);
        return sprite != null ? sprite : fallback;
    }
}
