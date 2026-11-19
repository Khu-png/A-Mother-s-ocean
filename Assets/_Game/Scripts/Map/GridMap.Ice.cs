using System.Collections.Generic;
using UnityEngine;

// Babies sealed in ice: the ice melts once enough other babies are rescued, and
// freezes again if the route is undone far enough to lose them.
public partial class GridMap
{
    private const int ICE_SHARD_CAPACITY = 110;
    private const int CRACK_SHARD_COUNT = 7;
    private const int MELT_SHARD_COUNT = 24;
    private IceShardBurst iceShards;
    private readonly Dictionary<Vector2Int, IceCover> iceCovers =
        new Dictionary<Vector2Int, IceCover>();

    private void BuildIce(Vector2Int cell, int threshold)
    {
        IceCover ice = prefabs.CreateIce(runtimeRoot, cell);
        ice.Setup(cell, CellToLocalPosition(cell), cellSize, threshold);
        iceCovers.Add(cell, ice);
    }

    private bool IsFrozen(Vector2Int cell)
    {
        return iceCovers.TryGetValue(cell, out IceCover ice) && ice.IsFrozen;
    }

    private void RefreshIce()
    {
        int collected = CollectedTargetCount();
        bool cracked = false;
        bool melted = false;
        foreach (IceCover ice in iceCovers.Values)
        {
            IceChange change = ice.Refresh(collected - (targets[ice.Cell].IsCollected ? 1 : 0));
            if (change == IceChange.None) continue;
            PlayIceShards(ice.Cell, change == IceChange.Melted);
            cracked |= change == IceChange.Cracked;
            melted |= change == IceChange.Melted;
        }
        // One sound per kind however many covers react, so a rescue never stacks clips.
        if (cracked) SoundManager.Ins.PlaySound(GameConstants.ICE_CRACK_SOUND, Random.Range(0.92f, 1.08f), 1f);
        if (melted) SoundManager.Ins.PlaySound(GameConstants.ICE_BREAK_SOUND, Random.Range(0.95f, 1.05f), 1f);
    }

    // A rescue chips a few flakes off every remaining ice; the last one shatters.
    private void PlayIceShards(Vector2Int cell, bool melted)
    {
        iceShards.Play(CellToLocalPosition(cell), melted ? MELT_SHARD_COUNT : CRACK_SHARD_COUNT,
            melted ? 1.8f : 1.1f, melted ? 0.2f : 0.13f);
    }

    private int CollectedTargetCount()
    {
        int collected = 0;
        foreach (TargetPoint target in targets.Values)
        {
            if (target.IsCollected) collected++;
        }
        return collected;
    }

    private void ShakeIce(Vector2Int cell)
    {
        iceCovers[cell].PlayDenied();
        SoundManager.Ins.PlaySound(GameConstants.ICE_BLOCKED_SOUND);
    }
}
