using UnityEditor;
using UnityEngine;

namespace LegendaryTools.Systems.ScreenFlow.Editor
{
    [CustomEditor(typeof(ScreenFlowConfig))]
    [CanEditMultipleObjects]
    public class ScreenFlowConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button(nameof(ScreenFlowConfig.FindConfigs)))
            {
                (target as ScreenFlowConfig).FindConfigs();
            }

            if (GUILayout.Button(nameof(ScreenFlowConfig.WeaverClass)))
            {
                (target as ScreenFlowConfig).WeaverClass();
            }
        }
    }
}