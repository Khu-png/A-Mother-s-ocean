using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUnit : MonoBehaviour
{
    private Transform tf;
    private Coroutine despawnRoutine;
    public PoolType poolType;

    void OnDisable()
    {
        if (despawnRoutine != null) StopCoroutine(despawnRoutine);
        despawnRoutine = null;
    }
    public Transform TF
    {
        get
        {
            //tf = tf ?? gameObject.transform;
            if (tf == null)
            {
                tf = transform;
            }
            return tf;
        }
    }

    public void OnDespawn(float delay)
    {
        if (despawnRoutine != null) StopCoroutine(despawnRoutine);
        despawnRoutine = StartCoroutine(DespawnAfterDelay(delay));
    }

    private IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delay));
        despawnRoutine = null;
        HBPool.Despawn(this);
    }

}
