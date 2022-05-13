using System;
using System.Collections;
using LegendaryTools.Systems.AssetProvider;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LegendaryTools.Bragi
{
    [Serializable]
    public class AssetLoadableAudioClipString : AssetLoadable<AudioClip, string>
    {
    }
    
    public class ResourcesAudioConfig : AudioConfig
    {
        [Header("Loader")]
        public AssetLoadableAudioClipString AssetLoadable;

        public override bool PreLoad
        {
            get => AssetLoadable.PreLoad;
            set => AssetLoadable.PreLoad = value;
        }
        public override bool DontUnloadAfterLoad 
        { 
            get => AssetLoadable.DontUnloadAfterLoad;
            set => AssetLoadable.DontUnloadAfterLoad = value;
        }

        public override AssetProvider LoadingStrategy
        {
            get => AssetLoadable.LoadingStrategy;
            set => AssetLoadable.LoadingStrategy = value;
        }

        public override object AssetReference => AssetLoadable.AssetReference;
        public override bool IsInScene => AssetLoadable.IsInScene;
        public override Object LoadedAsset => AssetLoadable.LoadedAsset;
        public override bool IsLoaded => AssetLoadable.IsLoaded;
        public override bool IsLoading => AssetLoadable.IsLoading;
        
        public override IEnumerator Load()
        {
            yield return AssetLoadable.Load();
        }

        public override void Unload()
        {
            AssetLoadable.Unload();
        }

        public override void SetAsSceneAsset(Object sceneInstanceInScene)
        {
            AssetLoadable.SetAsSceneAsset(sceneInstanceInScene);
        }

        public override void ClearLoadedAssetRef()
        {
            AssetLoadable.ClearLoadedAssetRef();
        }
    }
}