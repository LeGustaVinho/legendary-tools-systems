using UnityEngine;
using UnityEngine.UI;

namespace LegendaryTools.Systems.ScreenFlow
{
    public enum ScreenFlowTriggerMode
    {
        Trigger,
        MoveBack,
        ClosePopup,
    }

    public class UIScreenFlowTrigger : MonoBehaviour
    {
        public ScreenFlowTriggerMode Mode = ScreenFlowTriggerMode.Trigger;

        public ScreenFlowConfig ScreenFlowConfig;
        public UIEntityBaseConfig UiEntity;
        public bool Enqueue;
        
        private Button button;

        public void ProcessTrigger()
        {
            if (ScreenFlow.Instance != null)
            {
                switch (Mode)
                {
                    case ScreenFlowTriggerMode.Trigger:
                    {
                        ScreenFlow.Instance.SendTrigger(UiEntity, enqueue:Enqueue);
                        break;
                    }
                    case ScreenFlowTriggerMode.MoveBack:
                    {
                        ScreenFlow.Instance.MoveBack(enqueue:Enqueue);
                        break;
                    }
                    case ScreenFlowTriggerMode.ClosePopup:
                    {
                        ScreenFlow.Instance.CloseForegroundPopup(enqueue:Enqueue);
                        break;
                    }
                }
            }
        }

        private void Start()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ProcessTrigger);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(ProcessTrigger);
            }
        }
    }
}
