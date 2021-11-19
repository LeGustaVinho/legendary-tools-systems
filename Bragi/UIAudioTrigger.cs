using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LegendaryTools.Bragi
{
    public enum AudioTriggerType
    {
        PointerClick,
        PointerEnter,
        PointerExit,
        PointerUp,
        PointDown,
        
        Submit,
        Select,
        
        BeginDrag,
        Drag,
        Drop,
        EndDrag,
        
        TriggerEnter,
        TriggerStay,
        TriggerExit,
        
        Animator, //TODO
        Animation, //TODO
        
        Custom
    }

    public enum AudioTriggerPlayMode
    {
        Default,
        PlayAtThisLocation,
        PlayAndParent
    }

    [Serializable]
    public struct AudioConfigTrigger
    {
        public AudioTriggerType TriggerType;
        public AudioTriggerPlayMode PlayMode;
        public AudioConfigBase Config;
        public string Custom;
    }
    
    public class UIAudioTrigger : MonoBehaviour, 
        IPointerClickHandler, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler,
        ISelectHandler, ISubmitHandler
    {
        public AudioConfigTrigger[] AudioConfigTriggers;
        
        private Dictionary<AudioTriggerType, List<AudioConfigTrigger>> audioConfigTriggerTable = new Dictionary<AudioTriggerType, List<AudioConfigTrigger>>();
        private Dictionary<AudioTriggerType, AudioConfigTrigger> customAudioConfigTriggerTable = new Dictionary<AudioTriggerType, AudioConfigTrigger>();
        
        public void OnPointerClick(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.PointerClick);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.PointerEnter);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.PointDown);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.PointerUp);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.PointerExit);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.BeginDrag);
        }

        public void OnDrag(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.Drag);
        }

        public void OnDrop(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.Drop);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.EndDrag);
        }

        public void OnSelect(BaseEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.Select);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            ProcessTrigger(AudioTriggerType.Submit);
        }

        public virtual void OnTriggerEnter(Collider collider)
        {
            ProcessTrigger(AudioTriggerType.TriggerEnter);
        }
        
        public virtual void OnTriggerStay(Collider collider)
        {
            ProcessTrigger(AudioTriggerType.TriggerStay);
        }
        
        public virtual void OnTriggerExit(Collider collider)
        {
            ProcessTrigger(AudioTriggerType.TriggerExit);
        }

        public virtual void CustomTrigger(string triggerName)
        {
            ProcessTrigger(AudioTriggerType.Custom, triggerName);
        }

        protected virtual void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (AudioConfigTrigger audioConfigTrigger in AudioConfigTriggers)
            {
                if (!audioConfigTriggerTable.ContainsKey(audioConfigTrigger.TriggerType))
                {
                    audioConfigTriggerTable.Add(audioConfigTrigger.TriggerType, new List<AudioConfigTrigger>());
                }
                
                audioConfigTriggerTable[audioConfigTrigger.TriggerType].Add(audioConfigTrigger);

                if (audioConfigTrigger.TriggerType == AudioTriggerType.Custom)
                {
                    
                }
            }
        }

        protected virtual void ProcessTrigger(AudioTriggerType triggerType, string customString = "")
        {
            if (audioConfigTriggerTable.TryGetValue(triggerType, out List<AudioConfigTrigger> audioConfigTriggers))
            {
                if (string.IsNullOrEmpty(customString))
                {
                    foreach (AudioConfigTrigger audioConfigTrigger in audioConfigTriggers)
                    {
                        Play(audioConfigTrigger);
                    }
                }
                else
                {
                    int index = audioConfigTriggers.FindIndex(item => item.Custom == customString);
                    if (index >= 0)
                    {
                        Play(audioConfigTriggers[index]);
                    }
                }
            }
        }

        protected void Play(AudioConfigTrigger audioConfigTrigger)
        {
            switch (audioConfigTrigger.PlayMode)
            {
                case AudioTriggerPlayMode.Default:
                    audioConfigTrigger.Config.Play();
                    break;
                case AudioTriggerPlayMode.PlayAtThisLocation:
                    audioConfigTrigger.Config.Play(transform.position);
                    break;
                case AudioTriggerPlayMode.PlayAndParent:
                    audioConfigTrigger.Config.Play(transform);
                    break;
            }
        }
    }
}