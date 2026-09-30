using UnityEngine;

public static class VibrationSettings
{
    private const string EnabledKey = "VibrationEnabled";
    public static bool IsEnabled => PlayerPrefs.GetInt(EnabledKey, 1) == 1;

    public static void Toggle()
    {
        PlayerPrefs.SetInt(EnabledKey, IsEnabled ? 0 : 1);
        PlayerPrefs.Save();
        Vibrate();
    }

    public static void Vibrate()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (IsEnabled) Handheld.Vibrate();
#endif
    }
}
