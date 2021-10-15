using UnityEngine;

namespace LegendaryTools.Systems.ScreenFlow
{
    [CreateAssetMenu(menuName = "CustomTools/ScreenFlow/ScreenConfig")]
    public class ScreenConfig : UIEntityBaseConfig
    {
        [Header("Popups")] public bool AllowPopups;
        public bool AllowStackablePopups;

        [Header("Behaviour")] public bool CanMoveBackFromHere;
        public bool CanMoveBackToHere;
        public BackKeyBehaviour BackKeyBehaviour = BackKeyBehaviour.ScreenMoveBack;
        public PopupsBehaviour PopupBehaviour = PopupsBehaviour.HideFirstThenTransit;
    }
}