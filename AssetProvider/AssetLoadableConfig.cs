using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LegendaryTools.Systems.AssetProvider
{
    public interface IAssetLoaderConfig
    {
        public bool PreLoad { get; set; }
        public bool DontUnloadAfterLoad { get; set; }

        public AssetProvider LoadingStrategy { get; set; }
        public object AssetReference { get; }
        
        bool IsInScene { get; } //Flag used to identify that this asset does not need load/unload because it is serialized in the scene
        UnityEngine.Object LoadedAsset { get; }
        bool IsLoaded { get; }
        bool IsLoading { get; }
        IEnumerator Load();
        void Unload();
        void SetAsSceneAsset(UnityEngine.Object sceneInstanceInScene);
        public void ClearLoadedAssetRef();
    }
    
    public interface IAssetLoaderConfig<T, W> : IAssetLoaderConfig
        where T : UnityEngine.Object
    {
        new W AssetReference { get; }
        new T LoadedAsset { get; }
        void SetAsSceneAsset(T sceneInstanceInScene);
    }

    [Serializable]
    public class AssetLoadable<T, W> : IAssetLoaderConfig<T, W>
        where T: UnityEngine.Object
    {
        [SerializeField] private bool _preload;
        [SerializeField] private bool _dontUnloadAfterLoad;
        [SerializeField] private AssetProvider _loadingStrategy;
        [SerializeField] private W _assetReference;
        
        public bool PreLoad
        {
            get => _preload;
            set => _preload = value;
        }
        public bool DontUnloadAfterLoad 
        {
            get => _dontUnloadAfterLoad;
            set => _dontUnloadAfterLoad = value;
        }

        public AssetProvider LoadingStrategy
        {
            get => _loadingStrategy;
            set => _loadingStrategy = value;
        }
        
        object IAssetLoaderConfig.AssetReference => AssetReference;
        Object IAssetLoaderConfig.LoadedAsset => LoadedAsset;
        
        public W AssetReference => _assetReference;

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

            if (LoadingStrategy != null)
            {
                IsLoading = true;
                yield return LoadingStrategy.LoadAsync<T>(AssetReference, OnLoadAssetAsync);
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

        public void SetAsSceneAsset(Object sceneInstanceInScene)
        {
            SetAsSceneAsset(sceneInstanceInScene as T);
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