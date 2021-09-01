using System;
using System.Collections.Generic;
using Unity.Assertions;

namespace Core.States
{
    public delegate T StateFactory<out T>();
    
    public class StateMachine<TTrigger>
    {
        private readonly Dictionary<Type, IStateDefinition> _states = new Dictionary<Type,StateMachine<TTrigger>.IStateDefinition>();
        private readonly Dictionary<(Type, TTrigger), IStateDefinition> _transitions = new Dictionary<(Type, TTrigger), IStateDefinition>();
        private (IStateDefinition Definition, IState State) _activeState;

        public void Fire(TTrigger trigger)
        {
            var activeDefinition = _activeState.Definition;
            var transitionDefinition = (activeDefinition?.StateType, trigger);

            Assert.IsTrue(_transitions.ContainsKey(transitionDefinition), $"Transition from state {_activeState.State?.ToString() ?? "ROOT"} not found by trigger {trigger}");

            activeDefinition = _transitions[transitionDefinition];
            
            _activeState.State?.OnExit();
            _activeState = (activeDefinition, activeDefinition?.CreateState());
            _activeState.State?.OnEnter();
        }

        public void DefineState<TState>(StateFactory<TState> factory) where TState : IState
        {
            _states[typeof(TState)] = new StateMachine<TTrigger>.StateDefinition<TState>(factory);
        }

        public void DefineTransition<TState1, TState2>(TTrigger trigger)
            where TState1 : IState
            where TState2 : IState
        {
            var type1 = typeof(TState1);
            Assert.IsTrue(_states.ContainsKey(type1), $"State {type1} not defined");
            
            var type2 = typeof(TState2);
            Assert.IsTrue(_states.ContainsKey(type2), $"State {type2} not defined");

            _transitions[(type1, trigger)] = _states[type2];
        }
        
        public void DefineStartTransition<TState>(TTrigger trigger)
            where TState : IState
        {
            var type = typeof(TState);
            Assert.IsTrue(_states.ContainsKey(type), $"State {type} not defined");

            _transitions[(null, trigger)] = _states[type];
        }
        
        private interface IStateDefinition
        {
            IState CreateState();
            Type StateType { get; }
        }
        
        private class StateDefinition<TState> : IStateDefinition where TState : IState
        {
            private readonly StateFactory<TState> _factory;

            public StateDefinition(StateFactory<TState> factory)
            {
                _factory = factory;
            }

            public IState CreateState()
            {
                return _factory();
            }

            public Type StateType => typeof(TState);
        }
    }
}