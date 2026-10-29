using UnityEditor;

public static class WinWavePreview
{
    [MenuItem("Tools/A Mother's Ocean/Preview Win Wave %#F9")]
    public static void Preview()
    {
        GameManager.Ins.OnFinish();
    }

    [MenuItem("Tools/A Mother's Ocean/Preview Win Wave %#F9", true)]
    private static bool CanPreview()
    {
        return EditorApplication.isPlaying && GameManager.IsState(GameState.Gameplay);
    }
}
