using System;
using UnityEngine;

[Serializable]
public sealed class MenuPageBinding
{
    [SerializeField] private MenuTabId tab;
    [SerializeField] private RectTransform page;

    public MenuTabId Tab => tab;
    public RectTransform Page => page;
}
