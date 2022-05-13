using System;
using System.Collections;
using UnityEngine;

namespace LegendaryTools.Systems.AssetProvider
{
    [CreateAssetMenu(menuName = "Tools/ScreenFlow/ResourcesAssetProvider")]
    public class ResourcesAssetProvider : AssetProvider
    {
        public override T Load<T>(object arg)
        {
            string path = (string)arg;
            if (path.Length > 0)
            {
                return Resources.Load<T>(path);
            }

            return null;
        }

        public override IEnumerator LoadAsync<T>(object arg, Action<T> onComplete)
        {
            string path = (string)arg;
            if (path.Length > 0)
            {
                ResourceRequest resourcesRequest = Resources.LoadAsync<T>(path);

                while (!resourcesRequest.isDone)
                {
                    yield return null;
                }

                onComplete.Invoke(resourcesRequest.asset as T);
            }
        }

        public override void Unload<T>(ref T instance)
        {
            instance = null;
            Resources.UnloadUnusedAssets();
        }
    }
}