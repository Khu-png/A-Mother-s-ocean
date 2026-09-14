using UnityEngine;

public class BlockTile : MonoBehaviour
{
    [SerializeField] private MapTile tile;
    [SerializeField] private Color blockColor = new Color(0.02f, 0.03f, 0.04f, 1f);

    public MapTile Tile => tile;

    public void BindTile(MapTile setupTile)
    {
        tile = setupTile;
    }

    public void Setup(MapTile setupTile, Color color)
    {
        tile = setupTile;
        blockColor = color;
        Sprite sprite = MapTileAssetLibrary.Sprite("tile_block");
        tile.SetSprite(sprite);
        tile.SetColor(sprite != null ? Color.white : blockColor);
        tile.SetLabel(sprite != null ? string.Empty : "X", Color.white);
    }
}
