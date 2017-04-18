using System;
using System.Collections.Generic;

namespace ReGoap.Unity.FSM
{
    public interface ISmState
    {
        List<ISmTransition> Transitions { get; set; }

        void Enter();
        void Exit();
        void Init(StateMachine stateMachine);
        bool IsActive();

        int GetPriority();
    }

    public interface ISmTransition
    {
        Type TransitionCheck(ISmState state);
        int GetPriority();
    }

// you can inherit your FSM's transition from this, but feel free to implement your own (note: must implement ISmTransition and IComparable<ISmTransition>)
    public class SmTransition : ISmTransition, IComparable<ISmTransition>
    {
        private readonly int priority;
        private readonly Func<ISmState, Type> checkFunc;

        public SmTransition(int priority, Func<ISmState, Type> checkFunc)
        {
            this.priority = priority;
            this.checkFunc = checkFunc;
        }

        public Type TransitionCheck(ISmState state)
        {
            return checkFunc(state);
        }

        public int GetPriority()
        {
            return priority;
        }

        public int CompareTo(ISmTransition other)
        {
            return -GetPriority().CompareTo(other.GetPriority());
        }
    }
}