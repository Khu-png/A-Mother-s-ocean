using System;
using UnityEngine;

public enum LevelDifficulty
{
    Easy,
    Hard,
    SuperHard
}

[Serializable]
public sealed class LevelDifficultyEntry
{
    [SerializeField, Min(1)] private int level = 1;
    [SerializeField] private LevelDifficulty difficulty;

    public int Level => level;
    public LevelDifficulty Difficulty => difficulty;
}

[Serializable]
public sealed class LevelDifficultyRule
{
    [SerializeField, Min(1)] private int fromLevel = 1;
    [SerializeField, Min(1)] private int toLevel = 1000;
    [Tooltip("Last digits that match (0-9), e.g. 5 = levels 5, 15, 25. Empty = every level in the range.")]
    [SerializeField] private int[] lastDigits = new int[0];
    [SerializeField] private LevelDifficulty difficulty;

    public LevelDifficulty Difficulty => difficulty;

    public bool Matches(int level)
    {
        if (level < fromLevel || level > toLevel) return false;
        if (lastDigits.Length == 0) return true;
        return Array.IndexOf(lastDigits, level % 10) >= 0;
    }
}

[Serializable]
public sealed class LevelDifficultyTier
{
    [SerializeField] private LevelDifficulty difficulty;
    [SerializeField, Min(0)] private int coinReward = 20;

    public LevelDifficulty Difficulty => difficulty;
    public int CoinReward => coinReward;
}

// The one place that says how hard each level is. Priority: an explicit level entry,
// then the first matching rule, then the default. Edit this table, not each LevelData.
[CreateAssetMenu(fileName = "LevelDifficultyConfig", menuName = "A Mother's Ocean/Level Difficulty Config")]
public sealed class LevelDifficultyConfig : ScriptableObject
{
    [SerializeField] private LevelDifficulty defaultDifficulty = LevelDifficulty.Easy;
    [Tooltip("Pin one level to a difficulty. Wins over every rule.")]
    [SerializeField] private LevelDifficultyEntry[] levels = new LevelDifficultyEntry[0];
    [Tooltip("Patterns such as 'every level ending in 5'. The first matching rule wins.")]
    [SerializeField] private LevelDifficultyRule[] rules = new LevelDifficultyRule[0];
    [SerializeField] private LevelDifficultyTier[] tiers = new LevelDifficultyTier[0];

    public LevelDifficulty GetDifficulty(int levelNumber)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].Level == levelNumber) return levels[i].Difficulty;
        }
        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i].Matches(levelNumber)) return rules[i].Difficulty;
        }
        return defaultDifficulty;
    }

    public int GetCoinReward(int levelNumber)
    {
        LevelDifficulty difficulty = GetDifficulty(levelNumber);
        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].Difficulty == difficulty) return tiers[i].CoinReward;
        }
        throw new UnityException($"{name}: no tier is defined for {difficulty}.");
    }

    void OnValidate()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            for (int j = i + 1; j < levels.Length; j++)
            {
                if (levels[i].Level == levels[j].Level)
                    GameDiagnostics.LogWarning($"{name}: level {levels[i].Level} is listed twice; the first entry wins.", this);
            }
        }
    }
}
