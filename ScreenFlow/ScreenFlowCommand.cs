using System;
using Object = UnityEngine.Object;

namespace LegendaryTools.Systems.ScreenFlow
{
    public readonly struct ScreenFlowCommand
    {
        public readonly ScreenFlowCommandType Type;
        public readonly Object Object;
        public readonly System.Object Args;
        public readonly Action<ScreenBase> OnCompleted;

        public ScreenFlowCommand(ScreenFlowCommandType type, Object o, object args, Action<ScreenBase> onCompleted = null)
        {
            Type = type;
            Object = o;
            Args = args;
            OnCompleted = onCompleted;
        }
    }
}