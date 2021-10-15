using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LegendaryTools.Systems.ScreenFlow.Editor
{
    [CustomEditor(typeof(UIScreenFlowTrigger))]
    [CanEditMultipleObjects]
    public class UIScreenFlowTriggerEditor : UnityEditor.Editor
    {
        private SerializedProperty triggerModeProperty;
        private SerializedProperty screenFlowConfigProperty;
        private SerializedProperty uiEntityProperty;
        private SerializedProperty enqueueProperty;
        
        private ScreenConfig[] screens;
        private PopupConfig[] popups;
        private UIEntityBaseConfig[] entities;
        private ScreenFlowConfig screenFlowConfig;
        private int uiEntityNameIndex;

        private bool IsPrefabMode => EditorSceneManager.IsPreviewScene((target as MonoBehaviour).gameObject.scene);

        private void OnEnable()
        {
            triggerModeProperty = serializedObject.FindProperty(nameof(UIScreenFlowTrigger.Mode));
            screenFlowConfigProperty = serializedObject.FindProperty(nameof(UIScreenFlowTrigger.ScreenFlowConfig));
            uiEntityProperty = serializedObject.FindProperty(nameof(UIScreenFlowTrigger.UiEntity));
            enqueueProperty = serializedObject.FindProperty(nameof(UIScreenFlowTrigger.Enqueue));

            Update();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(triggerModeProperty);

            ScreenFlowTriggerMode currentMode = (ScreenFlowTriggerMode) Enum.ToObject(typeof(ScreenFlowTriggerMode),
                triggerModeProperty.enumValueIndex);

            if (currentMode == ScreenFlowTriggerMode.Trigger)
            {
                if (ScreenFlow.Instance == null || IsPrefabMode)
                {
                    EditorGUILayout.PropertyField(screenFlowConfigProperty);
                }
            }

            if (uiEntityProperty.objectReferenceValue != null && entities != null)
            {
                int nameIndex = Array.FindIndex(entities, item => item == uiEntityProperty.objectReferenceValue);
                if (nameIndex >= 0)
                {
                    uiEntityNameIndex = nameIndex;
                }
            }

            if (currentMode == ScreenFlowTriggerMode.Trigger && screenFlowConfig != null)
            {
                uiEntityNameIndex = EditorGUILayout.Popup("Name", uiEntityNameIndex, GenerateUiEntityNames());
            }

            if (uiEntityNameIndex >= 0 && entities != null && entities.Length > 0)
            {
                uiEntityProperty.objectReferenceValue = entities[uiEntityNameIndex];
            }
            
            EditorGUILayout.PropertyField(enqueueProperty);

            serializedObject.ApplyModifiedProperties();

            Update();
        }

        private void Update()
        {
            if (ScreenFlow.Instance != null && !IsPrefabMode)
            {
                if (ScreenFlow.Instance.Config != null)
                {
                    screenFlowConfig = ScreenFlow.Instance.Config;
                }
            }
            else
            {
                screenFlowConfig = screenFlowConfigProperty.objectReferenceValue as ScreenFlowConfig;
            }

            if (screenFlowConfig != null)
            {
                screens = screenFlowConfig.Screens;
                popups = screenFlowConfig.Popups;
                GenerateUiEntityNames();
            }
        }

        private string[] GenerateUiEntityNames()
        {
            string[] names = new string[(screens?.Length ?? 0) + (popups?.Length ?? 0)];
            entities = new UIEntityBaseConfig[names.Length];

            int i = 0;
            if (screens != null)
            {
                foreach (var screen in screens)
                {
                    entities[i] = screen;
                    names[i] = "[Screen] " + screen.name;
                    i++;
                }
            }

            if (popups != null)
            {
                foreach (var popup in popups)
                {
                    entities[i] = popup;
                    names[i] = "[Popup] " + popup.name;
                    i++;
                }
            }

            return names;
        }
    }
}