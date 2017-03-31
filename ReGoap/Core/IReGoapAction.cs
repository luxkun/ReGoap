using System;
using System.Collections.Generic;

public interface IReGoapAction
{
    // this should return current's action calculated parameter, will be added to the run method
    // userful for dynamic actions, for example a GoTo action can save some informations (wanted position)
    // while being chosen from the planner, we save this information and give it back when we run the method
    IReGoapActionSettings GetSettings(IReGoapAgent goapAgent, ReGoapState goalState);
    void Run(IReGoapAction previousAction, IReGoapAction nextAction, IReGoapActionSettings settings, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail);
    void Exit(IReGoapAction nextAction);
    Dictionary<string, object> GetGenericValues();
    string GetName();
    bool IsActive();
    void PostPlanCalculations(IReGoapAgent goapAgent);
    bool IsInterruptable();
    void AskForInterruption();
    // THREAD SAFE
    ReGoapState GetPreconditions(ReGoapState goalState, IReGoapAction next = null);
    ReGoapState GetEffects(ReGoapState goalState, IReGoapAction next = null);
    bool CheckProceduralCondition(IReGoapAgent goapAgent, ReGoapState goalState, IReGoapAction nextAction = null);
    float GetCost(ReGoapState goalState, IReGoapAction next = null);
    // DO NOT CHANGE RUNTIME ACTION VARIABLES, precalculation can be runned many times even while an action is running
    void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState);
}

public interface IReGoapActionSettings
{
}