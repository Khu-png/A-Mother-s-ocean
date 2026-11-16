using UnityEngine;

[CreateAssetMenu(fileName = "LevelBackgroundSet", menuName = "A Mother's Ocean/Level Background Set")]
public sealed class LevelBackgroundSet : ScriptableObject
{
    [SerializeField] private string[] resourcePaths;
    [SerializeField, Min(1)] private int levelsPerBackground = 10;

    // Backgrounds repeat from the first one once every entry has been used.
    public string PathForLevel(int levelNumber)
    {
        int block = (levelNumber - 1) / levelsPerBackground;
        return resourcePaths[block % resourcePaths.Length];
    }
}
