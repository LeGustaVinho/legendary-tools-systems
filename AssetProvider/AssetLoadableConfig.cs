using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LegendaryTools.Systems.AssetProvider
{
    public interface IAssetLoaderConfig<T> where T : Object
    {
        bool IsInScene { get; } //Flag used to identify that this asset does not need load/unload because it is serialized in the scene
        T LoadedAsset { get; }
        bool IsLoaded { get; }
        bool IsLoading { get; }
        IEnumerator Load();
        void Unload();
        void SetAsSceneAsset(T sceneInstanceInScene);
    }

    public class AssetLoadableConfig<T> : ScriptableObject
        where T: UnityEngine.Object
    {
        public AssetLoadable<T> AssetLoadable;
    }

    [Serializable]
    public class AssetLoadable<T> : IAssetLoaderConfig<T>
        where T: UnityEngine.Object
    {
        [Header("Loading")] public bool PreLoad;
        public bool DontUnloadAfterLoad;

        public AssetProvider LoadingStrategy;
        public bool UseAsyncLoading;
        public T HardReference;
        public string[] WeakReference;
        public bool IsInScene { private set; get; } //Flag used to identify that this asset does not need load/unload because it is serialized in the scene

        public T LoadedAsset => loadedAsset;

        public bool IsLoaded => loadedAsset != null;

        public bool IsLoading { private set; get; }

        private T loadedAsset;

        public IEnumerator Load()
        {
            if (IsInScene)
            {
                yield break;
            }
            
            if (HardReference != null)
            {
                loadedAsset = HardReference;
                yield break;
            }

            if (LoadingStrategy != null)
            {
                if (UseAsyncLoading)
                {
                    IsLoading = true;
                    yield return LoadingStrategy.LoadAsync<T>(WeakReference, OnLoadAssetAsync);
                }
                else
                {
                    loadedAsset = LoadingStrategy.Load<T>(WeakReference);
                }
            }
            else
            {
                Debug.LogError("[AssetLoaderConfig:Load] -> LoadingStrategy is null");
            }
        }
        
        public void Unload()
        {
            if (!IsInScene)
            {
                if (loadedAsset != null && LoadingStrategy != null)
                {
                    LoadingStrategy.Unload(ref loadedAsset);
                }
            }
        }
        
        public void SetAsSceneAsset(T sceneInstanceInScene)
        {
            IsInScene = sceneInstanceInScene != null;
            loadedAsset = sceneInstanceInScene;
        }

        public void ClearLoadedAssetRef()
        {
            loadedAsset = null;
        }
        
        private void OnLoadAssetAsync(T screenBase)
        {
            loadedAsset = screenBase;
            IsLoading = false;
        }
    }
}