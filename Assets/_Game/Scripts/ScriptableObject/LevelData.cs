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

	public string[] MapRows => mapRows;

	public void SetMapRows(string[] rows)
	{
		mapRows = rows;
	}
}
