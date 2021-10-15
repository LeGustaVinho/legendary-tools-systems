using System.Collections.Generic;
using UnityEditor;

namespace LegendaryTools.Systems.ScreenFlow.Editor
{
    [InitializeOnLoadAttribute]
    public static class ScreenFlowHooks
    {
        static ScreenFlowHooks()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange newState)
        {
            switch (newState)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                {
                    List<ScreenFlowConfig> screenFlowConfigs = ScreenFlowEditorUtils.FindAllScreenFlowConfigs();

                    foreach (var screenFlowConfig in screenFlowConfigs)
                    {
                        screenFlowConfig.FindConfigs();
                        ScreenFlowEditorUtils.WeaverClassFor(screenFlowConfig, true);
                    }

                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();

                    break;
                }
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }
    }
}