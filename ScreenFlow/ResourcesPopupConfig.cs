using System;
using System.Collections;
using LegendaryTools.Systems.AssetProvider;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LegendaryTools.Systems.ScreenFlow
{
    [Serializable]
    public class PopupBaseStringAssetLoadable : AssetLoadable<PopupBase, string>
    {
    }
    
    [CreateAssetMenu(menuName = "Tools/ScreenFlow/PopupConfig/ResourcesPopupConfig")]
    public class ResourcesPopupConfig : PopupConfig
    {
        [Header("Loader")]
        public PopupBaseStringAssetLoadable AssetLoadable;

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

        public override AssetProvider.AssetProvider LoadingStrategy
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