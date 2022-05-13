using System.Collections;
using LegendaryTools.Systems.AssetProvider;
using UnityEngine;

namespace LegendaryTools.Bragi
{
    public abstract class AudioConfigBase : ScriptableObject
    {
        public abstract AudioHandler[] Play(AudioSettings overrideSettings = null, bool allowFading = true);

        public abstract AudioHandler[] Play(Vector3 position, AudioSettings overrideSettings = null, bool allowFading = true);

        public abstract AudioHandler[] Play(Transform parent, AudioSettings overrideSettings = null, bool allowFading = true);
    }

    public abstract class AudioConfigBaseAssetLoader : AudioConfigBase, IAssetLoaderConfig
    {
        public abstract bool PreLoad { get; set; }
        public abstract bool DontUnloadAfterLoad { get; set; }
        public abstract AssetProvider LoadingStrategy { get; set; }
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