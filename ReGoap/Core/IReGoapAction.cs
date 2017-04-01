using System;
using System.Collections.Generic;

public interface IReGoapAction<T, W>
{
    // this should return current's action calculated parameter, will be added to the run method
    // userful for dynamic actions, for example a GoTo action can save some informations (wanted position)
    // while being chosen from the planner, we save this information and give it back when we run the method
    IReGoapActionSettings<T, W> GetSettings(IReGoapAgent<T, W> goapAgent, ReGoapState<T, W> goalState);
    void Run(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, IReGoapActionSettings<T, W> settings, ReGoapState<T, W> goalState, Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail);
    void Exit(IReGoapAction<T, W> nextAction);
    Dictionary<string, object> GetGenericValues();
    string GetName();
    bool IsActive();
    void PostPlanCalculations(IReGoapAgent<T, W> goapAgent);
    bool IsInterruptable();
    void AskForInterruption();
    // THREAD SAFE
    ReGoapState<T, W> GetPreconditions(ReGoapState<T, W> goalState, IReGoapAction<T, W> next = null);
    ReGoapState<T, W> GetEffects(ReGoapState<T, W> goalState, IReGoapAction<T, W> next = null);
    bool CheckProceduralCondition(IReGoapAgent<T, W> goapAgent, ReGoapState<T, W> goalState, IReGoapAction<T, W> nextAction = null);
    float GetCost(ReGoapState<T, W> goalState, IReGoapAction<T, W> next = null);
    // DO NOT CHANGE RUNTIME ACTION VARIABLES, precalculation can be runned many times even while an action is running
    void Precalculations(IReGoapAgent<T, W> goapAgent, ReGoapState<T, W> goalState);
}

public interface IReGoapActionSettings<T, W>
{
}