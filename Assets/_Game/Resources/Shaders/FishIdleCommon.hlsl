#ifndef FISH_IDLE_COMMON_INCLUDED
#define FISH_IDLE_COMMON_INCLUDED

// Shared fake-skeletal idle for side-view fish sprites facing +X (tail toward local x = 0).
// "local" is the 0-1 position inside the sprite. The including shader declares the
// material properties used below, so gameplay (URP sprite) and menu (UI) fish match.

float FishHash(float n)
{
    return frac(sin(n * 12.9898) * 43758.5453);
}

// Returns where to sample so the drawn pixel appears displaced (inverse mapping).
float2 FishBodySample(float2 local, float time)
{
    float swimTime = time * _SwimSpeed;
    float breath = sin(swimTime * 0.5) * _BreathAmount;
    local = _BreathCenter.xy + (local - _BreathCenter.xy) / (1.0 + breath);

    float tail = saturate((_TailStart - local.x) / _TailStart);
    tail *= tail;
    float wave = sin(swimTime - local.x * _WaveLength);
    float2 offset;
    offset.y = wave * (_TailAmplitude * tail + _BodyAmplitude);
    // The tail looks shorter at the ends of its sweep, as if turning in depth.
    offset.x = -abs(wave) * _TailSquash * tail;
    return local - offset;
}

// Where the pupils look: a new random direction every _LookInterval, eased in.
float2 FishLookDirection(float time)
{
    float slot = floor(time / _LookInterval);
    float blend = smoothstep(0.85, 1.0, frac(time / _LookInterval));
    float2 current = float2(FishHash(slot) * 2.0 - 1.0, (FishHash(slot + 7.1) * 2.0 - 1.0) * 0.5);
    float2 next = float2(FishHash(slot + 1.0) * 2.0 - 1.0, (FishHash(slot + 8.1) * 2.0 - 1.0) * 0.5);
    return lerp(current, next, blend) * _LookAmount;
}

// Shifts the iris inside one eye; the eye outline stays in place.
float2 FishApplyEye(float2 local, float2 center, float2 radius, float2 look)
{
    float eyeDistance = length((local - center) / radius);
    return local - look * radius * (1.0 - smoothstep(0.5, 0.95, eyeDistance));
}

// Full idle: body motion, then the looking eyes. Returns the sample position (local 0-1).
float2 FishIdleSample(float2 local, float phase)
{
    float time = _Time.y + phase;
    float2 sampleLocal = FishBodySample(local, time);
    float2 look = FishLookDirection(time);
    sampleLocal = FishApplyEye(sampleLocal, _EyeCenter.xy, _EyeRadius.xy, look);
    if (_Eye2Center.z > 0.5)
        sampleLocal = FishApplyEye(sampleLocal, _Eye2Center.xy, _Eye2Radius.xy, look);
    return sampleLocal;
}

#endif
