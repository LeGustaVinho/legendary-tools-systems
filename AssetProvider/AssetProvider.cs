using System;
using System.Collections;
using UnityEngine;

namespace LegendaryTools.Systems.AssetProvider
{
    public abstract class AssetProvider : ScriptableObject
    {
        public abstract T Load<T>(object arg) where T : UnityEngine.Object;

        public abstract IEnumerator LoadAsync<T>(object arg, Action<T> onComplete) where T : UnityEngine.Object;

        public abstract void Unload<T>(ref T instance) where T : UnityEngine.Object;
    }
}