using System;
using System.Collections.Generic;

public interface IReGoapAction
{
    GoapActionSettings GetSettings(IReGoapAgent goapAgent, ReGoapState goalState);
    void Run(IReGoapAction previousAction, IReGoapAction nextAction, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail);
    void Exit(IReGoapAction nextAction);
    Dictionary<string, object> GetGenericValues(); // ex. "target": Transform for goto
    string GetName();
    bool IsActive();
    void PostPlanCalculations(IReGoapAgent goapAgent);
    bool IsInterruptable();
    void AskForInterruption();
    // THREAD SAFE
    ReGoapState GetEffects(ReGoapState goalState);
    bool CheckProceduralCondition(IReGoapAgent goapAgent, ReGoapState goalState);
    ReGoapState GetPreconditions(ReGoapState goalState);
    int GetCost(ReGoapState goalState);
    // DO NOT CHANGE RUNTIME ACTION VARIABLES, precalculation can be runned many times even while an action is running
    void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState);
}

public class GoapActionSettings
{
}