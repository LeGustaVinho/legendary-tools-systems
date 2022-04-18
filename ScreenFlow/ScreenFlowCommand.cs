using System;
using Object = UnityEngine.Object;

namespace LegendaryTools.Systems.ScreenFlow
{
    public readonly struct ScreenFlowCommand
    {
        public readonly ScreenFlowCommandType Type;
        public readonly Object Object;
        public readonly System.Object Args;
        public readonly Action<ScreenBase> OnShow;
        public readonly Action<ScreenBase> OnHide;

        public ScreenFlowCommand(ScreenFlowCommandType type, Object o, object args, Action<ScreenBase> onShow = null, Action<ScreenBase> onHide = null)
        {
            Type = type;
            Object = o;
            Args = args;
            OnShow = onShow;
            OnHide = onHide;
        }
    }
}