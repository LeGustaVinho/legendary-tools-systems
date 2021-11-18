using System;
using System.Collections;
using UnityEngine;

namespace LegendaryTools.Systems.ScreenFlow
{
    [CreateAssetMenu(menuName = "Tools/ScreenFlow/ResourcesAssetProvider")]
    public class ResourcesAssetProvider : AssetProvider
    {
        public override T Load<T>(string[] args)
        {
            if (args.Length > 0)
            {
                return Resources.Load<T>(args[0]);
            }

            return null;
        }

        public override IEnumerator LoadAsync<T>(string[] args, Action<T> onComplete)
        {
            if (args.Length > 0)
            {
                ResourceRequest resourcesRequest = Resources.LoadAsync<T>(args[0]);

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