using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LegendaryTools.Systems.ScreenFlow
{
    public class EntityArgPair<T>
    {
        public T Entity;
        public System.Object Args;

        public EntityArgPair(T uiEntity, object args)
        {
            Entity = uiEntity;
            Args = args;
        }
    }

    public enum ScreenFlowCommandType
    {
        Trigger,
        MoveBack,
        ClosePopup
    }
    
    public class ScreenFlowCommand
    {
        public ScreenFlowCommandType Type;
        public UnityEngine.Object Object;
        public System.Object Args;

        public ScreenFlowCommand(ScreenFlowCommandType type, Object o, object args)
        {
            Type = type;
            Object = o;
            Args = args;
        }
    }

    [Serializable]
    public class ScreenInScene
    {
        public ScreenConfig Config;
        public ScreenBase ScreenInstance;
    }
    
    [Serializable]
    public class PopupInScene
    {
        public PopupConfig Config;
        public PopupBase PopupInstance;
    }
    
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class ScreenFlow : SingletonBehaviour<ScreenFlow>
    {
        public ScreenFlowConfig Config;
        public ScreenConfig StartScreen;

        [Header("UI Entities In Scene")] 
        public List<ScreenInScene> ScreensInScene = new List<ScreenInScene>();
        public List<PopupInScene> PopupsInScene = new List<PopupInScene>();

        public bool IsTransiting => screenTransitionRoutine != null || 
                                    popupTransitionRoutine != null || 
                                    transitionRoutine != null;

        public bool IsPreloading => preloadRoutine != null;

        public ScreenConfig CurrentScreenConfig =>
            screensHistory.Count > 0 ? screensHistory[screensHistory.Count - 1].Entity : null;

        public ScreenBase CurrentScreenInstance { private set; get; }

        public PopupConfig CurrentPopupConfig =>
            popupConfigsStack.Count > 0 ? popupConfigsStack[popupConfigsStack.Count - 1] : null;

        public PopupBase CurrentPopupInstance =>
            popupInstancesStack.Count > 0 ? popupInstancesStack[popupInstancesStack.Count - 1] : null;

        public int PopupStackCount => popupInstancesStack.Count;

        protected readonly List<UIEntityBaseConfig> preloadQueue = new List<UIEntityBaseConfig>();
        protected readonly List<EntityArgPair<ScreenConfig>> screensHistory = new List<EntityArgPair<ScreenConfig>>();
        protected readonly List<PopupConfig> popupConfigsStack = new List<PopupConfig>();
        protected readonly List<PopupBase> popupInstancesStack = new List<PopupBase>();
        protected readonly Dictionary<PopupBase, Canvas> allocatedPopupCanvas = new Dictionary<PopupBase, Canvas>();
        protected readonly List<Canvas> availablePopupCanvas = new List<Canvas>();
        
        private readonly List<ScreenFlowCommand> commandQueue = new List<ScreenFlowCommand>();
        private readonly Dictionary<string, UIEntityBaseConfig> uiEntitiesLookup =
            new Dictionary<string, UIEntityBaseConfig>();

        private Coroutine preloadRoutine;
        private Coroutine screenTransitionRoutine;
        private Coroutine popupTransitionRoutine;
        private Coroutine nextScreenLoading;
        private Coroutine newPopupLoading;
        private Coroutine hideScreenRoutine;
        private Coroutine showScreenRoutine;
        private Coroutine hidePopupRoutine;
        private Coroutine showPopupRoutine;
        private Coroutine transitionRoutine;

        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasScaler canvasScaler;
        private GraphicRaycaster graphicRaycaster;

        public void SendTrigger(string name, System.Object args = null, bool enqueue = false)
        {
            if (uiEntitiesLookup.TryGetValue(name, out UIEntityBaseConfig uiEntityBaseConfig))
            {
                SendTrigger(uiEntityBaseConfig, args, enqueue);
            }
        }

        public void SendTrigger(UIEntityBaseConfig uiEntity, System.Object args = null, bool enqueue = false)
        {
            if (!IsTransiting)
            {
                commandQueue.Add(new ScreenFlowCommand(ScreenFlowCommandType.Trigger, uiEntity, args));
                transitionRoutine = StartCoroutine(ProcessCommandQueue());
            }
            else
            {
                if (enqueue)
                {
                    commandQueue.Add(new ScreenFlowCommand(ScreenFlowCommandType.Trigger, uiEntity, args));
                }
            }
        }

        public void MoveBack(System.Object args = null, bool enqueue = false)
        {
            if (!IsTransiting)
            {
                commandQueue.Add(new ScreenFlowCommand(ScreenFlowCommandType.MoveBack, null, args));
                transitionRoutine = StartCoroutine(ProcessCommandQueue());
            }
            else
            {
                if (enqueue)
                {
                    commandQueue.Add(new ScreenFlowCommand(ScreenFlowCommandType.MoveBack, null, args));
                }
            }
        }

        public void CloseForegroundPopup(System.Object args = null, bool enqueue = false)
        {
            if (CurrentPopupInstance != null)
            {
                ClosePopup(CurrentPopupInstance, args, enqueue);
            }
        }

        public void ClosePopup(PopupBase popupBase, System.Object args = null, bool enqueue = false)
        {
            if (!IsTransiting)
            {
                commandQueue.Add(new ScreenFlowCommand(ScreenFlowCommandType.ClosePopup, popupBase, args));
                transitionRoutine = StartCoroutine(ProcessCommandQueue());
            }
            else
            {
                if (enqueue)
                {
                    commandQueue.Add(new ScreenFlowCommand(ScreenFlowCommandType.ClosePopup, popupBase, args));
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        protected override void Start()
        {
            base.Start();

            if (Config == null)
            {
                Debug.LogError("[ScreenFlow:Start] -> Config is null");
                return;
            }

            Initialize();
            Preload();

            if (StartScreen != null)
            {
                SendTrigger(StartScreen);
            }
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ProcessBackKey();
            }
        }

        private void Initialize()
        {
            uiEntitiesLookup.Clear();
            foreach (ScreenConfig screenConfig in Config.Screens)
            {
                if (!uiEntitiesLookup.ContainsKey(screenConfig.name))
                {
                    uiEntitiesLookup.Add(screenConfig.name, screenConfig);    
                }
                else
                {
                    Debug.LogError("[ScreenFlow:Start()] -> UI Entity " + screenConfig.name + " already exists in ScreenFlow");
                }
            }

            foreach (PopupConfig popupConfig in Config.Popups)
            {
                if (!uiEntitiesLookup.ContainsKey(popupConfig.name))
                {
                    uiEntitiesLookup.Add(popupConfig.name, popupConfig);
                }
                else
                {
                    Debug.LogError("[ScreenFlow:Start()] -> UI Entity " + popupConfig.name + " already exists in ScreenFlow");
                }
            }
            
            foreach (ScreenInScene screenInScene in ScreensInScene)
            {
                if (!uiEntitiesLookup.ContainsKey(screenInScene.Config.name))
                {
                    uiEntitiesLookup.Add(screenInScene.Config.name, screenInScene.Config);
                    screenInScene.Config.SetAsSceneAsset(screenInScene.ScreenInstance);
                }
                else
                {
                    Debug.LogError("[ScreenFlow:Start()] -> UI Entity " + screenInScene.Config.name + " already exists in ScreenFlow");
                }
            }

            foreach (PopupInScene popupInScene in PopupsInScene)
            {
                if (!uiEntitiesLookup.ContainsKey(popupInScene.Config.name))
                {
                    uiEntitiesLookup.Add(popupInScene.Config.name, popupInScene.Config);
                    popupInScene.Config.SetAsSceneAsset(popupInScene.PopupInstance);
                }
                else
                {
                    Debug.LogError("[ScreenFlow:Start()] -> UI Entity " + popupInScene.Config.name + " already exists in ScreenFlow");
                }
            }
        }

        private IEnumerator ProcessCommandQueue()
        {
            while (commandQueue.Count > 0)
            {
                ScreenFlowCommand next = commandQueue[0];
                commandQueue.RemoveAt(0);

                switch (next.Type)
                {
                    case ScreenFlowCommandType.Trigger:
                    {
                        if (next.Object is ScreenConfig screenConfig)
                        {
                            yield return ScreenTransitTo(screenConfig, false, next.Args);
                        }
                        else if (next.Object is PopupConfig popupConfig)
                        {
                            if (CurrentScreenConfig != null)
                            {
                                if (CurrentScreenConfig.AllowPopups)
                                {
                                    yield return PopupTransitTo(popupConfig, next.Args);
                                }
                            }
                        }
                        break;
                    }
                    case ScreenFlowCommandType.MoveBack:
                    {
                        yield return MoveBackOp(next.Args);
                        break;
                    }
                    case ScreenFlowCommandType.ClosePopup:
                    {
                        yield return  ClosePopupOp(next.Object as PopupBase, next.Args);
                        break;
                    }
                }
            }

            transitionRoutine = null;
        }

        private IEnumerator MoveBackOp(System.Object args)
        {
            EntityArgPair<ScreenConfig> previousScreenConfig = screensHistory.Count > 1 ? screensHistory[screensHistory.Count - 2] : null;
            if (previousScreenConfig != null)
            {
                if (CurrentScreenConfig.CanMoveBackFromHere && previousScreenConfig.Entity.CanMoveBackToHere)
                {
                    yield return ScreenTransitTo(previousScreenConfig.Entity, 
                        true, args ?? previousScreenConfig.Args);
                }
            }
        }
        
        private IEnumerator ClosePopupOp(PopupBase popupBase, System.Object args)
        {
            int stackIndex = popupInstancesStack.FindIndex(item => item == popupBase);

            if (stackIndex >= 0)
            {
                bool isTopOfStack = stackIndex == popupInstancesStack.Count - 1;
                PopupConfig popupConfig = popupConfigsStack[stackIndex];

                PopupConfig behindPopupConfig = null;
                PopupBase behindPopupInstance = null;

                if (stackIndex - 1 >= 0)
                {
                    behindPopupConfig = popupConfigsStack[stackIndex - 1];
                    behindPopupInstance = popupInstancesStack[stackIndex - 1];
                }

                if (isTopOfStack)
                {
                    switch (popupConfig.AnimationType)
                    {
                        case AnimationType.NoAnimation:
                        case AnimationType.Wait:
                        {
                            //Wait for hide's animation to complete
                            yield return popupBase.Hide(args);
                            DisposePopupFromHide(popupConfig, popupBase);
                            break;
                        }
                        case AnimationType.Intersection:
                        {
                            hidePopupRoutine = StartCoroutine(popupBase.Hide(args));
                            break;
                        }
                    }

                    if (behindPopupInstance != null && behindPopupConfig.HideWhenGoingToBackground)
                    {
                        switch (popupConfig.AnimationType)
                        {
                            case AnimationType.NoAnimation:
                            case AnimationType.Wait:
                            {
                                //Wait for shows's animation to complete
                                yield return behindPopupInstance.Show(args);
                                break;
                            }
                            case AnimationType.Intersection:
                            {
                                //Show animation starts playing (may be playing in parallel with hide's animation)
                                showPopupRoutine = StartCoroutine(behindPopupInstance.Show(args));
                                break;
                            }
                        }
                    }

                    if (hidePopupRoutine != null) //If we were waiting for hide's animation
                    {
                        yield return hidePopupRoutine; //Wait for hide's animation to complete
                        DisposePopupFromHide(CurrentPopupConfig, CurrentPopupInstance);
                        hidePopupRoutine = null;
                    }

                    if (showPopupRoutine != null) //If we were waiting for show's animation
                    {
                        yield return showPopupRoutine; //Wait for show's animation to complete
                        showPopupRoutine = null;
                    }
                }
                else
                {
                    DisposePopupFromHide(popupConfig, popupBase);
                }
            }

            popupTransitionRoutine = null;
        }

        private void Preload()
        {
            preloadQueue.Clear();
            preloadQueue.AddRange(Array.FindAll(Config.Screens, item => item.PreLoad));
            preloadQueue.AddRange(Array.FindAll(Config.Popups, item => item.PreLoad));

            preloadRoutine = StartCoroutine(PreloadingAssets());
        }

        private IEnumerator PreloadingAssets()
        {
            foreach (var uiEntityBaseConfig in preloadQueue)
            {
                yield return uiEntityBaseConfig.Load();
            }

            preloadRoutine = null;
        }

        private IEnumerator ScreenTransitTo(ScreenConfig screenConfig, bool isMoveBack = false,
            System.Object args = null)
        {
            if (!screenConfig.IsLoaded)
            {
                if (!screenConfig.IsLoading) //Prevents loading, because the asset is being loaded in the preload routine 
                {
                    if (preloadQueue.Contains(screenConfig)) //Check if it is in the preloading queue 
                    {
                        preloadQueue.Remove(screenConfig);
                    }

                    //Schedule the new screen to load in the background
                    nextScreenLoading = StartCoroutine(screenConfig.Load());
                }
            }

            yield return HandlePopupsOnScreenTransit(args);

            if (CurrentScreenConfig != null)
            {
                switch (CurrentScreenConfig.AnimationType)
                {
                    case AnimationType.NoAnimation:
                    case AnimationType.Wait:
                    {
                        //Wait for hide's animation to complete
                        yield return CurrentScreenInstance.Hide(args);

                        if (CurrentScreenConfig.IsInScene)
                        {
                            //Screen is serialized on scene, no need to destroy or unload, so just set it disabled
                            CurrentScreenInstance.gameObject.SetActive(false);
                        }
                        else
                        {
                            Destroy(CurrentScreenInstance.gameObject);

                            if (!CurrentScreenConfig.DontUnloadAfterLoad)
                            {
                                CurrentScreenConfig.Unload();
                            }
                        }

                        break;
                    }
                    case AnimationType.Intersection:
                    {
                        //Hide animation starts playing 
                        hideScreenRoutine = StartCoroutine(CurrentScreenInstance.Hide(args));
                        break;
                    }
                }
            }

            if (nextScreenLoading != null)
            {
                yield return nextScreenLoading; //Wait for the new screen to load completely if it hasn't loaded yet
                nextScreenLoading = null;
            }
            else if (!screenConfig.IsLoaded) //The asset is probably still being loaded in the preload routine.
            {
                yield return new WaitUntil(() => screenConfig.IsLoaded); //Waits for the asset to be fully loaded into the preload routine 
            }

            ScreenBase newScreenInstance;
            if (screenConfig.IsInScene)
            {
                newScreenInstance = screenConfig.LoadedScreen;
                newScreenInstance.gameObject.SetActive(true);
            }
            else
            {
                newScreenInstance = InstantiateUIElement<ScreenBase>(screenConfig.LoadedScreen,
                    rectTransform, out RectTransform instanceRT, out RectTransform prefabRT);
            }

            switch (screenConfig.AnimationType)
            {
                case AnimationType.NoAnimation:
                case AnimationType.Wait:
                {
                    //Wait for shows's animation to complete
                    yield return newScreenInstance.Show(args);
                    break;
                }
                case AnimationType.Intersection:
                {
                    //Show animation starts playing (may be playing in parallel with hide's animation)
                    showScreenRoutine = StartCoroutine(newScreenInstance.Show(args));
                    break;
                }
            }

            if (hideScreenRoutine != null) //If we were waiting for hide's animation
            {
                yield return hideScreenRoutine; //Wait for hide's animation to complete
                if (CurrentScreenConfig.IsInScene)
                {
                    //Screen is serialized on scene, no need to destroy or unload, so just set it disabled
                    CurrentScreenInstance.gameObject.SetActive(false); 
                }
                else
                {
                    Destroy(CurrentScreenInstance.gameObject);

                    if (!CurrentScreenConfig.DontUnloadAfterLoad)
                    {
                        CurrentScreenConfig.Unload();
                    }
                }
                hideScreenRoutine = null;
            }

            if (showScreenRoutine != null) //If we were waiting for show's animation
            {
                yield return showScreenRoutine; //Wait for show's animation to complete
                showScreenRoutine = null;
            }

            //Update to new state
            CurrentScreenInstance = newScreenInstance;

            if (isMoveBack)
            {
                screensHistory.RemoveAt(screensHistory.Count - 1);
            }
            else
            {
                screensHistory.Add(new EntityArgPair<ScreenConfig>(screenConfig, args));
            }

            foreach (PopupBase popup in popupInstancesStack)
            {
                popup.ParentScreen = CurrentScreenInstance;
            }

            screenTransitionRoutine = null;
        }

        private IEnumerator PopupTransitTo(PopupConfig popupConfig, System.Object args = null)
        {
            if (!popupConfig.IsLoaded)
            {
                newPopupLoading = StartCoroutine(popupConfig.Load());
            }

            if (CurrentPopupConfig != null)
            {
                switch (CurrentPopupConfig.AnimationType)
                {
                    case AnimationType.NoAnimation:
                    case AnimationType.Wait:
                    {
                        if (CurrentScreenConfig.AllowStackablePopups && !CurrentPopupConfig.HideWhenGoingToBackground)
                        {
                            CurrentPopupInstance.OnGoToBackground(args);
                        }

                        if (!CurrentScreenConfig.AllowStackablePopups || CurrentPopupConfig.HideWhenGoingToBackground)
                        {
                            //Wait for hide's animation to complete
                            yield return CurrentPopupInstance.Hide(args);
                            DisposePopupFromHide(CurrentPopupConfig, CurrentPopupInstance);
                        }

                        break;
                    }
                    case AnimationType.Intersection:
                    {
                        if (!CurrentScreenConfig.AllowStackablePopups || CurrentPopupConfig.HideWhenGoingToBackground)
                        {
                            //Hide animation starts playing 
                            hidePopupRoutine = StartCoroutine(CurrentPopupInstance.Hide(args));
                        }

                        break;
                    }
                }
            }

            yield return newPopupLoading; //Wait for the new popup to load completely if it hasn't loaded yet
            newPopupLoading = null;
            
            PopupBase newPopup = null;
            Canvas canvasPopup = null;
            if (popupConfig.IsInScene)
            {
                newPopup = popupConfig.LoadedScreen as PopupBase;
                canvasPopup = GetComponentInParent<Canvas>();
                if (canvasPopup == null)
                {
                    canvasPopup = GetComponentInChildren<Canvas>();
                }
                newPopup.gameObject.SetActive(true);
            }
            else
            {
                canvasPopup = GetCanvas(popupConfig.LoadedScreen); //Check if the prefab popup has any canvas, if it does, we don't need to instantiate a new canvas
                
                if (canvasPopup != null)
                {
                    //Instantiate the popup from the prefab (and it already has canvas =D) 
                    newPopup = InstantiateUIElement<ScreenBase>(popupConfig.LoadedScreen, null,
                        out RectTransform instanceRT, out RectTransform prefabRT) as PopupBase;
                }
                else
                {
                    //Instantiate the popup from the prefab
                    newPopup = InstantiateUIElement<ScreenBase>(popupConfig.LoadedScreen, null,
                        out RectTransform instanceRT, out RectTransform prefabRT) as PopupBase;

                    //Instantiate new canvas to hold popup
                    canvasPopup = AllocatePopupCanvas(newPopup);

                    //Parent popup to canvas
                    ReparentUIElement(instanceRT, prefabRT, canvasPopup.transform);
                }
            }

            newPopup.ParentScreen = CurrentScreenInstance;

            //Change the order of the canvas, so that it is always above screen canvas
            CalculatePopupCanvasSortOrder(canvasPopup, CurrentPopupInstance);

            switch (popupConfig.AnimationType)
            {
                case AnimationType.NoAnimation:
                case AnimationType.Wait:
                {
                    //Wait for shows's animation to complete
                    yield return newPopup.Show(args);
                    break;
                }
                case AnimationType.Intersection:
                {
                    //Show animation starts playing (may be playing in parallel with hide's animation)
                    showPopupRoutine = StartCoroutine(newPopup.Show(args));
                    break;
                }
            }

            if (hidePopupRoutine != null) //If we were waiting for hide's animation
            {
                yield return hidePopupRoutine; //Wait for hide's animation to complete
                DisposePopupFromHide(CurrentPopupConfig, CurrentPopupInstance);
                hidePopupRoutine = null;
            }

            if (showPopupRoutine != null) //If we were waiting for show's animation
            {
                yield return showPopupRoutine; //Wait for show's animation to complete
                showPopupRoutine = null;
            }
            
            newPopup.OnClosePopupRequest += OnClosePopupRequest;

            //Update to new state
            popupConfigsStack.Add(popupConfig);
            popupInstancesStack.Add(newPopup);

            popupTransitionRoutine = null;
        }

        private void OnClosePopupRequest(PopupBase popupToClose)
        {
            ClosePopup(popupToClose);
        }

        private IEnumerator HandlePopupsOnScreenTransit(System.Object args = null)
        {
            if (CurrentPopupInstance != null)
            {
                switch (CurrentScreenConfig.PopupBehaviour)
                {
                    case PopupsBehaviour.PreserveAllOnHide:
                    {
                        //Just keep going
                        break;
                    }
                    case PopupsBehaviour.HideFirstThenTransit:
                    {
                        yield return ClosePopupOp(CurrentPopupInstance, args);
                        for (int i = popupConfigsStack.Count - 1; i >= 0; i--)
                        {
                            DisposePopupFromHide(popupConfigsStack[i], popupInstancesStack[i]);
                        }

                        break;
                    }
                    case PopupsBehaviour.DestroyAllThenTransit:
                    {
                        for (int i = popupConfigsStack.Count - 1; i >= 0; i--)
                        {
                            DisposePopupFromHide(popupConfigsStack[i], popupInstancesStack[i]);
                        }

                        break;
                    }
                }
            }
        }

        private void DisposePopupFromHide(PopupConfig popupConfig, PopupBase popupInstance)
        {
            //Remove current popup from stack
            popupConfigsStack.Remove(popupConfig);
            popupInstancesStack.Remove(popupInstance);
            RecyclePopupCanvas(popupInstance);

            popupInstance.OnClosePopupRequest -= OnClosePopupRequest;

            if (popupConfig.IsInScene)
            {
                popupInstance.gameObject.SetActive(false);
            }
            else
            {
                Destroy(popupInstance.gameObject);

                if (!popupConfig.DontUnloadAfterLoad)
                {
                    popupConfig.Unload();
                }
            }
        }

        private T InstantiateUIElement<T>(T screenPrefab, Transform parent, out RectTransform elementInstanceRT,
            out RectTransform elementPrefabRT)
            where T : ScreenBase
        {
            T elementInstance = Instantiate(screenPrefab);
            elementInstanceRT = elementInstance.GetComponent<RectTransform>();
            elementPrefabRT = screenPrefab.GetComponent<RectTransform>();

            ReparentUIElement(elementInstanceRT, elementPrefabRT, parent);

            return elementInstance;
        }

        private void ReparentUIElement(RectTransform elementInstanceRT, RectTransform elementPrefabRT, Transform parent)
        {
            elementInstanceRT.SetParent(parent);
            elementInstanceRT.SetAsLastSibling();
            elementInstanceRT.localPosition = elementPrefabRT.position;
            elementInstanceRT.localRotation = elementPrefabRT.rotation;
            elementInstanceRT.localScale = elementPrefabRT.localScale;

            elementInstanceRT.anchoredPosition3D = elementPrefabRT.anchoredPosition3D;
            elementInstanceRT.anchorMin = elementPrefabRT.anchorMin;
            elementInstanceRT.anchorMax = elementPrefabRT.anchorMax;
            elementInstanceRT.sizeDelta = elementPrefabRT.sizeDelta;
            elementInstanceRT.offsetMin = elementPrefabRT.offsetMin;
            elementInstanceRT.offsetMax = elementPrefabRT.offsetMax;
        }

        private Canvas GetCanvas(ScreenBase popupBase)
        {
            Canvas canvasPopup = popupBase.GetComponentInParent<Canvas>();
            return canvasPopup;
        }

        private Canvas AllocatePopupCanvas(PopupBase popupInstance)
        {
            Canvas availableCanvas = null;
            if (availablePopupCanvas.Count > 0)
            {
                availableCanvas = availablePopupCanvas[availablePopupCanvas.Count - 1];
                availablePopupCanvas.RemoveAt(availablePopupCanvas.Count - 1);
            }
            else
            {
                availableCanvas = CreatePopupCanvas();
            }

            allocatedPopupCanvas.Add(popupInstance, availableCanvas);
            availableCanvas.gameObject.SetActive(true);
            return availableCanvas;
        }

        private void RecyclePopupCanvas(PopupBase popupInstance)
        {
            if (allocatedPopupCanvas.TryGetValue(popupInstance, out Canvas popupCanvas))
            {
                allocatedPopupCanvas.Remove(popupInstance);
                availablePopupCanvas.Add(popupCanvas);
                popupCanvas.gameObject.SetActive(false);
            }
        }

        private Canvas CreatePopupCanvas()
        {
            if (Config.OverridePopupCanvasPrefab != null)
            {
                Canvas canvasPopupOverride = Instantiate(Config.OverridePopupCanvasPrefab);
                DontDestroyOnLoad(canvasPopupOverride); //Canvas are persistent because they can be reused 
                return canvasPopupOverride;
            }

            GameObject canvasPopupGo = new GameObject("[Canvas] - Popup");
            DontDestroyOnLoad(canvasPopupGo); //Canvas are persistent because they can be reused 

            Canvas canvasPopup = canvasPopupGo.AddComponent<Canvas>();
            CanvasScaler canvasScalerPopup = canvasPopupGo.AddComponent<CanvasScaler>();
            GraphicRaycaster graphicRaycasterPopup = canvasPopupGo.AddComponent<GraphicRaycaster>();

            //Copies the settings from the screen's canvas to the popup's canvas
            canvasPopup.renderMode = canvas.renderMode;
            canvasPopup.pixelPerfect = canvas.pixelPerfect;
            canvasPopup.sortingLayerID = canvas.sortingLayerID;
            canvasPopup.sortingOrder = canvas.sortingOrder;
            canvasPopup.targetDisplay = canvas.targetDisplay;
            canvasPopup.renderMode = canvas.renderMode;
            canvasPopup.additionalShaderChannels = canvas.additionalShaderChannels;
            canvasPopup.worldCamera = canvas.worldCamera;

            //Copies the settings from the screen's canvasScaler to the popup's canvasScaler
            canvasScalerPopup.uiScaleMode = canvasScaler.uiScaleMode;
            canvasScalerPopup.referenceResolution = canvasScaler.referenceResolution;
            canvasScalerPopup.screenMatchMode = canvasScaler.screenMatchMode;
            canvasScalerPopup.referencePixelsPerUnit = canvasScaler.referencePixelsPerUnit;
            canvasScalerPopup.scaleFactor = canvasScaler.scaleFactor;
            canvasScalerPopup.physicalUnit = canvasScaler.physicalUnit;
            canvasScalerPopup.fallbackScreenDPI = canvasScaler.fallbackScreenDPI;
            canvasScalerPopup.defaultSpriteDPI = canvasScaler.defaultSpriteDPI;

            //Copies the settings from the screen's graphicRaycaster to the popup's graphicRaycaster
            graphicRaycasterPopup.ignoreReversedGraphics = graphicRaycaster.ignoreReversedGraphics;
            graphicRaycasterPopup.blockingObjects = graphicRaycaster.blockingObjects;

            return canvasPopup;
        }

        private void CalculatePopupCanvasSortOrder(Canvas canvasPopup, PopupBase topOfStackPopupInstance)
        {
            if (canvasPopup != null)
            {
                if (topOfStackPopupInstance == null)
                {
                    canvasPopup.sortingOrder = canvas.sortingOrder + 1;
                }
                else
                {
                    if (allocatedPopupCanvas.TryGetValue(topOfStackPopupInstance, out Canvas currentPopupCanvas))
                    {
                        canvasPopup.sortingOrder = currentPopupCanvas.sortingOrder + 1;
                    }
                }

                canvasPopup.name = "[Canvas] - Popup #" + canvasPopup.sortingOrder;
            }
        }

        private void ProcessBackKey()
        {
            if (CurrentScreenConfig != null)
            {
                //check if the screen is overriding config back key behaviour
                if (CurrentScreenInstance.BackKeyBehaviourOverride == BackKeyBehaviourOverride.Inherit)
                {
                    switch (CurrentScreenConfig.BackKeyBehaviour)
                    {
                        case BackKeyBehaviour.NotAllowed: return;
                        case BackKeyBehaviour.ScreenMoveBack:
                            MoveBack();
                            break;
                        case BackKeyBehaviour.CloseFirstPopup:
                        {
                            if (CurrentPopupInstance != null)
                            {
                                CloseForegroundPopup();
                            }
                            else
                            {
                                MoveBack();
                            }

                            break;
                        }
                    }
                }
                else
                {
                    switch (CurrentScreenInstance.BackKeyBehaviourOverride)
                    {
                        case BackKeyBehaviourOverride.NotAllowed: return;
                        case BackKeyBehaviourOverride.ScreenMoveBack:
                            MoveBack();
                            break;
                        case BackKeyBehaviourOverride.CloseFirstPopup:
                        {
                            if (CurrentPopupInstance != null)
                            {
                                CloseForegroundPopup();
                            }
                            else
                            {
                                MoveBack();
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}