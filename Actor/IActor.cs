using System;
using UnityEngine;

namespace LegendaryTools.Systems.Actor
{
    public interface IActor : IMonoBehaviour, IRectTransform, IGameObject, IDisposable
    {
        Transform Transform { get; }
        RectTransform RectTransform { get; }
        GameObject GameObject { get; }
    }
}