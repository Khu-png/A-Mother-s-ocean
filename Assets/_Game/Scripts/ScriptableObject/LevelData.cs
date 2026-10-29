using UnityEngine;

public enum LevelDifficulty
{
	Easy,
	Hard,
	SuperHard
}

[CreateAssetMenu(fileName = "Level_", menuName = "A Mother's Ocean/Level Data")]
public class LevelData : ScriptableObject
{
	private const int EASY_REWARD = 20;
	private const int HARD_REWARD = 50;
	private const int SUPER_HARD_REWARD = 100;

	[SerializeField] private string[] mapRows =
	{
		"F|T|A.",
		".SAO-.",
		"XOTOTX",
		".XOT-.",
		"..XO.."
	};
	[SerializeField] private LevelDifficulty difficulty;
	[SerializeField] private bool grantsGiftChest;

	public string[] MapRows => mapRows;
	public LevelDifficulty Difficulty => difficulty;
	public bool GrantsGiftChest => grantsGiftChest;
	public int CompletionCoinReward => difficulty == LevelDifficulty.Easy
		? EASY_REWARD
		: difficulty == LevelDifficulty.Hard ? HARD_REWARD : SUPER_HARD_REWARD;

	public void SetMapRows(string[] rows)
	{
		mapRows = rows;
	}

	public void SetDifficulty(LevelDifficulty value)
	{
		difficulty = value;
	}
}
