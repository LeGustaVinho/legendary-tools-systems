using System.Collections.Generic;

namespace LegendaryTools.Systems.ScreenFlow
{
    public class StateMachineScreenFlow<TState, TTrigger> : ScreenFlow
    {
        public StateMachine<TState, TTrigger> StateMachine { private set; get; }
        public bool EnqueueStateTransition;
        protected Dictionary<State<TState, TTrigger>, string> stateMapping;

        public void SetStateMachine(StateMachine<TState, TTrigger> stateMachine,
            Dictionary<State<TState, TTrigger>, string> stateMapping)
        {
            if (stateMachine != null)
            {
                StateMachine = stateMachine;
                this.stateMapping = stateMapping;
                StateMachine.OnStateMachineTransit += OnStateMachineTransit;
            }
            else
            {
                if (StateMachine != null)
                {
                    StateMachine.OnStateMachineTransit -= OnStateMachineTransit;
                }

                StateMachine = null;
                this.stateMapping = null;
            }
        }

        private void OnStateMachineTransit(State<TState, TTrigger> newState, StateEventType eventType, object args)
        {
            if (eventType == StateEventType.Enter)
            {
                if (stateMapping.TryGetValue(newState, out string triggerName))
                {
                    SendTrigger(triggerName, args, EnqueueStateTransition);
                }
            }
        }
    }
}