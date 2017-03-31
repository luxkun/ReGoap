using System;
using System.Collections.Generic;

public interface IReGoapGoal
{
    void Run(Action<IReGoapGoal> callback);
    // THREAD SAFE METHODS (cannot use any unity library!)
    Queue<ReGoapActionState> GetPlan();
    string GetName();
    void Precalculations(IGoapPlanner goapPlanner);
    bool IsGoalPossible();
    ReGoapState GetGoalState();
    float GetPriority();
    void SetPlan(Queue<ReGoapActionState> path);
    float GetErrorDelay();
}