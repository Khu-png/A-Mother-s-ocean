using System;
using UnityEngine;

[Serializable]
public sealed class MenuTankFish
{
    [SerializeField] private RectTransform fish;
    [SerializeField] private Vector2 travel = new Vector2(40f, 12f);
    [SerializeField, Min(0.01f)] private float speed = 0.45f;
    [SerializeField] private float phase;
    private Vector2 origin;

    public void CaptureOrigin()
    {
        origin = fish.anchoredPosition;
    }

    public void Swim(float time)
    {
        float cycle = time * speed + phase;
        float x = Mathf.Sin(cycle) * travel.x;
        float y = Mathf.Sin(cycle * 0.61f) * travel.y;
        fish.anchoredPosition = origin + new Vector2(x, y);
    }
}

public sealed class MenuTankPage : MonoBehaviour
{
    [SerializeField] private MenuTankFish[] fish;

    void Awake()
    {
        for (int i = 0; i < fish.Length; i++) fish[i].CaptureOrigin();
    }

    void Update()
    {
        float time = Time.unscaledTime;
        for (int i = 0; i < fish.Length; i++) fish[i].Swim(time);
    }
}
