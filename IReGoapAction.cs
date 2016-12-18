using System;
using System.Collections.Generic;

public interface IReGoapAction
{
    GoapActionSettings GetSettings(IReGoapAgent goapAgent, ReGoapState goalState);
    void Run(IReGoapAction previousAction, IReGoapAction nextAction, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail);
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

public class GoapActionSettings
{
}