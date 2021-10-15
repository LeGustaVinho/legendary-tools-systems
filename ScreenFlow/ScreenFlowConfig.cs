using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LegendaryTools.Systems.ScreenFlow
{
    public enum AnimationType
    {
        NoAnimation,
        Wait,
        Intersection,
    }

    public enum BackKeyBehaviour
    {
        NotAllowed,
        ScreenMoveBack,
        CloseFirstPopup,
    }

    public enum PopupsBehaviour
    {
        PreserveAllOnHide,
        HideFirstThenTransit,
        DestroyAllThenTransit,
    }

    [CreateAssetMenu(menuName = "CustomTools/ScreenFlow/ScreenFlowConfig")]
    public class ScreenFlowConfig : ScriptableObject
    {
        public ScreenConfig[] Screens;
        public PopupConfig[] Popups;

        public Canvas OverridePopupCanvasPrefab;

#if UNITY_EDITOR
        [Header("Weaver (Editor Only)")] public string WeaverNamespace;
        public string WeaverClassname;

        public void FindConfigs()
        {
            Screens = ScreenFlowEditorUtils.FindAssetConfigNear<ScreenConfig>(this).ToArray();
            Popups = ScreenFlowEditorUtils.FindAssetConfigNear<PopupConfig>(this).ToArray();

            EditorUtility.SetDirty(this);
        }

        public void WeaverClass()
        {
            ScreenFlowEditorUtils.WeaverClassFor(this, true);
        }

#endif
    }
}