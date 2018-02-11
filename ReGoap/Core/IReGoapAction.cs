using System;
using System.Collections.Generic;

namespace ReGoap.Core
{
    public struct GoapActionStackData<T, W>
    {
        public ReGoapState<T, W> currentState;
        public ReGoapState<T, W> goalState;
        public IReGoapAgent<T, W> agent;
        public IReGoapAction<T, W> next;
        public ReGoapState<T, W> settings;
    }

    public interface IReGoapAction<T, W>
    {
        // this should return current's action calculated parameter, will be added to the run method
        // userful for dynamic actions, for example a GoTo action can save some informations (wanted position)
        // while being chosen from the planner, we save this information and give it back when we run the method
        // most of actions would return a single item list, but more complex could return many items
        List<ReGoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData);
        void Run(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState, Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail);
        // Called when the action has been added inside a running Plan
        void PlanEnter(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState);
        // Called when the plan, which had this action, has either failed or completed
        void PlanExit(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState);
        void Exit(IReGoapAction<T, W> nextAction);
        string GetName();
        bool IsActive();
        bool IsInterruptable();
        void AskForInterruption();
        // MUST BE IMPLEMENTED AS THREAD SAFE
        ReGoapState<T, W> GetPreconditions(GoapActionStackData<T, W> stackData);
        ReGoapState<T, W> GetEffects(GoapActionStackData<T, W> stackData);
        bool CheckProceduralCondition(GoapActionStackData<T, W> stackData);
        float GetCost(GoapActionStackData<T, W> stackData);
        // DO NOT CHANGE RUNTIME ACTION VARIABLES, precalculation can be runned many times even while an action is running
        void Precalculations(GoapActionStackData<T, W> stackData);

        string ToString(GoapActionStackData<T, W> stackData);
    }
}