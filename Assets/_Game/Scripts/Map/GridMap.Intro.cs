using UnityEngine;

public partial class GridMap
{
    [Header("Intro")]
    [SerializeField] private BoardIntroMotion introMotion = new BoardIntroMotion();

    public bool IsPlayingIntro => introMotion.IsPlaying;

    public void PlayIntro()
    {
        foreach (MapTile tile in tiles.Values) tile.BeginEntrance();
        startPoint.BeginEntrance();
        introMotion.Begin();
        ApplyIntro();
    }

    private void AdvanceIntro()
    {
        introMotion.Advance(Time.unscaledDeltaTime);
        ApplyIntro();
    }

    private void ApplyIntro()
    {
        ApplyRootScale();
        ApplyTileEntrance();
        ApplyPieceEntrance(introMotion.PieceFactor);
    }

    // Board cells are centered on the root, so scaling the root zooms around the board center.
    private void ApplyRootScale()
    {
        runtimeRoot.localScale = Vector3.one * (layoutRootScale * introMotion.CurrentFactor);
    }

    // Tiles near the center appear first, matching the circular reveal of the loading cover.
    private void ApplyTileEntrance()
    {
        Vector2 center = new Vector2(width - 1, height - 1) * 0.5f;
        float farthest = Mathf.Max(0.5f, center.magnitude);
        foreach (MapTile tile in tiles.Values)
        {
            float waveOrder = Vector2.Distance(tile.Cell, center) / farthest;
            tile.ApplyEntranceScale(introMotion.TileFactor(waveOrder));
        }
    }

    private void ApplyPieceEntrance(float factor)
    {
        startPoint.ApplyEntranceScale(factor);
        finishPoint.ApplyEntranceScale(factor);
        foreach (TargetPoint target in targets.Values) target.ApplyEntranceScale(factor);
        foreach (IceCover ice in iceCovers.Values) ice.ApplyEntranceScale(factor);
        player.SetRootScale(factor);
    }
}
