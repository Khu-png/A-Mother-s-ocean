using System.Collections.Generic;
using UnityEngine;

public sealed class ParticleEffectPool
{
    private readonly Transform ROOT;
    private readonly ParticleSystem PREFAB;
    private readonly List<ParticleSystem> PARTICLES;
    private int nextIndex;

    public ParticleEffectPool(ParticleSystem prefab, int initialQuantity, Transform parent)
    {
        ROOT = parent;
        PREFAB = prefab;
        int quantity = Mathf.Max(1, initialQuantity);
        PARTICLES = new List<ParticleSystem>(quantity);
        // Bounded prewarming moves initial allocations out of repeated playback.
        for (int index = 0; index < quantity; index++) PARTICLES.Add(CreateParticle());
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        if (PARTICLES.Count == 0) return;
        nextIndex = (nextIndex + 1) % PARTICLES.Count;
        ParticleSystem particle = PARTICLES[nextIndex];
        if (particle == null || particle.IsAlive(true))
        {
            particle = CreateParticle();
            PARTICLES.Insert(nextIndex, particle);
        }
        particle.transform.SetPositionAndRotation(position, rotation);
        particle.Play();
    }

    public void Release()
    {
        for (int index = PARTICLES.Count - 1; index >= 0; index--)
        {
            ParticleSystem particle = PARTICLES[index];
            if (particle == null) continue;
            particle.gameObject.SetActive(false);
            if (Application.isPlaying) Object.Destroy(particle.gameObject);
            else Object.DestroyImmediate(particle.gameObject);
        }
        PARTICLES.Clear();
    }

    private ParticleSystem CreateParticle()
    {
        ParticleSystem particle = Object.Instantiate(PREFAB, ROOT);
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule settings = particle.main;
        settings.loop = false;
        settings.playOnAwake = false;
        settings.stopAction = ParticleSystemStopAction.None;
        return particle;
    }
}
