using UnityEngine;

namespace LegendaryTools.Systems.ScreenFlow
{
    //[CreateAssetMenu(menuName = "Tools/ScreenFlow/PopupConfig")]
    public abstract class PopupConfig : UIEntityBaseConfig
    {
        public PopupGoingBackgroundBehaviour GoingBackgroundBehaviour = PopupGoingBackgroundBehaviour.DontHide;
    }
}