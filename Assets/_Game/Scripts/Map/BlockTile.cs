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
        tile.SetColor(blockColor);
        tile.SetLabel(string.Empty, Color.white);
    }
}
