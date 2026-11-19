using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "A Mother's Ocean/Level Data")]
public class LevelData : ScriptableObject
{
	[SerializeField] private string[] mapRows =
	{
		"F|T|A.",
		".SAO-.",
		"XOTOTX",
		".XOT-.",
		"..XO.."
	};
	[SerializeField] private bool grantsGiftChest;
	[SerializeField, Min(1)] private int timeLimitSeconds = 120;

	public string[] MapRows => mapRows;
	public bool GrantsGiftChest => grantsGiftChest;
	public int TimeLimitSeconds => timeLimitSeconds;

	public void SetMapRows(string[] rows)
	{
		mapRows = rows;
	}

	public void SetTimeLimit(int seconds)
	{
		timeLimitSeconds = Mathf.Max(1, seconds);
	}
}
