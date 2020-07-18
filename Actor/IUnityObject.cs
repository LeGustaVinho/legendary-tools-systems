using UnityEngine;

namespace LegendaryTools.Systems.Actor
{
    public interface IUnityObject
    {
        HideFlags HideFlags { get; set; }

        int GetInstanceID();
    }
}