using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T ins;

    public static T Ins => ins;

    protected void RegisterSingleton(T instance)
    {
        if (ins != null && ins != instance)
        {
            throw new InvalidOperationException($"Only one {typeof(T).Name} may be registered.");
        }

        ins = instance;
    }
}
