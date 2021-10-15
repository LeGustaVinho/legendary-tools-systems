using System.Collections;
using UnityEngine;

namespace LegendaryTools.Systems.ScreenFlow
{
    public class UIEntityBaseConfig : ScriptableObject
    {
        public AnimationType AnimationType = AnimationType.NoAnimation;

        [Header("Loading")] public bool PreLoad;
        public bool DontUnloadAfterLoad;

        public AssetProvider LoadingStrategy;
        public bool UseAsyncLoading;
        public ScreenBase HardReference;
        public string[] WeakReference;
        
        public bool IsInScene { private set; get; } //Flag used to identify that this screen does not need load/unload because it is serialized in the scene

        public Transform HardReferenceTransform;
        
        public ScreenBase LoadedScreen => loadedScreen;

        public bool IsLoaded => loadedScreen != null;

        public bool IsLoading { private set; get; }

        private ScreenBase loadedScreen;

        public IEnumerator Load()
        {
            if (IsInScene)
            {
                yield break;
            }
            
            if (HardReference != null)
            {
                loadedScreen = HardReference;
                yield break;
            }

            if (LoadingStrategy != null)
            {
                if (UseAsyncLoading)
                {
                    IsLoading = true;
                    yield return LoadingStrategy.LoadAsync<ScreenBase>(WeakReference, OnLoadAssetAsync);
                }
                else
                {
                    loadedScreen = LoadingStrategy.Load<ScreenBase>(WeakReference);
                }
            }
            else
            {
                Debug.LogError("[UIEntityBaseConfig:Load] -> LoadingStrategy is null");
            }
        }

        private void OnLoadAssetAsync(ScreenBase screenBase)
        {
            loadedScreen = screenBase;
            IsLoading = false;
        }

        public void Unload()
        {
            if (!IsInScene)
            {
                if (loadedScreen != null && LoadingStrategy != null)
                {
                    LoadingStrategy.Unload(ref loadedScreen);
                }
            }
        }

        public void SetAsSceneAsset(ScreenBase sceneInstanceInScene)
        {
            IsInScene = sceneInstanceInScene != null;
            loadedScreen = sceneInstanceInScene;
        }
    }
}