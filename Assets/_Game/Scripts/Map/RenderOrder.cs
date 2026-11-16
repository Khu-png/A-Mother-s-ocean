// Single table of sorting orders for every gameplay renderer, back to front.
// Orders are spaced by 10 so a new element can slide into a band without renumbering.
// Set from code wherever a script owns the renderer; the prefab-baked ones (tile label,
// and the numbers shown in prefabs) mirror this table. UI canvases sit above all of it.
public static class RenderOrder
{
    public const int BACKGROUND = -100;
    public const int AMBIENT_BUBBLES = -90;
    public const int TILE_SHADOW = -30;
    public const int TILE_DEPTH = -20;
    public const int TILE_FACE = -10;
    public const int GRID_LINES = 0;
    public const int TILE_LABEL = 5;
    public const int START_POINT = 10;
    public const int PATH_TRAIL = 20;
    public const int FINISH_POINT = 30;
    public const int TARGET_POINT = 40;
    public const int PLAYER_SHADOW = 50;
    public const int PLAYER = 60;
    public const int EFFECT_BUBBLES = 70;
}
