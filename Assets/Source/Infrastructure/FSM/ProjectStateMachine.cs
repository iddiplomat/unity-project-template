using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Source.Infrastructure.FSM
{
    public class ProjectStateMachine : IProjectStateMachine
    {
        private readonly Dictionary<Type, IProjectState> _states = new();
        private readonly Queue<IProjectState> _queue = new();

        private IProjectState _currentState;
        private bool _isTransitioning;

        public void AddState<TState>(TState state) where TState : IProjectState
        {
            _states.Add(typeof(TState), state);
        }

        public async UniTask Enter<TState>() where TState : IProjectState
        {
            if (!_states.TryGetValue(typeof(TState), out var state))
            {
                Debug.LogError($"{typeof(TState).Name} state not found!");
                return;
            }

            if (_isTransitioning)
            {
                Debug.Log($"[FSM] Transition in progress. Queued: {typeof(TState).Name} (queue size: {_queue.Count + 1})");
                _queue.Enqueue(state);
                return;
            }

            await TransitionTo(state);
        }

        private async UniTask TransitionTo(IProjectState state)
        {
            var stateName = state.GetType().Name;
            _isTransitioning = true;

            if (_currentState != null)
            {
                var previousName = _currentState.GetType().Name;
                Debug.Log($"[FSM] Exiting: {previousName}");
                await _currentState.Exit();
                Debug.Log($"[FSM] Exited: {previousName}");
            }

            _currentState = state;
            Debug.Log($"[FSM] Entering: {stateName}");
            await _currentState.Enter();
            Debug.Log($"[FSM] Entered: {stateName}");

            _isTransitioning = false;

            if (_queue.Count > 0)
            {
                var next = _queue.Dequeue();
                Debug.Log($"[FSM] Processing queued: {next.GetType().Name} (remaining: {_queue.Count})");
                await TransitionTo(next);
            }
        }
    }
}