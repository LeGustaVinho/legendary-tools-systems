using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace LegendaryTools.Systems.ScreenFlow.Editor
{
    public class ScreenFlowPreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            List<ScreenFlowConfig> screenFlowConfigs = ScreenFlowEditorUtils.FindAllScreenFlowConfigs();

            foreach (var screenFlowConfig in screenFlowConfigs)
            {
                screenFlowConfig.FindConfigs();
                ScreenFlowEditorUtils.WeaverClassFor(screenFlowConfig, true);
            }
        }
    }
}