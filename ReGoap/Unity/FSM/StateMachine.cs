using System;
using System.Collections.Generic;
using UnityEngine;

// simple FSM, feel free to use this or your own or unity animator's behaviour or anything you like with ReGoap
namespace ReGoap.Unity.FSM
{
    public class StateMachine : MonoBehaviour
    {
        private Dictionary<Type, ISmState> states;
        private Dictionary<string, object> values;
        private static Dictionary<string, object> globalValues;
        private List<ISmTransition> genericTransitions;

        public bool enableStackedStates;
        public Stack<ISmState> currentStates;

        public ISmState CurrentState
        {
            get
            {
                if (enableStackedStates)
                    return currentStates.Count == 0 ? null : currentStates.Peek();
                return currentState;
            }
        }

        private ISmState currentState;

        public MonoBehaviour initialState;

        public bool permitLoopTransition = true;

        public bool orderTransitions;

        void OnDisable()
        {
            if (CurrentState != null)
                CurrentState.Exit();
        }

        void Awake()
        {
            enabled = true;
            states = new Dictionary<Type, ISmState>();
            values = new Dictionary<string, object>();
            currentStates = new Stack<ISmState>();
            genericTransitions = new List<ISmTransition>();
            globalValues = new Dictionary<string, object>();
        }

        void Start()
        {
            foreach (var state in GetComponents<ISmState>())
            {
                AddState(state);
                var monoB = (MonoBehaviour) state;
                monoB.enabled = false;
            }
            Switch(initialState.GetType());
        }

        public void AddState(ISmState state)
        {
            state.Init(this);
            states[state.GetType()] = state;
        }

        public void AddGenericTransition(ISmTransition func)
        {
            genericTransitions.Add(func);
            if (orderTransitions)
                genericTransitions.Sort();
        }

        public T GetValue<T>(string key)
        {
            if (!HasValue(key))
                return default(T);
            return (T) values[key];
        }

        public bool HasValue(string key)
        {
            return values.ContainsKey(key);
        }

        public void SetValue<T>(string key, T value)
        {
            values[key] = value;
        }

        public void RemoveValue(string key)
        {
            values.Remove(key);
        }

        public static T GetGlobalValue<T>(string key)
        {
            return (T) globalValues[key];
        }

        public static bool HasGlobalValue(string key)
        {
            return globalValues.ContainsKey(key);
        }

        public static void SetGlobalValue<T>(string key, T value)
        {
            globalValues[key] = value;
        }

        void FixedUpdate()
        {
            Check();
        }

        void Check()
        {
            for (var index = genericTransitions.Count - 1; index >= 0; index--)
            {
                var trans = genericTransitions[index];
                var result = trans.TransitionCheck(CurrentState);
                if (result != null)
                {
                    Switch(result);
                    return;
                }
            }
            if (CurrentState == null) return;
            for (var index = CurrentState.Transitions.Count - 1; index >= 0; index--)
            {
                var trans = CurrentState.Transitions[index];
                var result = trans.TransitionCheck(CurrentState);
                if (result != null)
                {
                    Switch(result);
                    return;
                }
            }
        }

        public void Switch<T>() where T : MonoBehaviour, ISmState
        {
            Switch(typeof(T));
        }

        public void Switch(Type T)
        {
            if (CurrentState != null)
            {
                if (!permitLoopTransition && (CurrentState.GetType() == T)) return;
                ((MonoBehaviour) CurrentState).enabled = false;
                CurrentState.Exit();
            }
            if (enableStackedStates)
                currentStates.Push(states[T]);
            else
                currentState = states[T];
            ((MonoBehaviour) CurrentState).enabled = true;
            CurrentState.Enter();

            if (orderTransitions)
                CurrentState.Transitions.Sort();
        }

        public void PopState()
        {
            if (!enableStackedStates)
            {
                throw new UnityException(
                    "[StateMachine] Trying to pop a state from a state machine with disabled stacked states.");
            }
            currentStates.Peek().Exit();
            ((MonoBehaviour) currentStates.Pop()).enabled = false;
            ((MonoBehaviour) currentStates.Peek()).enabled = true;
            currentStates.Peek().Enter();
        }
    }
}