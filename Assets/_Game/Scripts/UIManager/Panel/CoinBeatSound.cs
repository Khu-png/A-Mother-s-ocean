using UnityEngine;

// Pitch ladder for the coin count-up: every tier steps up a major pentatonic scale,
// so the more coins are paid out the higher the tone, and it always sounds in tune.
public static class CoinBeatSound
{
    private const float MAX_PITCH = 2.5f;
    private const int SEMITONES_PER_OCTAVE = 12;
    private static readonly int[] SCALE_SEMITONES = { 0, 2, 4, 7, 9 };

    public static float Pitch(int tierIndex)
    {
        int octave = tierIndex / SCALE_SEMITONES.Length;
        int semitone = SCALE_SEMITONES[tierIndex % SCALE_SEMITONES.Length]
            + SEMITONES_PER_OCTAVE * octave;
        return Mathf.Min(MAX_PITCH, Mathf.Pow(2f, semitone / (float)SEMITONES_PER_OCTAVE));
    }
}
