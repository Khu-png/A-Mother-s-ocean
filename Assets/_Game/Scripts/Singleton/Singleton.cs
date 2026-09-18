using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T ins;

    public static T Ins
    {
        get
        {
            if (ins == null)
            {
                // Need to create a new GameObject to attach the singleton to.
                ins = new GameObject(nameof(T)).AddComponent<T>();
            }
            return ins;
        }
    }

    protected void RegisterSingleton(T instance)
    {
        if (ins == null) ins = instance;
    }
}
