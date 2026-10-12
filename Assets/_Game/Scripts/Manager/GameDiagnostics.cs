using System;
using System.Diagnostics;
using UnityEngine;

public static class GameDiagnostics
{
    [Conditional("UNITY_EDITOR")]
    public static void Log(object message, UnityEngine.Object context = null)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(message, context);
#endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(object message, UnityEngine.Object context = null)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.LogWarning(message, context);
#endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void LogError(object message, UnityEngine.Object context = null)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.LogError(message, context);
#endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void LogException(Exception exception, UnityEngine.Object context = null)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.LogException(exception, context);
#endif
    }
}
