using System.Collections;
using LegendaryTools.Systems.AssetProvider;
using UnityEngine;

namespace LegendaryTools.Systems.ScreenFlow
{
    public abstract class UIEntityBaseConfig : ScriptableObject, IAssetLoaderConfig
    {
        public AnimationType AnimationType = AnimationType.NoAnimation;
        public abstract bool PreLoad { get; set; }
        public abstract bool DontUnloadAfterLoad { get; set; }
        public abstract AssetProvider.AssetProvider LoadingStrategy { get; set; }
        public abstract object AssetReference { get; }
        public abstract bool IsInScene { get; }
        public abstract Object LoadedAsset { get; }
        public abstract bool IsLoaded { get; }
        public abstract bool IsLoading { get; }
        public abstract IEnumerator Load();

        public abstract void Unload();

        public abstract void SetAsSceneAsset(Object sceneInstanceInScene);
        public abstract void ClearLoadedAssetRef();
    }
}