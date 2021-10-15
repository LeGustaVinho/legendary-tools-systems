using System;

namespace LegendaryTools.Systems.ScreenFlow
{
    public abstract class PopupBase : ScreenBase
    {
        public event Action<PopupBase> OnClosePopupRequest;
        public ScreenBase ParentScreen;

        public virtual void OnGoToBackground(System.Object args)
        {

        }

        public virtual void ClosePopup()
        {
            OnClosePopupRequest?.Invoke(this);
        }
    }
}