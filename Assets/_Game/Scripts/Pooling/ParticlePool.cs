using System.Collections.Generic;
using UnityEngine;

public static class ParticlePool
{
    private const int INITIAL_PLAY_POOL_SIZE = 10;
    private static Transform root;
    private static readonly Dictionary<ParticleType, ParticleSystem> SHORTCUTS = new Dictionary<ParticleType, ParticleSystem>();
    private static readonly Dictionary<int, ParticleEffectPool> POOLS = new Dictionary<int, ParticleEffectPool>();

    public static Transform Root
    {
        get
        {
            if (root == null) root = new GameObject("ParticlePool").transform;
            return root;
        }
    }

    public static void Preload(ParticleSystem prefab, int qty = 1, Transform parent = null)
    {
        if (prefab == null || POOLS.ContainsKey(prefab.GetInstanceID())) return;
        POOLS.Add(prefab.GetInstanceID(), new ParticleEffectPool(prefab, qty, parent));
    }

    public static void Play(ParticleSystem prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null)
        {
            GameDiagnostics.LogError("ParticlePool requires a particle prefab.");
            return;
        }
        int prefabId = prefab.GetInstanceID();
        if (!POOLS.ContainsKey(prefabId))
        {
            Transform poolRoot = new GameObject("VFX_" + prefab.name).transform;
            poolRoot.SetParent(Root, false);
            Preload(prefab, INITIAL_PLAY_POOL_SIZE, poolRoot);
        }
        POOLS[prefabId].Play(pos, rot);
    }

    public static void Play(ParticleType particleType, Vector3 pos, Quaternion rot)
    {
        if (!SHORTCUTS.ContainsKey(particleType))
        {
            GameDiagnostics.LogError("Particle type is not registered: " + particleType);
            return;
        }
        Play(SHORTCUTS[particleType], pos, rot);
    }

    public static void Release(ParticleSystem prefab)
    {
        if (prefab == null) return;
        int prefabId = prefab.GetInstanceID();
        if (!POOLS.ContainsKey(prefabId)) return;
        POOLS[prefabId].Release();
        POOLS.Remove(prefabId);
    }

    public static void Shortcut(ParticleType particleType, ParticleSystem particleSystem)
    {
        SHORTCUTS[particleType] = particleSystem;
    }
}
