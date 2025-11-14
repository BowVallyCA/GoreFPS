using System;
using System.Collections.Generic;

namespace _Project.Code.Core.StateMachine
{
    public class FiniteStateMachine<T> where T : IState
    {
        private readonly Dictionary<Type, T> _states = new();
        
        public T CurrentState { get; protected set; }

        public FiniteStateMachine(T initialState)
        {
            if (initialState == null)
                throw new ArgumentNullException(nameof(initialState));
                
            AddState(initialState);
            CurrentState = initialState;
            CurrentState.Enter();
        }

        public void AddState(T state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
                
            var stateType = state.GetType();
            
            if (_states.ContainsKey(stateType))
                throw new InvalidOperationException($"State {stateType.Name} already exists in state machine");
                
            _states[stateType] = state;
        }

        public void TransitionTo<TState>() where TState : T
        {
            var stateType = typeof(TState);
            
            if (!_states.TryGetValue(stateType, out var nextState))
                throw new InvalidOperationException($"State {stateType.Name} not found in state machine");

            if ((object)CurrentState == (object)nextState)
                return;

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
        }

        public TState GetState<TState>() where TState : T
        {
            if (_states.TryGetValue(typeof(TState), out var state))
                return (TState)state;

            return default;
        }

        public virtual void Update() => CurrentState?.Update();
        
        public virtual void FixedUpdate() => CurrentState?.FixedUpdate();
    }
}